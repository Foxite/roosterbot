using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[Group("klas"), RoosterBot.Attributes.LogTag("StudentSM")]
	public class StudentScheduleModule : ScheduleModuleBase<StudentSetInfo> {
		[Command("nu", RunMode = RunMode.Async), Summary("Welke les een klas nu heeft")]
		public async Task StudentCurrentCommand(string klas) {
			ReturnValue<ScheduleRecord> result = await GetRecord(false, new StudentSetInfo() { ClassName = klas.ToUpper() });
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
					ReplyDeferred(response, new StudentSetInfo() { ClassName = klas.ToUpper() }, record);

					if (record.Activity == "pauze") {
						await GetAfterCommandFunction();
					}
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen"), Summary("Welke les een klas hierna heeft")]
		public async Task StudentNextCommand(string klas) {
			ReturnValue<ScheduleRecord> result = await GetRecord(true, new StudentSetInfo() { ClassName = klas.ToUpper() });
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
					ReplyDeferred(response, new StudentSetInfo() { ClassName = klas.ToUpper() }, record);

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
				ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(new StudentSetInfo() { ClassName = clazz.ToUpper() }, day, arguments.Item4);
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
						await ReplyAsync(response, new StudentSetInfo() { ClassName = clazz.ToUpper() }, null);
					} else {
						response = $"{clazz.ToUpper()}: Rooster ";
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
							cells[recordIndex][1] = $"{record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}";
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
		public async Task StudentTomorrowCommand(string klas) {
			await StudentWeekdayCommand(klas + " morgen");
		}

		[Command("vandaag", RunMode = RunMode.Async), Summary("Je rooster voor vandaag")]
		public async Task StudentTodayCommand(string klas) {
			await StudentWeekdayCommand(klas + " vandaag");
		}
	}
}
