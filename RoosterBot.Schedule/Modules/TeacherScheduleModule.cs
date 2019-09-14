using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Schedule {
	[LogTag("TeacherSM"), HiddenFromList]
	public class TeacherScheduleModule : ScheduleModuleBase {
		[Command("nu", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherCurrentCommand([Remainder] TeacherInfo teacher) {
			ReturnValue<ScheduleRecord> result = await GetRecord(teacher);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = string.Format(ResourcesService.GetString(Culture, "TeacherScheduleModule_TeacherCurrentCommand_NoCurrentRecord"), teacher.DisplayText);
					ReturnValue<ScheduleRecord> nextRecord = GetNextRecord(teacher).GetAwaiter().GetResult();

					if (nextRecord.Success && nextRecord.Value.Start.Date != DateTime.Today) {
						response += ResourcesService.GetString(Culture, "TeacherScheduleModule_TeacherProbablyAbsent");
					}

					ReplyDeferred(response, teacher, record);
				} else {
					await RespondRecord(string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_PretextNow"), teacher.DisplayText), teacher, record);
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen"), Priority(1)]
		public async Task TeacherNextCommand([Remainder] TeacherInfo teacher) {
			ReturnValue<ScheduleRecord> result = await GetNextRecord(teacher);
			if (result.Success) {
				ScheduleRecord record = result.Value;

				if (record == null) {
					string response = string.Format(ResourcesService.GetString(Culture, "TeacherScheduleModule_TeacherCurrentCommand_NoCurrentRecord"), teacher.DisplayText);
					ReturnValue<ScheduleRecord> nextRecord = GetNextRecord(teacher).GetAwaiter().GetResult();

					if (nextRecord.Success && nextRecord.Value.Start.Date != DateTime.Today) {
						response += ResourcesService.GetString(Culture, "TeacherScheduleModule_TeacherProbablyAbsent");
					}

					ReplyDeferred(response, teacher, record);
				} else {
					string pretext;
					if (record.Start.Date == DateTime.Today) {
						pretext = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_PretextNext"), teacher.DisplayText);
					} else {
						pretext = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_Pretext_FirstOn"), teacher.DisplayText, ScheduleUtil.GetStringFromDayOfWeek(Culture, record.Start.DayOfWeek));
					}

					await RespondRecord(pretext, teacher, record);
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherWeekdayCommand(DayOfWeek day, [Remainder] TeacherInfo teacher) {
			await RespondDay(teacher, ScheduleUtil.NextDayOfWeek(day, false));
		}

		[Command("vandaag", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherTodayCommand([Remainder] TeacherInfo teacher) {
			await RespondDay(teacher, DateTime.Today);
		}

		[Command("morgen", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherTomorrowCommand([Remainder] TeacherInfo teacher) {
			await RespondDay(teacher, DateTime.Today.AddDays(1));
		}

		[Command("deze week", RunMode = RunMode.Sync)]
		public async Task ShowThisWeekWorkingDaysCommand([Remainder] TeacherInfo teacher) {
			await RespondWeek(teacher, 0);
		}

		[Command("volgende week", RunMode = RunMode.Sync)]
		public async Task ShowNextWeekWorkingDaysCommand([Remainder] TeacherInfo teacher) {
			await RespondWeek(teacher, 1);
		}

		[Command("over", RunMode = RunMode.Sync)]
		public async Task ShowFutureCommand([Range(1, 52)] int amount, string unit, TeacherInfo teacher) {
			if (unit == "uur") {
				ReturnValue<ScheduleRecord> result = await GetRecordAfterTimeSpan(teacher, TimeSpan.FromHours(amount));
				if (result.Success) {
					ScheduleRecord record = result.Value;
					if (record != null) {
						await RespondRecord(string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_InXHours"), teacher.DisplayText, amount), teacher, record);
					} else {
						await ReplyAsync(ResourcesService.GetString(Culture, "ScheduleModuleBase_ShowFutureCommand_NoRecordAtThatTime"));
					}
				}
			} else if (unit == "dag" || unit == "dagen") {
				await RespondDay(teacher, DateTime.Today.AddDays(amount));
			} else if (unit == "week" || unit == "weken") {
				await RespondWeek(teacher, amount);
			} else {
				await MinorError(ResourcesService.GetString(Culture, "ScheduleModuleBase_ShowFutureCommand_OnlySupportUnits"));
			}
		}

		private async Task RespondDay(TeacherInfo teacher, DateTime date) {
			ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(teacher, date);
			if (result.Success) {
				ScheduleRecord[] records = result.Value;
				string response;
				if (records.Length == 0) {
					response = string.Format(ResourcesService.GetString(Culture, "TeacherScheduleModule_RespondDay_NoRecordRelative"), teacher.DisplayText, ScheduleUtil.GetRelativeDateReference(Culture, date));
					if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) {
						response += ResourcesService.GetString(Culture, "ScheduleModuleBase_ThatIsWeekend");
					}
					ReplyDeferred(response, null, null);
				} else {
					response = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_ResondDay_ScheduleForRelative"), teacher.DisplayText, ScheduleUtil.GetRelativeDateReference(Culture, date));

					string[][] cells = new string[records.Length + 1][];
					cells[0] = new string[] {
						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondDay_ColumnActivity"),
						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondDay_ColumnTime"),
						"Klas",
						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondDay_ColumnRoom")
					};
					int recordIndex = 1;
					foreach (ScheduleRecord record in records) {
						cells[recordIndex] = new string[4];
						cells[recordIndex][0] = record.Activity.DisplayText;
						cells[recordIndex][1] = $"{record.Start.ToString("HH:mm")} - {record.End.ToString("HH:mm")}";
						cells[recordIndex][2] = record.StudentSetsString;
						cells[recordIndex][3] = record.RoomString;

						recordIndex++;
					}
					response += Util.FormatTextTable(cells);
					ReplyDeferred(response, teacher, records.Last());
				}
			}
		}

		private async Task RespondWeek(TeacherInfo info, int weeksFromNow) {
			ReturnValue<AvailabilityInfo[]> result = await GetWeekAvailabilityInfo(info, weeksFromNow);
			if (result.Success) {
				AvailabilityInfo[] availability = result.Value;


				string response;
				if (availability.Length > 0) {
					if (weeksFromNow == 0) {
						response = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondWeek_ScheduleThisWeek"), info.DisplayText);
					} else if (weeksFromNow == 1) {
						response = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondWeek_ScheduleNextWeek"), info.DisplayText);
					} else {
						response = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondWeek_ScheduleInXWeeks"), info.DisplayText, weeksFromNow);
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
						response = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondWeek_NotPresentThisWeek"), info.DisplayText);
					} else if (weeksFromNow == 1) {
						response = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondWeek_NotPresentNextWeek"), info.DisplayText);
					} else {
						response = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondWeek_NotPresentInXWeeks"), info.DisplayText, weeksFromNow);
					}
				}

				await ReplyAsync(response);
			}
		}
	}
}
