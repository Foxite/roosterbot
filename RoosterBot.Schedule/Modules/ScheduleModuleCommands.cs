using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Schedule {
	[Name("#ScheduleModule_Name")]
	[Description("#ScheduleModule_Summary")]
	public partial class ScheduleModule : RoosterModule {
		private string m_ResponseCaption = "";

		public ScheduleService Schedules { get; set; } = null!;
		//UserConfig.OnScheduleRequestByUser(Context.Channel, m_LookedUpData);

		[Command("#ScheduleModule_NowCommand"), Description("#ScheduleModule_DefaultCurrentCommand_Summary")]
		public async Task<CommandResult> CurrentCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			if (resolve.Success) {
				info = resolve.Value;
				ReturnValue<ScheduleRecord?> recordResult = await GetRecordAtDateTime(info, DateTime.Now);
				if (recordResult.Success) {
					ScheduleRecord? record = recordResult.Value;
					if (record == null) {
						m_ResponseCaption = GetString("ScheduleModule_CurrentCommand_NoCurrentRecord", info.DisplayText);

						if (DateTimeUtil.IsWeekend(DateTime.Today)) {
							m_ResponseCaption += GetString("ScheduleModule_ItIsWeekend");
						}

						return new TextResult(null, m_ResponseCaption);
						//return await NextCommand(info);
					} else {
						return GetResult(record, info, GetString("ScheduleModule_PretextNow", info.DisplayText));
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
					ScheduleRecord record = recordResult.Value;
					string pretext;
					if (record.Start.Date == DateTime.Today) {
						pretext = GetString("ScheduleModule_PretextNext", info.DisplayText);
					} else {
						pretext = GetString("ScheduleModule_Pretext_FirstOn", info.DisplayText, record.Start.DayOfWeek.GetName(Culture));
					}
					return GetResult(record, info, pretext);
				} else {
					return recordResult.ErrorResult;
				}
			} else {
				return resolve.ErrorResult;
			}
		}
		
		[Command("#ScheduleModule_DayCommand"), Description("#ScheduleModule_DefaultWeekdayCommand_Summary")]
		public Task<CommandResult> WeekdayCommand([Name("#ScheduleModule_DayCommand_Day")] DayOfWeek day, [Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			return RespondDay(info, DateTimeUtil.NextDayOfWeek(day, false));
		}

		[Command("#ScheduleModule_TodayCommand"), Description("#ScheduleModule_DefaultTodayCommand_Summary")]
		public Task<CommandResult> TodayCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			return RespondDay(info, DateTime.Today);
		}

		[Command("#ScheduleModule_TomorrowCommand"), Description("#ScheduleModule_DefaultTomorrowCommand_Summary")]
		public Task<CommandResult> TomorrowCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			return RespondDay(info, DateTime.Today.AddDays(1));
		}
		
		[Command("#ScheduleModule_ThisWeekCommand"), Description("#ScheduleModule_ShowThisWeekWorkingDays_Summary")]
		public Task<CommandResult> ShowThisWeekWorkingDaysCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			return RespondWeek(info, 0);
		}

		[Command("#ScheduleModule_NextWeekCommand"), Description("#ScheduleModule_ShowNextWeekWorkingDays_Summary")]
		public Task<CommandResult> ShowNextWeekWorkingDaysCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			return RespondWeek(info, 1);
		}
		/*
		[Command("#ScheduleModule_FutureCommand"), Description("#ScheduleModule_ShowFutureCommand_Summary")]
		public async Task<CommandResult> ShowFutureCommand([Name("#ScheduleModule_ShowFutureCommand_AmountParameterName")] int amount, [Name("#ScheduleModule_ShowFutureCommand_UnitParameterName"), TypeDisplay("#ScheduleModule_ShowFutureCommand_UnitTypeDisplayName")] string unit, [Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			info = ResolveNullInfo(info);
			if (info != null) {
				unit = unit.ToLower();
				if (GetString("ScheduleModule_ShowFutureCommand_UnitHours").Split('|').Contains(unit)) {
					ReturnValue<ScheduleRecord?> result = await GetRecordAtDateTime(info, DateTime.Now + TimeSpan.FromHours(amount));
					if (result.Success) {
						ScheduleRecord? record = result.Value;
						if (record != null) {
							await RespondRecord(GetString("ScheduleModule_InXHours", info.DisplayText, amount), info, record);
						} else {
							m_Result.AddResult(new TextResult(null, GetString("ScheduleModule_ShowFutureCommand_NoRecordAtThatTime")));
							m_LookedUpData = new LastScheduleCommandInfo(info, DateTime.Now + TimeSpan.FromHours(amount), ScheduleResultKind.Single);
							await AfterCommand();
						}
					}
				} else if (GetString("ScheduleModule_ShowFutureCommand_UnitDays").Split('|').Contains(unit)) {
					await RespondDay(info, DateTime.Today.AddDays(amount));
				} else if (GetString("ScheduleModule_ShowFutureCommand_UnitWeeks").Split('|').Contains(unit)) {
					await RespondWeek(info, amount);
				} else {
					MinorError(GetString("ScheduleModule_ShowFutureCommand_OnlySupportUnits"));
				}
			}
			return m_Result;
		}*/
	}
}
