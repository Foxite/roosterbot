using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot {
	public class TeacherScheduleModule : ScheduleModuleBase {
		public TeacherScheduleModule(ScheduleService serv, ConfigService config) : base(serv, config, "TSM") { }

		[Command("leraarnu", RunMode = RunMode.Async)]
		public async Task TeacherCurrentCommand([Remainder] string teacher) {
			if (!await CheckCooldown())
				return;

			teacher = GetTeacherAbbrFromName(teacher);

			if (teacher == null) {
				await Context.Message.AddReactionAsync(new Emoji("❌"));
				await ReplyAsync("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				if (teacher == "MJA, MKU") { // There are multiple
					string response = "";
					response += "\"Martijn\" kan Martijn Jacobs of Martijn Kunstman zijn.\n";
					response += RespondTeacherCurrent("Martijn Jacobs", Service.GetCurrentRecord("StaffMember", "MJA")) + "\n";
					response += RespondTeacherCurrent("Martijn Kunstman", Service.GetCurrentRecord("StaffMember", "MKU")) + "\n";
					response += "Hou er rekening mee dat ik niet weet of een leraar ziek is, of om een andere reden weg is.";
					await ReplyAsync(response);
				} else {
					ReturnValue<ScheduleRecord> result = await GetRecord(false, "StaffMember", teacher);
					if (result.Success) {
						ScheduleRecord record = result.Value;
						await ReplyAsync(RespondTeacherCurrent(GetTeacherNameFromAbbr(teacher), record));
					}
				}
			}
		}


		[Command("leraarhierna", RunMode = RunMode.Async)]
		public async Task TeacherNextCommand([Remainder] string teacher) {
			if (!await CheckCooldown())
				return;

			teacher = GetTeacherAbbrFromName(teacher);
			if (teacher == null) {
				await Context.Message.AddReactionAsync(new Emoji("❌"));
				await ReplyAsync("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				if (teacher == "MJA, MKU") { // There are multiple
					string response = "";
					response += "\"Martijn\" kan Martijn Jacobs of Martijn Kunstman zijn.\n";
					response += RespondTeacherNext("Martijn Jacobs", Service.GetNextRecord("StaffMember", "MJA")) + "\n";
					response += RespondTeacherNext("Martijn Kunstman", Service.GetNextRecord("StaffMember", "MKU")) + "\n";
					response += "Hou er rekening mee dat ik niet weet of een leraar ziek is, of om een andere reden weg is.";
					await ReplyAsync(response);
				} else {
					ReturnValue<ScheduleRecord> result = await GetRecord(true, "StaffMember", teacher);
					if (result.Success) {
						ScheduleRecord record = result.Value;
						if (record != null) {
							await RespondTeacherNext(GetTeacherNameFromAbbr(teacher), record);
						} else {
							await Context.Message.AddReactionAsync(new Emoji("⛔"));
							await ReplyAsync("Ik weet niet wat, maar er is iets gloeiend misgegaan. Probeer het later nog eens? Dat moet ik zeggen van mijn maker, maar volgens mij gaat het niet werken totdat hij het fixt. Sorry.\n" +
								$"{(await Context.Client.GetUserAsync(133798410024255488)).Mention} FIX IT! (GetRecord(TS1)==null)");
						}
					}
				}
			}
		}

		// This is a seperate function because we two teachers have the same name. We would have to write this function three times in TeacherCurrentCommand().
		private string RespondTeacherCurrent(string teacher, ScheduleRecord record) {
			if (record == null) {
				string response = $"Het ziet ernaar uit dat {teacher} nu niets heeft.";
				if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
					response += " Het is dan ook weekend.";
				}
				return response;
			} else {
				string response = "";
				if (record.Activity == "pauze") {
					response += $"{teacher} geeft nu pauze van {record.Start.ToShortTimeString()} tot {record.End.ToShortTimeString()}.";
				} else {
					response += $"{teacher} geeft nu ";
					response += record.Activity + (record.StudentSets != "" ? $" aan {record.StudentSets}" : "") + (record.Room != "" ? $" in {record.Room}" : ", en ik weet niet waar") + ".\n";
					TimeSpan actualDuration = record.End - record.Start;
					string[] givenDuration = record.Duration.Split(":");
					response += $"Dit is begonnen om {record.Start.ToShortTimeString()} en eindigd om {record.End.ToShortTimeString()}. Dit duurt dus {record.Duration}.\n";
					if (!(actualDuration.Hours == int.Parse(givenDuration[0]) && actualDuration.Minutes == int.Parse(givenDuration[1]))) {
						response += $"Tenminste, dat staat er, maar volgens mijn berekeningen is dat complete onzin en duurt de les eigenlijk {actualDuration.Hours}:{actualDuration.Minutes}.\n";
					}
				}
				return response;
			}
		}

		private async Task RespondTeacherNext(string teacher, ScheduleRecord record) {
			string response = teacher + " geeft hierna ";
			if (record.Activity == "pauze") {
				response += $"pauze van {record.Start.ToShortTimeString()} tot {record.End.ToShortTimeString()}.";
			} else {
				response += record.Activity + (record.StudentSets != "" ? $" aan {record.StudentSets}" : "") + (record.Room != "" ? $" in {record.Room}" : ", en ik weet niet waar") + ".\n";
				TimeSpan actualDuration = record.End - record.Start;
				string[] givenDuration = record.Duration.Split(":");
				if (record.Start.Day == DateTime.Today.Day) {
					response += $"Dit begint om {record.Start.ToShortTimeString()} en eindigd om {record.End.ToShortTimeString()}. Dit duurt dus {record.Duration}.\n";
				} else {
					response += $"Dit begint morgen om {record.Start.ToShortTimeString()} en eindigd om {record.End.ToShortTimeString()}. Dit duurt dus {record.Duration}.\n";
				}

				if (!(actualDuration.Hours == int.Parse(givenDuration[0]) && actualDuration.Minutes == int.Parse(givenDuration[1]))) {
					response += $"Tenminste, dat staat er, maar volgens mijn berekeningen is dat complete onzin en duurt de les eigenlijk {actualDuration.Hours}:{actualDuration.Minutes}.\n";
				}
			}
			await ReplyAsync(response);
		}
	}
}
