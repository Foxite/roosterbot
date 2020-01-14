using System;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Schedule {
	[Name("#ScheduleModule_Name")]
	[Description("#ScheduleModule_Summary")]
	public class ScheduleModule : RoosterModule {
		public ScheduleService Schedules { get; set; } = null!;
		
		[Command("#ScheduleModule_NowCommand"), Description("#ScheduleModule_DefaultCurrentCommand_Summary")]
		public async Task<CommandResult> CurrentCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			if (resolve.Success) {
				info = resolve.Value;
				ReturnValue<ScheduleRecord?> recordResult = await GetRecordAtDateTime(info, DateTime.Now);
				if (recordResult.Success) {
					if (recordResult.Value == null) {
						string caption = GetString("ScheduleModule_CurrentCommand_NoCurrentRecord", info.DisplayText);
						if (DateTimeUtil.IsWeekend(DateTime.Today)) {
							caption += GetString("ScheduleModule_ItIsWeekend");
						}
						// TODO allow for pagination to the next item
						return new TextResult(null, caption);
					} else {
						return GetSingleResult(recordResult.Value, info, info.DisplayText);
					}
				} else {
					return recordResult.ErrorResult;
				}
			} else {
				return resolve.ErrorResult;
			}
		}
		
		[Command("#ScheduleModule_NextCommand"), Description("#ScheduleModule_DefaultNextCommand_Summary")]
		public async Task<CommandResult> NextCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			if (resolve.Success) {
				info = resolve.Value;
				ReturnValue<ScheduleRecord> recordResult = await GetRecordAfterDateTime(info, DateTime.Now);
				if (recordResult.Success) {
					return GetSingleResult(recordResult.Value, info, info.DisplayText);
				} else {
					return recordResult.ErrorResult;
				}
			} else {
				return resolve.ErrorResult;
			}
		}
		
		[Command("#ScheduleModule_DayCommand"), Description("#ScheduleModule_DefaultWeekdayCommand_Summary")]
		public CommandResult WeekdayCommand([Name("#ScheduleModule_DayCommand_Day")] DayOfWeek day, [Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			return RespondDay(info, DateTimeUtil.NextDayOfWeek(day, false));
		}

		[Command("#ScheduleModule_TodayCommand"), Description("#ScheduleModule_DefaultTodayCommand_Summary")]
		public CommandResult TodayCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			return RespondDay(info, DateTime.Today);
		}

		[Command("#ScheduleModule_TomorrowCommand"), Description("#ScheduleModule_DefaultTomorrowCommand_Summary")]
		public CommandResult TomorrowCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			return RespondDay(info, DateTime.Today.AddDays(1));
		}
		
		[Command("#ScheduleModule_ThisWeekCommand"), Description("#ScheduleModule_ShowThisWeekWorkingDays_Summary")]
		public CommandResult ShowThisWeekWorkingDaysCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			return RespondWeek(info, 0);
		}

		[Command("#ScheduleModule_NextWeekCommand"), Description("#ScheduleModule_ShowNextWeekWorkingDays_Summary")]
		public CommandResult ShowNextWeekWorkingDaysCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			return RespondWeek(info, 1);
		}
		
		[Command("#ScheduleModule_FutureCommand"), Description("#ScheduleModule_ShowFutureCommand_Summary")]
		public async Task<CommandResult> ShowFutureCommand(
			[Name("#ScheduleModule_ShowFutureCommand_AmountParameterName")] int amount,
			[Name("#ScheduleModule_ShowFutureCommand_UnitParameterName"), TypeDisplay("#ScheduleModule_ShowFutureCommand_UnitTypeDisplayName")] string unit,
			[Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			if (resolve.Success) {
				info = resolve.Value;
				unit = unit.ToLower();
				if (GetString("ScheduleModule_ShowFutureCommand_UnitHours").Split('|').Contains(unit)) {
					ReturnValue<ScheduleRecord?> result = await GetRecordAtDateTime(info, DateTime.Now + TimeSpan.FromHours(amount));
					if (result.Success) {
						ScheduleRecord? record = result.Value;
						if (record == null) {
							// TODO allow for pagination to the next item
							return new TextResult(null, GetString("ScheduleModule_ShowFutureCommand_NoRecordAtThatTime"));
						} else {
							return GetSingleResult(record, info, info.DisplayText);
						}
					} else {
						return result.ErrorResult;
					}
				} else if (GetString("ScheduleModule_ShowFutureCommand_UnitDays").Split('|').Contains(unit)) {
					return RespondDay(info, DateTime.Today.AddDays(amount));
				} else if (GetString("ScheduleModule_ShowFutureCommand_UnitWeeks").Split('|').Contains(unit)) {
					return RespondWeek(info, amount);
				} else {
					return TextResult.Error(GetString("ScheduleModule_ShowFutureCommand_OnlySupportUnits"));
				}
			} else {
				return resolve.ErrorResult;
			}
		}

		private CommandResult RespondDay(IdentifierInfo? info, DateTime date) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			if (resolve.Success) {
				info = resolve.Value;
				return new PaginatedResult(new DayScheduleEnumerator(Context, info, date, new string[] {
					GetString("ScheduleModule_RespondDay_ColumnActivity"),
					GetString("ScheduleModule_RespondDay_ColumnTime"),
					GetString("ScheduleModule_RespondDay_ColumnStudentSets"),
					GetString("ScheduleModule_RespondDay_ColumnTeacher"),
					GetString("ScheduleModule_RespondDay_ColumnRoom")
				}));
			} else {
				return resolve.ErrorResult;
			}
		}
		
		private CommandResult RespondWeek(IdentifierInfo? info, int weeksFromNow) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			return resolve.Success
				? new PaginatedResult(new WeekScheduleEnumerator(Context, resolve.Value, weeksFromNow))
				: resolve.ErrorResult;
		}

		private PaginatedResult GetSingleResult(ScheduleRecord record, IdentifierInfo identifier, string caption) =>
			new PaginatedResult(new SingleScheduleEnumerator(Context, record, identifier, caption));

		private ReturnValue<IdentifierInfo> ResolveNullInfo(IdentifierInfo? info) {
			if (info == null) {
				StudentSetInfo? ssi = Context.UserConfig.GetStudentSet();
				if (ssi != null) {
					return ReturnValue<IdentifierInfo>.Successful(ssi);
				} else {
					return ReturnValue<IdentifierInfo>.Unsuccessful(TextResult.Error(GetString("StudentSetInfoReader_CheckFailed_MentionSelf", GuildConfig.CommandPrefix)));
				}
			} else {
				return ReturnValue<IdentifierInfo>.Successful(info);
			}
		}

		private Task<ReturnValue<ScheduleRecord?>> GetRecordAtDateTime(IdentifierInfo identifier, DateTime datetime) {
			return HandleErrorAsync(() => Schedules.GetRecordAtDateTime(identifier, datetime, Context));
		}

		private Task<ReturnValue<ScheduleRecord>> GetRecordAfterDateTime(IdentifierInfo identifier, DateTime datetime) {
			return HandleErrorAsync(() => Schedules.GetRecordAfterDateTime(identifier, datetime, Context));
		}

		private async Task<ReturnValue<T>> HandleErrorAsync<T>(Func<Task<T>> action) {
			try {
				return ReturnValue<T>.Successful(await action());
			} catch (Exception e) {
				return ReturnValue<T>.Unsuccessful(TextResult.Error(GetString(e switch {
					IdentifierNotFoundException _ => "ScheduleModule_HandleError_NotFound",
					RecordsOutdatedException    _ => "ScheduleModule_HandleError_RecordsOutdated",
					NoAllowedChannelsException  _ => "ScheduleModule_HandleError_NoSchedulesAvailableForServer",
					_ => throw e
				})));
			}
		}
	}
}
