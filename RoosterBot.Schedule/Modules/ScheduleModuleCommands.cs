using System;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Schedule {
	[Name("#ScheduleModule_Name")]
	[Description("#ScheduleModule_Summary")]
	public partial class ScheduleModule : RoosterModule {
		private readonly CompoundResult m_Result = new CompoundResult("\n");
		private LastScheduleCommandInfo? m_LookedUpData;
		private int m_AfterRecursion = 0;

		public ScheduleService Schedules { get; set; } = null!;

		[Command("#ScheduleModule_NowCommand"), Description("#ScheduleModule_DefaultCurrentCommand_Summary")]
		public async Task<CommandResult> CurrentCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			info = ResolveNullInfo(info);
			if (info != null) {
				ReturnValue<ScheduleRecord?> result = await GetRecordAtDateTime(info, DateTime.Now);
				if (result.Success) {
					ScheduleRecord? record = result.Value;
					if (record == null) {
						string response = GetString("ScheduleModule_CurrentCommand_NoCurrentRecord", info.DisplayText);

						if (DateTimeUtil.IsWeekend(DateTime.Today)) {
							response += GetString("ScheduleModule_ItIsWeekend");
						}

						m_Result.AddResult(new TextResult(null, response));
						await NextCommand(info);
					} else {
						await RespondRecord(GetString("ScheduleModule_PretextNow", info.DisplayText), info, record);
					}
				}
			}
			return m_Result;
		}

		[Command("#ScheduleModule_NextCommand"), Description("#ScheduleModule_DefaultNextCommand_Summary")]
		public async Task<CommandResult> NextCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			info = ResolveNullInfo(info);
			if (info != null) {
				ReturnValue<ScheduleRecord> result = await GetRecordAfterDateTime(info, DateTime.Now);
				if (result.Success) {
					ScheduleRecord record = result.Value;
					string pretext;
					if (record.Start.Date == DateTime.Today) {
						pretext = GetString("ScheduleModule_PretextNext", info.DisplayText);
					} else {
						pretext = GetString("ScheduleModule_Pretext_FirstOn", info.DisplayText, record.Start.DayOfWeek.GetName(Culture));
					}
					await RespondRecord(pretext, info, record);
				}
			}
			return m_Result;
		}

		[Command("#ScheduleModule_DayCommand"), Description("#ScheduleModule_DefaultWeekdayCommand_Summary")]
		public async Task<CommandResult> WeekdayCommand([Name("#ScheduleModule_DayCommand_Day")] DayOfWeek day, [Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			await RespondDay(info, DateTimeUtil.NextDayOfWeek(day, false));
			return m_Result;
		}

		[Command("#ScheduleModule_TodayCommand"), Description("#ScheduleModule_DefaultTomorrowCommand_Summary")]
		public async Task<CommandResult> TodayCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			await RespondDay(info, DateTime.Today);
			return m_Result;
		}

		[Command("#ScheduleModule_TomorrowCommand"), Description("#ScheduleModule_DefaultTodayCommand_Summary")]
		public async Task<CommandResult> TomorrowCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			await RespondDay(info, DateTime.Today.AddDays(1));
			return m_Result;
		}

		[Command("#ScheduleModule_ThisWeekCommand"), Description("#ScheduleModule_ShowThisWeekWorkingDays_Summary")]
		public async Task<CommandResult> ShowThisWeekWorkingDaysCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			await RespondWeek(info, 0);
			return m_Result;
		}

		[Command("#ScheduleModule_NextWeekCommand"), Description("#ScheduleModule_ShowNextWeekWorkingDays_Summary")]
		public async Task<CommandResult> ShowNextWeekWorkingDaysCommand([Name("#ScheduleModule_IdentiferInfo_Name"), Remainder] IdentifierInfo? info = null) {
			await RespondWeek(info, 1);
			return m_Result;
		}

		[Command("#ScheduleModule_FutureCommand"), Description("#ScheduleModule_ShowNWeeksWorkingDays_Summary")]
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
		}

		[Command("#ScheduleModule_AfterCommand"), IgnoresExtraArguments, Description("#ScheduleModule_AfterCommand_Summary")]
		public async Task<CommandResult> AfterCommand() {
			// This allows us to call !daarna automatically in certain conditions, and prevents the recursion from causing problems.
			m_AfterRecursion++;
			if (m_AfterRecursion >= 5) {
				return m_Result;
			}

			LastScheduleCommandInfo? query = m_LookedUpData ?? UserConfig.GetLastScheduleCommand(Context.Channel);

			if (query == null) {
				MinorError(GetString("ScheduleModule_GetAfterCommand_NoContext"));
			} else {
				switch (query.Kind) {
					case ScheduleResultKind.Single:
						ReturnValue<ScheduleRecord> result = await GetRecordAfterDateTime(query.Identifier, query.RecordEndTime ?? DateTime.Now);

						if (result.Success) {
							string pretext;
							if (query.RecordEndTime == null) {
								pretext = GetString("ScheduleModule_PretextNext", query.Identifier.DisplayText);
							} else if (query.RecordEndTime.Value.Date != result.Value.Start.Date) {
								pretext = GetString("ScheduleModule_Pretext_FirstOn", query.Identifier.DisplayText, DateTimeUtil.GetStringFromDayOfWeek(Culture, result.Value.Start.DayOfWeek));
							} else {
								pretext = GetString("ScheduleModule_PretextAfterPrevious", query.Identifier.DisplayText);
							}

							await RespondRecord(pretext, query.Identifier, result.Value, false);
							if (result.Value.ShouldCallNextCommand) {
								await AfterCommand();
							}
						}
						break;
					case ScheduleResultKind.Day:
						await RespondDay(query.Identifier, query.RecordEndTime ?? DateTime.Today.AddDays(1));
						break;
					case ScheduleResultKind.Week:
						await RespondWeek(query.Identifier, (int) ((query.RecordEndTime - DateTime.Now)?.TotalDays ?? 7) / 7);
						break;
				}
			}
			return m_Result;
		}
	}
}
