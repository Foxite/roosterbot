using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[Group("lokaal"), RoosterBot.Attributes.LogTag("RoomSM")]
	public class RoomScheduleModule : ScheduleModuleBase<RoomInfo> {
		[Command("nu", RunMode = RunMode.Async), Summary("Wat er nu in een lokaal plaatsvindt")]
		private async Task RoomCurrentCommand(string lokaal) {
			ReturnValue<ScheduleRecord> result = await GetRecord(false, new RoomInfo() { Room = lokaal.ToUpper() });
			if (result.Success) {
				ScheduleRecord record = result.Value;
				string response;
				if (record == null) {
					response = "Het ziet ernaar uit dat daar nu niets is.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
				} else {
					response = $"{record.RoomString}: Nu\n";
					response += TableItemActivity(record, false);

					if (record.Activity != "stdag doc") {
						response += TableItemStaffMember(record);
						response += TableItemStudentSets(record);
						response += TableItemStartEndTime(record);
						response += TableItemDuration(record);
						response += TableItemBreak(record);
					}
				}
				ReplyDeferred(response, new RoomInfo() { Room = lokaal.ToUpper() }, record);
				
				if (record?.Activity == "pauze") {
					await GetAfterCommandFunction();
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen"), Summary("Wat er hierna in een lokaal plaatsvindt")]
		private async Task RoomNextCommand(string lokaal) {
			ReturnValue<ScheduleRecord> result = await GetRecord(true, new RoomInfo() { Room = lokaal.ToUpper() });
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					await FatalError($"`GetRecord(true, \"Room\", {lokaal})` returned null");
				} else {
					bool isToday = record.Start.Date == DateTime.Today;
					string response;

					if (isToday) {
						response = $"{record.RoomString}: Hierna\n";
					} else {
						response = $"{record.RoomString}: Als eerste op {Util.GetStringFromDayOfWeek(record.Start.DayOfWeek)}\n";
					}

					response += TableItemActivity(record, false);

					if (record.Activity != "stdag doc") {
						response += TableItemStaffMember(record);
						response += TableItemStudentSets(record);
						response += TableItemStartEndTime(record);
						response += TableItemDuration(record);
						response += TableItemBreak(record);
					}

					ReplyDeferred(response, new RoomInfo() { Room = lokaal.ToUpper() }, record);
					
					if (record.Activity == "pauze") {
						await GetAfterCommandFunction();
					}
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async), Summary("Welke les er als eerste in een lokaal op een dag")]
		public async Task RoomWeekdayCommand([Remainder] string lokaal_en_weekdag) {
			Tuple<bool, DayOfWeek, string> arguments = await GetValuesFromArguments(lokaal_en_weekdag);

			if (arguments.Item1) {
				DayOfWeek day = arguments.Item2;
				string lokaal = arguments.Item3.ToUpper();

				ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(new RoomInfo() { Room = lokaal.ToUpper() }, day);
				if (result.Success) {
					ScheduleRecord[] records = result.Value;
					string response;
					if (records.Length == 0) {
						response = "Het ziet ernaar uit dat daar vandaag niets is.";
						if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
							response += " Het is dan ook weekend.";
						}
						ReplyDeferred(response, new RoomInfo() { Room = lokaal.ToUpper() }, null);
					} else {
						response = $"{lokaal.ToUpper()}: Rooster voor ";
						if (DateTime.Today.DayOfWeek == day) {
							response += "vandaag";
						} else {
							response += Util.GetStringFromDayOfWeek(day);
						}
						response += "\n";

						string[][] cells = new string[records.Length + 1][];
						cells[0] = new string[] { "Activiteit", "Tijd", "Klas", "Leraar" };
						int recordIndex = 1;
						foreach (ScheduleRecord record in records) {
							cells[recordIndex] = new string[4];
							cells[recordIndex][0] = record.Activity;
							cells[recordIndex][1] = $"{record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}";
							cells[recordIndex][2] = record.StudentSetsString;
							cells[recordIndex][3] = record.StaffMember.Length == 0 ? "" : string.Join(", ", record.StaffMember.Select(t => t.DisplayText));
							recordIndex++;
						}
						response += Util.FormatTextTable(cells, true);
						ReplyDeferred(response, new RoomInfo() { Room = lokaal.ToUpper() }, records.Last());
					}
				}
			}
		}

		[Command("morgen", RunMode = RunMode.Async), Summary("Welke les er morgen als eerste in een lokaal is")]
		public async Task RoomTomorrowCommand(string lokaal) {
			await RoomWeekdayCommand(lokaal + " " + Util.GetStringFromDayOfWeek(DateTime.Today.AddDays(1).DayOfWeek));
		}

		[Command("vandaag", RunMode = RunMode.Async), Summary("Het rooster voor een lokaal voor vandaag")]
		public async Task RoomTodayCommand(string lokaal) {
			await RoomWeekdayCommand(lokaal + " " + Util.GetStringFromDayOfWeek(DateTime.Today.DayOfWeek));
		}
	}
}
