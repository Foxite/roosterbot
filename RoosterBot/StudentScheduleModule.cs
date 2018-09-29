using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	public class StudentScheduleModule : ScheduleModuleBase {
		public StudentScheduleModule(ScheduleService serv, ConfigService config) : base(serv, config, "SSM") { }

		[Command("nu", RunMode = RunMode.Async)]
		public async Task CurrentCommand(string clazz) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(false, "StudentSets", clazz);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = "Het ziet ernaar uit dat je nu niets hebt.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					await ReplyAsync(response);
				} else {
					string response = "Je hebt nu ";
					if (record.Activity == "pauze") {
						response += $"pauze tot {record.End.ToShortTimeString()}.";
					}
					response += $"{record.Activity} van {GetTeacherNameFromAbbr(record.StaffMember)} in {record.Room}\n";
					TimeSpan actualDuration = record.End - record.Start;
					string[] givenDuration = record.Duration.Split(":");
					response += $"Dit is begonnen om {record.Start.ToShortTimeString()} en eindigd om {record.End.ToShortTimeString()}. Dit duurt dus {record.Duration}.\n";
					if (!(actualDuration.Hours == int.Parse(givenDuration[0]) && actualDuration.Minutes == int.Parse(givenDuration[1]))) {
						response += $"Tenminste, dat staat er, maar volgens mijn berekeningen is dat complete onzin en duurt de les eigenlijk {actualDuration.Hours}:{actualDuration.Minutes}.\n";
					}
					await ReplyAsync(response);
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async)]
		public async Task NextCommand(string clazz) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(true, "StudentSets", clazz);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					await FatalError("GetRecord(SS1)==null)");
				} else {
					string response = "Je hebt hierna ";
					if (record.Activity == "pauze") {
						response += $"pauze van {record.Start.ToShortTimeString()} tot {record.End.ToShortTimeString()}.";
					} else {
						response += record.Activity;
						if (!string.IsNullOrEmpty(record.StaffMember)) {
							response += $" van {GetTeacherNameFromAbbr(record.StaffMember)}";
						}
						if (!string.IsNullOrEmpty(record.Room)) {
							response += $" in {record.Room}.\n";
						} else {
							response += ", maar ik weet niet waar.\n";
						}
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
	}
}
