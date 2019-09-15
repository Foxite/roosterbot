﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Schedule {
	[LogTag("StudentSM"), HiddenFromList]
	public class StudentScheduleModule : ScheduleModuleBase {
		[Command("nu", RunMode = RunMode.Async)]
		public async Task StudentCurrentCommand(StudentSetInfo info) {
			ReturnValue<ScheduleRecord> result = await GetRecord(info);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = string.Format(ResourcesService.GetString(Culture, "ScheduleModule_CurrentCommand_NoCurrentRecord"), info.DisplayText);

					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += ResourcesService.GetString(Culture, "ScheduleModuleBase_ItIsWeekend");
					}

					ReplyDeferred(response, info, record);
				} else {
					await RespondRecord(string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_PretextNow"), info.DisplayText), info, record);
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen")]
		public async Task StudentNextCommand(StudentSetInfo info) {
			ReturnValue<ScheduleRecord> result = await GetNextRecord(info);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				string pretext;
				if (record.Start.Date == DateTime.Today) {
					pretext = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_PretextNext"), info.DisplayText);
				} else {
					pretext = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_Pretext_FirstOn"), info.DisplayText, ScheduleUtil.GetStringFromDayOfWeek(Culture, record.Start.DayOfWeek));
				}
				await RespondRecord(pretext, info, record);
			}
		}

		[Command("dag", RunMode = RunMode.Async)]
		public async Task StudentWeekdayCommand(DayOfWeek day, StudentSetInfo info) {
			await RespondDay(info, ScheduleUtil.NextDayOfWeek(day, false));
		}

		[Command("vandaag", RunMode = RunMode.Async)]
		public async Task StudentTodayCommand(StudentSetInfo info) {
			await RespondDay(info, DateTime.Today);
		}

		[Command("morgen", RunMode = RunMode.Async)]
		public async Task StudentTomorrowCommand(StudentSetInfo info) {
			await RespondDay(info, DateTime.Today.AddDays(1));
		}

		[Command("deze week", RunMode = RunMode.Sync)]
		public async Task ShowThisWeekWorkingDaysCommand(StudentSetInfo info) {
			await RespondWorkingDays(info, 0);
		}

		[Command("volgende week", RunMode = RunMode.Sync)]
		public async Task ShowNextWeekWorkingDaysCommand(StudentSetInfo info) {
			await RespondWorkingDays(info, 1);
		}

		[Command("over", RunMode = RunMode.Sync)]
		public async Task ShowFutureCommand([Range(1, 52)] int amount, string unit, StudentSetInfo info) {
			if (unit == "uur") {
				ReturnValue<ScheduleRecord> result = await GetRecordAfterTimeSpan(info, TimeSpan.FromHours(amount));
				if (result.Success) {
					ScheduleRecord record = result.Value;
					if (record != null) {
						await RespondRecord(string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_InXHours"), info.DisplayText, amount), info, record);
					} else {
						await ReplyAsync(ResourcesService.GetString(Culture, "ScheduleModuleBase_ShowFutureCommand_NoRecordAtThatTime"));
					}
				}
			} else if (unit == "dag" || unit == "dagen") {
				await RespondDay(info, DateTime.Today.AddDays(amount));
			} else if (unit == "week" || unit == "weken") {
				await RespondWorkingDays(info, amount);
			} else {
				await MinorError(ResourcesService.GetString(Culture, "ScheduleModuleBase_ShowFutureCommand_OnlySupportUnits"));
			}
		}

		private async Task RespondDay(StudentSetInfo info, DateTime date) {
			ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(info, date);
			if (result.Success) {
				ScheduleRecord[] records = result.Value;
				string response;
				if (records.Length == 0) {
					response = string.Format(ResourcesService.GetString(Culture, "ScheduleModule_RespondDay_NoRecordAtRelative"), ScheduleUtil.GetRelativeDateReference(Culture, date));
					if (ScheduleUtil.IsWeekend(date)) {
						if (ScheduleUtil.IsWithinSameWeekend(date, DateTime.Today)) {
							response += ResourcesService.GetString(Culture, "ScheduleModuleBase_ItIsWeekend");
						} else {
							response += ResourcesService.GetString(Culture, "ScheduleModuleBase_ThatIsWeekend");
						}
					}
					ReplyDeferred(response, info, null);
				} else {
					response = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_ResondDay_ScheduleForRelative"), info.DisplayText, ScheduleUtil.GetRelativeDateReference(Culture, date));

					string[][] cells = new string[records.Length + 1][];
					cells[0] = new string[] {
						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondDay_ColumnActivity"),
						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondDay_ColumnTime"),

						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondDay_ColumnTeacher"),
						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondDay_ColumnRoom")
					};

					int recordIndex = 1;
					foreach (ScheduleRecord record in records) {
						cells[recordIndex] = new string[4];
						cells[recordIndex][0] = record.Activity.DisplayText;
						cells[recordIndex][1] = $"{record.Start.ToString("HH:mm")} - {record.End.ToString("HH:mm")}";

						cells[recordIndex][2] = record.StaffMemberString;
						cells[recordIndex][3] = record.RoomString;

						recordIndex++;
					}
					response += Util.FormatTextTable(cells);
					ReplyDeferred(response, info, records.Last());
				}
			}
		}

		private async Task RespondWorkingDays(StudentSetInfo info, int weeksFromNow) {
			ReturnValue<AvailabilityInfo[]> result = await GetWeekAvailabilityInfo(info, weeksFromNow);
			if (result.Success) {
				AvailabilityInfo[] availability = result.Value;

				string response = info.DisplayText + ": ";

				if (availability.Length > 0) {
					if (weeksFromNow == 0) {
						response = ResourcesService.GetString(Culture, "ScheduleModuleBase_ScheduleThisWeek");
					} else if (weeksFromNow == 1) {
						response = ResourcesService.GetString(Culture, "ScheduleModuleBase_ScheduleNextWeek");
					} else {
						response = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_ScheduleInXWeeks"), weeksFromNow);
					}
					response += "\n";

					string[][] cells = new string[availability.Length + 1][];
					cells[0] = new[] {
						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondWorkingDays_ColumnDay"),
						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondWorkingDays_ColumnFrom"),
						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondWorkingDays_ColumnTo")
					};

					int i = 1;
					foreach (AvailabilityInfo item in availability) {
						cells[i] = new[] {
							ScheduleUtil.GetStringFromDayOfWeek(Culture, item.StartOfAvailability.DayOfWeek).FirstCharToUpper(),
							item.StartOfAvailability.ToShortTimeString(),
							item.EndOfAvailability.ToShortTimeString()
						};
						i++;
					}
					response += Util.FormatTextTable(cells);
				} else {
					if (weeksFromNow == 0) {
						response += ResourcesService.GetString(Culture, "ScheduleModule_RespondWorkingDays_NotOnScheduleThisWeek");
					} else if (weeksFromNow == 1) {
						response += ResourcesService.GetString(Culture, "ScheduleModule_RespondWorkingDays_NotOnScheduleNextWeek");
					} else {
						response += string.Format(ResourcesService.GetString(Culture, "ScheduleModule_RespondWorkingDays_NotOnScheduleInXWeeks"), weeksFromNow);
					}
				}

				ReplyDeferred(response);
			}
		}
	}
}
