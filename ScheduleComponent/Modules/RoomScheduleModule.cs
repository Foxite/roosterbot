﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using RoosterBot.Attributes;
using RoosterBot.Preconditions;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Modules {
	[LogTag("RoomSM"), HiddenFromList]
	public class RoomScheduleModule : ScheduleModuleBase {
		[Command("nu", RunMode = RunMode.Async)]
		public async Task RoomCurrentCommand(RoomInfo room) {
			ReturnValue<ScheduleRecord> result = await GetRecord(false, room);
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
				ReplyDeferred(response, room, record);
				
				if (record?.Activity == "pauze") {
					await GetAfterCommand();
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen")]
		public async Task RoomNextCommand(RoomInfo room) {
			ReturnValue<ScheduleRecord> result = await GetRecord(true, room);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					await FatalError($"`GetRecord(true, \"Room\", {room.DisplayText})` returned null");
				} else {
					bool isToday = record.Start.Date == DateTime.Today;
					string response;

					if (isToday) {
						response = $"{record.RoomString}: Hierna\n";
					} else {
						response = $"{record.RoomString}: Als eerste op {ScheduleUtil.GetStringFromDayOfWeek(record.Start.DayOfWeek)}\n";
					}

					response += TableItemActivity(record, false);

					if (record.Activity != "stdag doc") {
						response += TableItemStaffMember(record);
						response += TableItemStudentSets(record);
						response += TableItemStartEndTime(record);
						response += TableItemDuration(record);
						response += TableItemBreak(record);
					}

					ReplyDeferred(response, room, record);
					
					if (record.Activity == "pauze") {
						await GetAfterCommand();
					}
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async)]
		public async Task RoomWeekdayCommand(RoomInfo info, DayOfWeek day) {
			await RespondDay(info, day, false);
		}

		[Command("dag", RunMode = RunMode.Async)]
		public async Task RoomWeekdayCommand(DayOfWeek day, RoomInfo info) {
			await RespondDay(info, day, false);
		}
		
		[Command("vandaag", RunMode = RunMode.Async)]
		public async Task RoomTodayCommand(RoomInfo info) {
			await RespondDay(info, ScheduleUtil.GetDayOfWeekFromString("vandaag"), true);
		}

		[Command("morgen", RunMode = RunMode.Async)]
		public async Task RoomTomorrowCommand(RoomInfo info) {
			await RespondDay(info, ScheduleUtil.GetDayOfWeekFromString("morgen"), false);
		}

		[Command("deze week", RunMode = RunMode.Sync)]
		public async Task ShowThisWeekWorkingDaysCommand(RoomInfo info) {
			await RespondWorkingDays(info, 0);
		}

		[Command("volgende week", RunMode = RunMode.Sync)]
		public async Task ShowNextWeekWorkingDaysCommand(RoomInfo info) {
			await RespondWorkingDays(info, 1);
		}

		[Command("over", RunMode = RunMode.Sync)]
		// TODO: grammarWeeks should be used to identify the time unit (hours, days, weeks)
		public async Task ShowNWeeksWorkingDaysCommand([Range(1, 52)] int weeks, string grammarWeeks, RoomInfo info) {
			// Plurality easter egg
			if (grammarWeeks != (weeks > 1 ? "weken" : "week")) {
				ReplyDeferred(":thinking:");
			}
			
			await RespondWorkingDays(info, weeks);
		}

		private async Task RespondWorkingDays(RoomInfo info, int weeksFromNow) {
			AvailabilityInfo[] availability = Schedules.GetWeekAvailability(info, 0, Context);

			string response = info.DisplayText + ": ";

			if (availability.Length > 0) {
				if (weeksFromNow == 0) {
					response += "Deze week";
				} else if (weeksFromNow == 1) {
					response += "Volgende week";
				} else {
					response += $"Over {weeksFromNow} weken";
				}
				response += " in gebruik op \n";
				
				string[][] cells = new string[availability.Length + 1][];
				cells[0] = new[] { "Dag", "Van", "Tot" };

				int i = 1;
				foreach (AvailabilityInfo item in availability) {
					cells[i] = new[] { ScheduleUtil.GetStringFromDayOfWeek(item.StartOfAvailability.DayOfWeek).FirstCharToUpper(), item.StartOfAvailability.ToShortTimeString(), item.EndOfAvailability.ToShortTimeString() };
					i++;
				}
				response += Util.FormatTextTable(cells, false);
			} else {
				response += "Niet in gebruik ";
				if (weeksFromNow == 0) {
					response += "deze week";
				} else if (weeksFromNow == 1) {
					response += "volgende week";
				} else {
					response += $"over {weeksFromNow} weken";
				}
			}

			await ReplyAsync(response);
		}

		private async Task RespondDay(RoomInfo info, DayOfWeek day, bool includeToday) {
			ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(info, day, includeToday);
			if (result.Success) {
				ScheduleRecord[] records = result.Value;
				string response;
				if (records.Length == 0) {
					response = "Het ziet ernaar uit dat daar ";
					if (DateTime.Today.DayOfWeek == day && includeToday) {
						response += "vandaag";
					} else if (DateTime.Today.AddDays(1).DayOfWeek == day) {
						response += "morgen";
					} else {
						response += "op " + ScheduleUtil.GetStringFromDayOfWeek(day);
					}
					response += " niets is.";

					if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					ReplyDeferred(response, info, null);
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
					cells[0] = new string[] { "Activiteit", "Tijd", "Klas", "Leraar" };
					int recordIndex = 1;
					foreach (ScheduleRecord record in records) {
						cells[recordIndex] = new string[4];
						cells[recordIndex][0] = record.Activity;
						cells[recordIndex][1] = $"{record.Start.ToString("HH:mm")} - {record.End.ToString("HH:mm")}";
						cells[recordIndex][2] = record.StudentSetsString;
						cells[recordIndex][3] = record.StaffMember.Length == 0 ? "---" : string.Join(", ", record.StaffMember.Select(t => t.DisplayText));
						recordIndex++;
					}
					response += Util.FormatTextTable(cells, true);
					ReplyDeferred(response, info, records.Last());
				}
			}
		}
	}
}
