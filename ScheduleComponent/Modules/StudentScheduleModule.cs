using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using RoosterBot.Attributes;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[LogTag("StudentSM"), HiddenFromList]
	public class StudentScheduleModule : ScheduleModuleBase {
		[Command("nu", RunMode = RunMode.Async)]
		public async Task StudentCurrentCommand(StudentSetInfo info) {
			ReturnValue<ScheduleRecord> result = await GetRecord(false, info);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				string response;
				if (record == null) {
					response = "Het ziet ernaar uit dat je nu niets hebt.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					await ReplyAsync(response, info, null);
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
						await GetAfterCommand();
					}
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen")]
		public async Task StudentNextCommand(StudentSetInfo info) {
			ReturnValue<ScheduleRecord> result = await GetRecord(true, info);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					await FatalError($"`GetRecord(true, \"StudentSets\", {info.DisplayText})` returned null");
				} else {
					bool isToday = record.Start.Date == DateTime.Today;
					string response;

					if (isToday) {
						response = $"{record.StudentSetsString}: Hierna\n";
					} else {
						response = $"{record.StudentSetsString}: Als eerste op {ScheduleUtil.GetStringFromDayOfWeek(record.Start.DayOfWeek)}\n";
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
						await GetAfterCommand();
					}
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async)]
		public async Task StudentWeekdayCommand(StudentSetInfo info, DayOfWeek day) {
			await RespondDay(info, day, false);
		}

		[Command("dag", RunMode = RunMode.Async)]
		public async Task StudentWeekdayCommand(DayOfWeek day, StudentSetInfo info) {
			await RespondDay(info, day, false);
		}

		[Command("vandaag", RunMode = RunMode.Async)]
		public async Task StudentTodayCommand(StudentSetInfo info) {
			await RespondDay(info, ScheduleUtil.GetDayOfWeekFromString("vandaag"), true);
		}

		[Command("morgen", RunMode = RunMode.Async)]
		public async Task StudentTomorrowCommand(StudentSetInfo info) {
			await RespondDay(info, ScheduleUtil.GetDayOfWeekFromString("morgen"), true);
		}

		[Command("deze week", RunMode = RunMode.Sync)]
		public Task ShowThisWeekWorkingDaysCommand(StudentSetInfo info) {
			AvailabilityInfo[] days = Schedules.GetWeekAvailability(info, 0);
			RespondWorkingDays(info, days, 0);
			return Task.CompletedTask;
		}

		[Command("volgende week", RunMode = RunMode.Sync)]
		public Task ShowNextWeekWorkingDaysCommand(StudentSetInfo info) {
			AvailabilityInfo[] days = Schedules.GetWeekAvailability(info, 1);
			RespondWorkingDays(info, days, 1);
			return Task.CompletedTask;
		}

		private void RespondWorkingDays(StudentSetInfo info, AvailabilityInfo[] availability, int weeksFromNow) {
			string response = info.DisplayText + ": ";

			if (availability.Length > 0) {
				if (weeksFromNow == 0) {
					response += "Deze week";
				} else if (weeksFromNow == 1) {
					response += "Volgende week";
				} else {
					response += $"Over {weeksFromNow} weken";
				}
				response += " op school op \n";
				
				string[][] cells = new string[availability.Length + 1][];
				cells[0] = new[] { "Dag", "Van", "Tot" };

				int i = 1;
				foreach (AvailabilityInfo item in availability) {
					cells[i] = new[] { ScheduleUtil.GetStringFromDayOfWeek(item.StartOfAvailability.DayOfWeek).FirstCharToUpper(), item.StartOfAvailability.ToShortTimeString(), item.EndOfAvailability.ToShortTimeString() };
					i++;
				}
				response += Util.FormatTextTable(cells, false);
			} else {
				response += "Niet op school ";
				if (weeksFromNow == 0) {
					response += "deze week";
				} else if (weeksFromNow == 1) {
					response += "volgende week";
				} else {
					response += $"over {weeksFromNow} weken";
				}
			}

			ReplyDeferred(response);
		}

		private async Task RespondDay(StudentSetInfo info, DayOfWeek day, bool includeToday) {

			ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(info, day, includeToday);
			if (result.Success) {
				ScheduleRecord[] records = result.Value;
				string response;
				if (records.Length == 0) {
					response = "Het ziet ernaar uit dat je ";

					if (DateTime.Today.DayOfWeek == day && includeToday) {
						response += "vandaag";
					} else if (DateTime.Today.AddDays(1).DayOfWeek == day) {
						response += "morgen";
					} else {
						response += "op " + ScheduleUtil.GetStringFromDayOfWeek(day);
					}

					response += " niets hebt.";
					if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					await ReplyAsync(response, info, null);
				} else {
					response = $"{info.DisplayText}: Rooster ";
					if (DateTime.Today.DayOfWeek == day && includeToday) {
						response += "voor vandaag";
					} else if (DateTime.Today.AddDays(1).DayOfWeek == day) {
						response += "voor morgen";
					} else {
						response += "op " + ScheduleUtil.GetStringFromDayOfWeek(day);
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
					ReplyDeferred(response, info, records.Last());
				}
			}
		}
	}
}
