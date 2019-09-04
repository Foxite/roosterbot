using System;
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
					string response = Resources.StudentScheduleModule_StudentCurrentCommand_NoCurrentRecord;
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += Resources.ScheduleModuleBase_ItIsWeekend;
					}
					await ReplyAsync(response, info, null);
				} else {
					await RespondRecord(string.Format(Resources.ScheduleModuleBase_PretextNow, record.StudentSetsString), info, record);
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen")]
		public async Task StudentNextCommand(StudentSetInfo info) {
			ReturnValue<ScheduleRecord> result = await GetNextRecord(info);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					await FatalError($"`GetNextRecord(\"StudentSets\", {info.DisplayText})` returned null");
				} else {
					string pretext;
					if (record.Start.Date == DateTime.Today)
					{
						pretext = string.Format(Resources.ScheduleModuleBase_PretextNext, record.StudentSetsString);
					} else {
						pretext = string.Format(Resources.ScheduleModuleBase_Pretext_FirstOn, record.StudentSetsString, ScheduleUtil.GetStringFromDayOfWeek(record.Start.DayOfWeek));
					}
					await RespondRecord(pretext, info, record);
				}
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
						await RespondRecord(string.Format(Resources.ScheduleModuleBase_InXHours, record.StudentSetsString, amount), info, record);
					} else {
						await ReplyAsync(Resources.ScheduleModuleBase_ShowFutureCommand_NoRecordAtThatTime);
					}
				}
			} else if (unit == "dag" || unit == "dagen") {
				await RespondDay(info, DateTime.Today.AddDays(amount));
			} else if (unit == "week" || unit == "weken") {
				await RespondWorkingDays(info, amount);
			} else {
				await MinorError(Resources.ScheduleModuleBase_ShowFutureCommand_OnlySupportUnits);
			}
		}

		private async Task RespondDay(StudentSetInfo info, DateTime date) {
			ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(info, date);
			if (result.Success) {
				ScheduleRecord[] records = result.Value;
				string response;
				if (records.Length == 0) {
					response = string.Format(Resources.StudentScheduleModule_RespondDay_NoRecordAtRelattive, ScheduleUtil.GetRelativeDateReference(date));
					if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) {
						response += Resources.ScheduleModuleBase_ItIsWeekend;
					}
					ReplyDeferred(response, info, null);
				} else {
					response = string.Format(Resources.ScheduleModuleBase_ResondDay_ScheduleForRelative, info.DisplayText, ScheduleUtil.GetRelativeDateReference(date));

					string[][] cells = new string[records.Length + 1][];
					cells[0] = new string[] {
						Resources.ScheduleModuleBase_RespondDay_ColumnActivity,
						Resources.ScheduleModuleBase_RespondDay_ColumnTime,
						Resources.ScheduleModuleBase_RespondDay_ColumnTeacher,
						Resources.ScheduleModuleBase_RespondDay_ColumnRoom
					};
					int recordIndex = 1;
					foreach (ScheduleRecord record in records) {
						cells[recordIndex] = new string[4];
						cells[recordIndex][0] = await Activities.GetActivityFromAbbreviation(Context, record.Activity);
						cells[recordIndex][1] = $"{record.Start.ToString("HH:mm")} - {record.End.ToString("HH:mm")}";
						cells[recordIndex][2] = record.StaffMember.Length == 0 ? "" : string.Join(", ", record.StaffMember.Select(t => t.DisplayText));
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
						response = Resources.ScheduleModuleBase_ScheduleThisWeek;
					} else if (weeksFromNow == 1) {
						response = Resources.ScheduleModuleBase_ScheduleNextWeek;
					} else {
						response = string.Format(Resources.ScheduleModuleBase_ScheduleInXWeeks, weeksFromNow);
					}
					response += "\n";

					string[][] cells = new string[availability.Length + 1][];
					cells[0] = new[] {
						Resources.ScheduleModuleBase_RespondWorkingDays_ColumnDay,
						Resources.ScheduleModuleBase_RespondWorkingDays_ColumnFrom,
						Resources.ScheduleModuleBase_RespondWorkingDays_ColumnTo
					};

					int i = 1;
					foreach (AvailabilityInfo item in availability) {
						cells[i] = new[] { ScheduleUtil.GetStringFromDayOfWeek(item.StartOfAvailability.DayOfWeek).FirstCharToUpper(), item.StartOfAvailability.ToShortTimeString(), item.EndOfAvailability.ToShortTimeString() };
						i++;
					}
					response += Util.FormatTextTable(cells);
				} else {
					if (weeksFromNow == 0) {
						response += Resources.StudentScheduleModule_RespondWorkingDays_NotAtSchoolThisWeek;
					} else if (weeksFromNow == 1) {
						response += Resources.StudentScheduleModule_RespondWorkingDays_NotAtSchoolNextWeek;
					} else {
						response += string.Format(Resources.StudentScheduleModule_RespondWorkingDays_NotAtSchoolInXWeeks, weeksFromNow);
					}
				}

				await ReplyAsync(response);
			}
		}
	}
}
