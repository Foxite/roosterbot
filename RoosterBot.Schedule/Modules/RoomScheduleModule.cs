using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Schedule {
	// TODO merge all children of ScheduleModuleBase
	// Be honest, these files are pretty big messes, and it doesn't help that there are 3 of them.
	// The biggest problem is that one of the modules takes an array, which is because it's the only TypeReader that can return multiple matches.
	// Proposed solutions:
	// - All IdentifierInfo return arrays
	// - TeacherInfo returns a single one and informs the user in case of multiple matches (either by returning one of them or returning an error)
	[LogTag("RoomSM"), HiddenFromList]
	public class RoomScheduleModule : ScheduleModuleBase {
		[Command("nu", RunMode = RunMode.Async)]
		public async Task RoomCurrentCommand(RoomInfo room) {
			ReturnValue<ScheduleRecord> result = await GetRecord(room);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = ResourcesService.GetString(Culture, "RoomScheduleModule_RoomCurrentCommand_NoCurrentRecord");
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += ResourcesService.GetString(Culture, "ScheduleModuleBase_ItIsWeekend");
					}
					ReplyDeferred(response, room, record);
				} else {
					await RespondRecord(string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_PretextNow"), record.RoomString), room, record);
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen")]
		public async Task RoomNextCommand(RoomInfo room) {
			ReturnValue<ScheduleRecord> result = await GetNextRecord(room);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					await FatalError($"`GetNextRecord(\"Room\", {room.DisplayText})` returned null");
				} else {
					string pretext;

					if (record.Start.Date == DateTime.Today) {
						pretext = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_PretextNext"), record.RoomString);
					} else {
						pretext = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_Pretext_FirstOn"), record.RoomString, ScheduleUtil.GetStringFromDayOfWeek(Culture, record.Start.DayOfWeek));
					}
					await RespondRecord(pretext, room, record);
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async)]
		public async Task RoomWeekdayCommand(DayOfWeek day, RoomInfo info) {
			await RespondDay(info, ScheduleUtil.NextDayOfWeek(day, false));
		}
		
		[Command("vandaag", RunMode = RunMode.Async)]
		public async Task RoomTodayCommand(RoomInfo info) {
			await RespondDay(info, DateTime.Today);
		}

		[Command("morgen", RunMode = RunMode.Async)]
		public async Task RoomTomorrowCommand(RoomInfo info) {
			await RespondDay(info, DateTime.Today.AddDays(1));
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
		public async Task ShowFutureCommand([Range(1, 52)] int amount, string unit, RoomInfo info) {
			if (unit == "uur") {
				ReturnValue<ScheduleRecord> result = await GetRecordAfterTimeSpan(info, TimeSpan.FromHours(amount));
				if (result.Success) {
					ScheduleRecord record = result.Value;
					if (record != null) {
						await RespondRecord(string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_InXHours"), record.RoomString, amount), info, record);
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

		private async Task RespondDay(RoomInfo info, DateTime date) {
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
						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondDay_ColumnStudentSets"),
						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondDay_ColumnTeacher")

					};

					int recordIndex = 1;
					foreach (ScheduleRecord record in records) {
						cells[recordIndex] = new string[4];
						cells[recordIndex][0] = record.Activity.DisplayText;
						cells[recordIndex][1] = $"{record.Start.ToString("HH:mm")} - {record.End.ToString("HH:mm")}";
						cells[recordIndex][2] = record.StudentSetsString;
						cells[recordIndex][3] = record.StaffMemberString;


						recordIndex++;
					}
					response += Util.FormatTextTable(cells);
					ReplyDeferred(response, info, records.Last());
				}
			}
		}

		private async Task RespondWorkingDays(RoomInfo info, int weeksFromNow) {
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
