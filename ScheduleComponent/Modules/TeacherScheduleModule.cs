﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using RoosterBot.Attributes;
using RoosterBot.Preconditions;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Modules {
	[LogTag("TeacherSM"), HiddenFromList]
	public class TeacherScheduleModule : ScheduleModuleBase {
		[Command("nu", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherCurrentCommand([Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo teacher in teachers) {
				ReturnValue<ScheduleRecord> result = await GetRecord(teachers[0]);
				if (result.Success) {
					ScheduleRecord record = result.Value;
					if (record == null) {
						string response = string.Format(Resources.TeacherScheduleModule_TeacherCurrentCommand_NoCurrentRecord, teacher.DisplayText);
						ReturnValue<ScheduleRecord> nextRecord = GetNextRecord(teacher).GetAwaiter().GetResult();

						if (nextRecord.Success && nextRecord.Value.Start.Date != DateTime.Today) {
							response += Resources.TeacherScheduleModule_TeacherProbablyAbsent;
						}

						ReplyDeferred(response, teacher, record);
					} else {
						await RespondRecord(string.Format(Resources.ScheduleModuleBase_PretextNow, teacher.DisplayText), teacher, record);
					}
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen"), Priority(1)]
		public async Task TeacherNextCommand([Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo teacher in teachers) {
				ReturnValue<ScheduleRecord> result = await GetNextRecord(teachers[0]);
				if (result.Success) {
					ScheduleRecord record = result.Value;

					if (record == null) {
						string response = string.Format(Resources.TeacherScheduleModule_TeacherCurrentCommand_NoCurrentRecord, teacher.DisplayText);
						ReturnValue<ScheduleRecord> nextRecord = GetNextRecord(teacher).GetAwaiter().GetResult();

						if (nextRecord.Success && nextRecord.Value.Start.Date != DateTime.Today) {
							response += Resources.TeacherScheduleModule_TeacherProbablyAbsent;
						}

						ReplyDeferred(response, teacher, record);
					} else {
						string pretext;
						if (record.Start.Date == DateTime.Today) {
							pretext = string.Format(Resources.ScheduleModuleBase_PretextNext, teacher.DisplayText);
						} else {
							pretext = string.Format(Resources.ScheduleModuleBase_Pretext_FirstOn, teacher.DisplayText, ScheduleUtil.GetStringFromDayOfWeek(record.Start.DayOfWeek));
						}

						await RespondRecord(pretext, teacher, record);
					}
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherWeekdayCommand(DayOfWeek day, [Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo teacher in teachers) {
				await RespondDay(teacher, ScheduleUtil.NextDayOfWeek(day, false));
			}
		}

		[Command("vandaag", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherTodayCommand([Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo teacher in teachers) {
				await RespondDay(teacher, DateTime.Today);
			}
		}

		[Command("morgen", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherTomorrowCommand([Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo teacher in teachers) {
				await RespondDay(teacher, DateTime.Today.AddDays(1));
			}
		}

		[Command("deze week", RunMode = RunMode.Sync)]
		public async Task ShowThisWeekWorkingDaysCommand([Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo info in teachers) {
				await RespondWeek(info, 0);
			}
		}

		[Command("volgende week", RunMode = RunMode.Sync)]
		public async Task ShowNextWeekWorkingDaysCommand([Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo info in teachers) {
				await RespondWeek(info, 1);
			}
		}

		[Command("over", RunMode = RunMode.Sync)]
		public async Task ShowFutureCommand([Range(1, 52)] int amount, string unit, TeacherInfo[] teachers) {
			if (unit == "uur") {
				foreach (TeacherInfo teacher in teachers) {
					ReturnValue<ScheduleRecord> result = await GetRecordAfterTimeSpan(teacher, TimeSpan.FromHours(amount));
					if (result.Success) {
						ScheduleRecord record = result.Value;
						if (record != null) {
							await RespondRecord(string.Format(Resources.ScheduleModuleBase_InXHours, string.Join(", ", teachers.Select(t => t.DisplayText)), amount), teacher, record);
						} else {
							await ReplyAsync(Resources.ScheduleModuleBase_ShowFutureCommand_NoRecordAtThatTime);
						}
					}
				}
			} else if (unit == "dag" || unit == "dagen") {
				foreach (TeacherInfo teacher in teachers) {
					await RespondDay(teacher, DateTime.Today.AddDays(amount));
				}
			} else if (unit == "week" || unit == "weken") {
				foreach (TeacherInfo teacher in teachers) {
					await RespondWeek(teacher, amount);
				}
			} else {
				await MinorError(Resources.ScheduleModuleBase_ShowFutureCommand_OnlySupportUnits);
			}
		}

		private async Task RespondDay(TeacherInfo teacher, DateTime date) {
			ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(teacher, date);
			if (result.Success) {
				ScheduleRecord[] records = result.Value;
				string response;
				if (records.Length == 0) {
					response = string.Format(Resources.TeacherScheduleModule_RespondDay_NoRecordRelative, teacher.DisplayText, ScheduleUtil.GetRelativeDateReference(date));
					if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) {
						response += Resources.ScheduleModuleBase_ThatIsWeekend;
					}
					ReplyDeferred(response, null, null);
				} else {
					response = string.Format(Resources.ScheduleModuleBase_ResondDay_ScheduleForRelative, teacher.DisplayText, ScheduleUtil.GetRelativeDateReference(date));

					string[][] cells = new string[records.Length + 1][];
					cells[0] = new string[] { Resources.ScheduleModuleBase_RespondDay_ColumnActivity, Resources.ScheduleModuleBase_RespondDay_ColumnTime, "Klas", Resources.ScheduleModuleBase_RespondDay_ColumnRoom };
					int recordIndex = 1;
					foreach (ScheduleRecord record in records) {
						cells[recordIndex] = new string[4];
						cells[recordIndex][0] = Activities.GetActivityFromAbbreviation(Context.Guild, record.Activity);
						cells[recordIndex][1] = $"{record.Start.ToString("HH:mm")} - {record.End.ToString("HH:mm")}";
						cells[recordIndex][2] = record.StudentSetsString;
						cells[recordIndex][3] = record.RoomString;

						recordIndex++;
					}
					response += Util.FormatTextTable(cells, true);
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
						response = string.Format(Resources.ScheduleModuleBase_RespondWeek_ScheduleThisWeek, info.DisplayText);
					} else if (weeksFromNow == 1) {
						response = string.Format(Resources.ScheduleModuleBase_RespondWeek_ScheduleNextWeek, info.DisplayText);
					} else {
						response = string.Format(Resources.ScheduleModuleBase_RespondWeek_ScheduleInXWeeks, info.DisplayText, weeksFromNow);
					}
					response += "\n";

					string[][] cells = new string[availability.Length + 1][];
					cells[0] = new[] { Resources.ScheduleModuleBase_RespondWorkingDays_ColumnDay,
						Resources.ScheduleModuleBase_RespondWorkingDays_ColumnFrom,
						Resources.ScheduleModuleBase_RespondWorkingDays_ColumnTo
					};

					int i = 1;
					foreach (AvailabilityInfo item in availability) {
						cells[i] = new[] { ScheduleUtil.GetStringFromDayOfWeek(item.StartOfAvailability.DayOfWeek).FirstCharToUpper(), item.StartOfAvailability.ToShortTimeString(), item.EndOfAvailability.ToShortTimeString() };
						i++;
					}
					response += Util.FormatTextTable(cells, false);
				} else {
					if (weeksFromNow == 0) {
						response = string.Format(Resources.ScheduleModuleBase_RespondWeek_NotPresentThisWeek, info.DisplayText);
					} else if (weeksFromNow == 1) {
						response = string.Format(Resources.ScheduleModuleBase_RespondWeek_NotPresentNextWeek, info.DisplayText);
					} else {
						response = string.Format(Resources.ScheduleModuleBase_RespondWeek_NotPresentInXWeeks, info.DisplayText, weeksFromNow);
					}
				}

				await ReplyAsync(response);
			}
		}
	}
}
