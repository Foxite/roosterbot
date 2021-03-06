﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Schedule {
	[Name("#ScheduleModule_Name")]
	[Description("#ScheduleModule_Summary")]
	public class ScheduleModule : RoosterModule {
		public ScheduleService Schedules { get; set; } = null!;

		#region Commands
		[Command("#ScheduleModule_NowCommand"), Description("#ScheduleModule_CurrentCommand_Summary")]
		public Task<CommandResult> CurrentCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null)
			=> RespondRecordAtTime(info, TimeSpan.Zero);
		
		[Command("#ScheduleModule_NextCommand"), Description("#ScheduleModule_NextCommand_Summary")]
		public async Task<CommandResult> NextCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			if (resolve.Success) {
				info = resolve.Value;
				ReturnValue<ScheduleRecord> recordResult = await GetRecordAfterDateTime(info, DateTime.Now);
				if (recordResult.Success) {
					return new PaginatedResult(new SingleScheduleEnumerator(Context, recordResult.Value, info), null);
				} else {
					return recordResult.ErrorResult;
				}
			} else {
				return resolve.ErrorResult;
			}
		}
		
		[Command("#ScheduleModule_DayCommand"), Description("#ScheduleModule_WeekdayCommand_Summary")]
		public CommandResult WeekdayCommand([Name("#ScheduleModule_DayCommand_Day")] DayOfWeek day, [Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			return RespondDay(info, DateTimeUtil.NextDayOfWeek(day, true));
		}

		[Command("#ScheduleModule_TodayCommand"), Description("#ScheduleModule_TodayCommand_Summary")]
		public CommandResult TodayCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			return RespondDay(info, DateTime.Today);
		}

		[Command("#ScheduleModule_TomorrowCommand"), Description("#ScheduleModule_TomorrowCommand_Summary")]
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
		public Task<CommandResult> ShowFutureCommand(
			[Name("#ScheduleModule_ShowFutureCommand_AmountParameterName")] int amount,
			[Name("#ScheduleModule_ShowFutureCommand_UnitParameterName"), TypeDisplay("#ScheduleModule_ShowFutureCommand_UnitTypeDisplayName")] string unit,
			[Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			unit = unit.ToLower();
			if (GetString("ScheduleModule_ShowFutureCommand_UnitHours").Split('|').Contains(unit)) {
				return RespondRecordAtTime(info, TimeSpan.FromHours(amount));
			} else if (GetString("ScheduleModule_ShowFutureCommand_UnitDays").Split('|').Contains(unit)) {
				return Task.FromResult(RespondDay(info, DateTime.Today.AddDays(amount)));
			} else if (GetString("ScheduleModule_ShowFutureCommand_UnitWeeks").Split('|').Contains(unit)) {
				return Task.FromResult(RespondWeek(info, amount));
			} else {
				return Task.FromResult<CommandResult>(TextResult.Error(GetString("ScheduleModule_ShowFutureCommand_OnlySupportUnits")));
			}
		}

		[Command("#ScheduleModule_AfterCommand"), HiddenFromList, IgnoresExtraArguments, Obsolete]
		public CommandResult DeprecatedCommand() {
			return TextResult.Info(GetString("ScheduleModule_AfterCommand_Output"));
		}
		#endregion

		#region Responses
		private async Task<CommandResult> RespondRecordAtTime(IdentifierInfo? info, TimeSpan fromNow) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			if (resolve.Success) {
				info = resolve.Value;
				DateTime datetime = DateTime.Now + fromNow;
				ReturnValue<ScheduleRecord?> recordResult = await GetRecordAtDateTime(info, datetime);
				if (recordResult.Success) {
					ScheduleRecord? record = recordResult.Value;
					string caption;
					if (record == null) {
						if (fromNow.TotalSeconds < 1) {
							caption = GetString("ScheduleModule_CurrentCommand_NoCurrentRecord", info.DisplayText);
							if (DateTimeUtil.IsWeekend(datetime.Date)) {
								caption += GetString("ScheduleModule_ItIsWeekend");
							}
						} else {
							caption = GetString("ScheduleModule_ShowFutureCommand_NoRecordAtThatTime", info.DisplayText);
							if (DateTimeUtil.IsWeekend(datetime.Date)) {
								caption += GetString("ScheduleModule_ThatIsWeekend");
							}
						}
						caption += GetString("ScheduleModule_CurrentCommand_Next");
						ReturnValue<ScheduleRecord> nextRecordResult = await GetRecordAfterDateTime(info, datetime);
						if (nextRecordResult.Success) {
							record = nextRecordResult.Value;
						} else {
							return nextRecordResult.ErrorResult;
						}
					} else {
						caption = "";
					}
					return new PaginatedResult(new SingleScheduleEnumerator(Context, record, info), caption);
				} else {
					return recordResult.ErrorResult;
				}
			} else {
				return resolve.ErrorResult;
			}
		}

		private CommandResult RespondDay(IdentifierInfo? info, DateTime date) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			if (resolve.Success) {
				info = resolve.Value;
				return new PaginatedResult(new DayScheduleEnumerator(Context, info, date), null);
			} else {
				return resolve.ErrorResult;
			}
		}
		
		private CommandResult RespondWeek(IdentifierInfo? info, int weeksFromNow) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			return resolve.Success
				? new PaginatedResult(new WeekScheduleEnumerator(Context, resolve.Value, weeksFromNow), null)
				: resolve.ErrorResult;
		}
		#endregion

		#region Util
		private ReturnValue<IdentifierInfo> ResolveNullInfo(IdentifierInfo? info) {
			if (info == null) {
				IdentifierInfo? ssi = Context.UserConfig.GetIdentifier();

				if (ssi != null) {
					return ReturnValue<IdentifierInfo>.Successful(ssi);
				} else {
					return ReturnValue<IdentifierInfo>.Unsuccessful(TextResult.Error(GetString("UserIdentifierParser_CheckFailed_MentionSelf", ChannelConfig.CommandPrefix)));
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
		#endregion
	}
}
