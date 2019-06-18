using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using RoosterBot.Attributes;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[Group("klas"), LogTag("StudentSM"), HiddenFromList]
	public class StudentScheduleModule : ScheduleModuleBase<StudentSetInfo> {
		[Command("nu", RunMode = RunMode.Async), Summary("Welke les een klas nu heeft")]
		public async Task StudentCurrentCommand(string klas = "ik") {
			(StudentSetInfo, bool) meResult = await ResolveMeQuery(klas);
			StudentSetInfo info = meResult.Item1;
			if (info == null) {
				if (meResult.Item2) {
					return;
				} else {
					info = new StudentSetInfo() { ClassName = klas.ToUpper() };
				}
			}
			ReturnValue<ScheduleRecord> result = await GetRecord(false, info);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				string response;
				if (record == null) {
					response = "Het ziet ernaar uit dat je nu niets hebt.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					await ReplyAsync(response, new StudentSetInfo() { ClassName = klas.ToUpper() }, null);
				} else {
					response = $"{record.StudentSetsString}: Nu\n";
					response += TableItemActivity(record, false);

					if (record.Activity != "stdag doc") {
						if (record.Activity != "pauze") {
							response += TableItemStaffMember(record);
							response += TableItemRoom(record);
						}

						response += TableItemStartEndTime(record);
						response += TableItemDuration(record);
						response += TableItemBreak(record);
					}
					ReplyDeferred(response, info, record);

					if (record.Activity == "pauze") {
						await GetAfterCommandFunction();
					}
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen"), Summary("Welke les een klas hierna heeft")]
		public async Task StudentNextCommand(string klas = "ik") {
			(StudentSetInfo, bool) meResult = await ResolveMeQuery(klas);
			StudentSetInfo info = meResult.Item1;
			if (info == null) {
				if (meResult.Item2) {
					return;
				} else {
					info = new StudentSetInfo() { ClassName = klas.ToUpper() };
				}
			}
			ReturnValue<ScheduleRecord> result = await GetRecord(true, info);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					await FatalError($"`GetRecord(true, \"StudentSets\", {klas})` returned null");
				} else {
					bool isToday = record.Start.Date == DateTime.Today;
					string response;

					if (isToday) {
						response = $"{record.StudentSetsString}: Hierna\n";
					} else {
						response = $"{record.StudentSetsString}: Als eerste op {Util.GetStringFromDayOfWeek(record.Start.DayOfWeek)}\n";
					}
					
					response += TableItemActivity(record, false);

					if (record.Activity != "stdag doc") {
						if (record.Activity != "pauze") {
							response += TableItemStaffMember(record);
							response += TableItemRoom(record);
						}
						
						response += TableItemStartEndTime(record);
						response += TableItemDuration(record);
						response += TableItemBreak(record);
					}
					ReplyDeferred(response, info, record);

					if (record.Activity == "pauze") {
						await GetAfterCommandFunction();
					}
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async), Summary("Welke les je als eerste hebt op een dag")]
		public async Task StudentWeekdayCommand([Remainder] string klas_en_weekdag) {
			Tuple<bool, DayOfWeek, string, bool> arguments = await GetValuesFromArguments(klas_en_weekdag);

			if (arguments.Item1) {
				DayOfWeek day = arguments.Item2;
				string clazz = arguments.Item3;
				(StudentSetInfo, bool) meResult = await ResolveMeQuery(clazz);
				StudentSetInfo info = meResult.Item1;
				if (info == null) {
					if (meResult.Item2) {
						return;
					} else {
						info = new StudentSetInfo() { ClassName = clazz.ToUpper() };
					}
				}
				ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(info, day, arguments.Item4);
				if (result.Success) {
					ScheduleRecord[] records = result.Value;
					string response;
					if (records.Length == 0) {
						response = "Het ziet ernaar uit dat je ";

						if (DateTime.Today.DayOfWeek == day && arguments.Item4) {
							response += "vandaag";
						} else if (DateTime.Today.AddDays(1).DayOfWeek == day) {
							response += "morgen";
						} else {
							response += "op " + Util.GetStringFromDayOfWeek(day);
						}

						response += " niets hebt.";
						if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) {
							response += " Het is dan ook weekend.";
						}
						await ReplyAsync(response, info, null);
					} else {
						response = $"{info.DisplayText}: Rooster ";
						if (DateTime.Today.DayOfWeek == day && arguments.Item4) {
							response += "voor vandaag";
						} else if (DateTime.Today.AddDays(1).DayOfWeek == day) {
							response += "voor morgen";
						} else {
							response += "op " + Util.GetStringFromDayOfWeek(day);
						}
						response += "\n";

						string[][] cells = new string[records.Length + 1][];
						cells[0] = new string[] { "Activiteit", "Tijd", "Leraar", "Lokaal" };
						int recordIndex = 1;
						foreach (ScheduleRecord record in records) {
							cells[recordIndex] = new string[4];
							cells[recordIndex][0] = record.Activity;
							cells[recordIndex][1] = $"{record.Start.ToString("HH:mm")} - {record.End.ToString("HH:mm")}";
							cells[recordIndex][2] = record.StaffMember.Length == 0 ? "" : string.Join(", ", record.StaffMember.Select(t => t.DisplayText));

							string room = record.RoomString;
							if (room.Contains(',')) {
								room = Util.FormatStringArray(room.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries), " en ");
							}
							cells[recordIndex][3] = room;
							recordIndex++;
						}
						response += Util.FormatTextTable(cells, true);
						ReplyDeferred(response, new StudentSetInfo() { ClassName = clazz.ToUpper() }, records.Last());
					}
				}
			}
		}

		[Command("morgen", RunMode = RunMode.Async), Summary("Welke les je morgen als eerste hebt")]
		public async Task StudentTomorrowCommand(string klas = "ik") {
			await StudentWeekdayCommand(klas + " morgen");
		}

		[Command("vandaag", RunMode = RunMode.Async), Summary("Je rooster voor vandaag")]
		public async Task StudentTodayCommand(string klas = "ik") {
			await StudentWeekdayCommand(klas + " vandaag");
		}
		
		/// <summary>
		/// Resolves a query for a mentioned user or the user themselves.
		/// </summary>
		/// <param name="input"></param>
		/// <returns>1. The StudentSetInfo that was resolved (might be null), 2. If a user was mentioned or "ik" was used (no additional error response should be given if this Item1 is null)</returns>
		protected async Task<(StudentSetInfo, bool)> ResolveMeQuery(string input) {
			if (input == "ik") {
				StudentSetInfo result = await Classes.GetClassForDiscordUser(Context.User);
				if (result == null) {
					ReplyDeferred("Ik weet niet in welke klas jij zit. Gebruik `!ik <jouw klas>` om dit in te stellen.");
				}
				return (result, true);
			} else {
				ulong? mentionedId = Util.ExtractIDFromMentionString(input);
				if (mentionedId.HasValue) {
					StudentSetInfo result = await Classes.GetClassForDiscordUser(mentionedId.Value);
					if (result == null) {
						ReplyDeferred("Ik weet niet in welke klas die persoon zit. Hij/zij moet `!ik <zijn/haar klas>` gebruiken om dit in te stellen.");
					}
					return (result, true);
				} else {
					return (null, false);
				}
			}
		}

	}
}
