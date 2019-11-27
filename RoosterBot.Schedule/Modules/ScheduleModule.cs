using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoosterBot.DateTimeUtils;
using Qmmands;

namespace RoosterBot.Schedule {
	// TODO (refactor) reduce the size of this file (it has 353 lines right now)
	// TODO (feature) Use the new Result system for this module, it is non functional until we use it
	[Name("#ScheduleModule_Name")]
	[Description("#ScheduleModule_Summary")]
	[Remarks("#ScheduleModule_Remarks")]
	[LocalizedModule("nl-NL", "en-US")]
	public class ScheduleModule : RoosterModuleBase {
		private readonly CompoundResult m_Result = new CompoundResult("\n");
		private LastScheduleCommandInfo? m_LookedUpData;

		public ScheduleService Schedules { get; set; } = null!;

		#region Commands
		[Command("#ScheduleModule_NowCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_DefaultCurrentCommand_Summary")]
		public async Task<CommandResult> CurrentCommand([Remainder] IdentifierInfo? info = null) {
			info = await ResolveNullInfo(info);
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

		[Command("#ScheduleModule_NextCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_DefaultNextCommand_Summary")]
		public async Task<CommandResult> NextCommand([Remainder] IdentifierInfo? info = null) {
			info = await ResolveNullInfo(info);
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

		[Command("#ScheduleModule_DayCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_DefaultWeekdayCommand_Summary")]
		public async Task<CommandResult> WeekdayCommand(DayOfWeek day, [Remainder] IdentifierInfo? info = null) {
			await RespondDay(info, DateTimeUtil.NextDayOfWeek(day, false));
			return m_Result;
		}

		[Command("#ScheduleModule_TodayCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_DefaultTomorrowCommand_Summary")]
		public async Task<CommandResult> TodayCommand([Remainder] IdentifierInfo? info = null) {
			await RespondDay(info, DateTime.Today);
			return m_Result;
		}

		[Command("#ScheduleModule_TomorrowCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_DefaultTodayCommand_Summary")]
		public async Task<CommandResult> TomorrowCommand([Remainder] IdentifierInfo? info = null) {
			await RespondDay(info, DateTime.Today.AddDays(1));
			return m_Result;
		}

		[Command("#ScheduleModule_ThisWeekCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_ShowThisWeekWorkingDays_Summary")]
		public async Task<CommandResult> ShowThisWeekWorkingDaysCommand([Remainder] IdentifierInfo? info = null) {
			await RespondWeek(info, 0);
			return m_Result;
		}

		[Command("#ScheduleModule_NextWeekCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_ShowNextWeekWorkingDays_Summary")]
		public async Task<CommandResult> ShowNextWeekWorkingDaysCommand([Remainder] IdentifierInfo? info = null) {
			await RespondWeek(info, 1);
			return m_Result;
		}

		[Command("#ScheduleModule_FutureCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_ShowNWeeksWorkingDays_Summary")]
		public async Task<CommandResult> ShowFutureCommand([Name("#ScheduleModule_ShowFutureCommand_AmountParameterName")] int amount, [Name("#ScheduleModule_ShowFutureCommand_UnitParameterName"), TypeDisplay("#ScheduleModule_ShowFutureCommand_UnitTypeDisplayName")] string unit, [Remainder] IdentifierInfo? info = null) {
			info = await ResolveNullInfo(info);
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
							m_LookedUpData = new LastScheduleCommandInfo(info, DateTime.Now + TimeSpan.FromHours(amount));
							await GetAfterCommand();
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

		[Command("#ScheduleModule_AfterCommand"), RunMode(RunMode.Parallel), IgnoresExtraArguments, Description("#ScheduleModule_AfterCommand_Summary")]
		public async Task<CommandResult> GetAfterCommand() {
			// This allows us to call !daarna automatically in certain conditions, and prevents the recursion from causing problems.
			await RespondAfter();
			return m_Result;
		}
		#endregion

		#region Record response functions
		protected async Task RespondRecord(string pretext, IdentifierInfo info, ScheduleRecord record, bool callNextIfBreak = true) {
			m_LookedUpData = new LastScheduleCommandInfo(info, record.End);
			IEnumerable<AspectListItem> aspects = record.Present(info);
			m_Result.AddResult(new AspectListResult(pretext, aspects));

			if (callNextIfBreak && record.ShouldCallNextCommand) {
				await RespondAfter(0);
			}
		}

		private async Task RespondDay(IdentifierInfo? info, DateTime date) {
			info = await ResolveNullInfo(info);
			if (info != null) {
				ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(info, date);
				if (result.Success) {
					ScheduleRecord[] records = result.Value;

					string relativeDateReference = DateTimeUtil.GetRelativeDateReference(date, Culture);

					if (records.Length == 0) {
						string response = GetString("ScheduleModule_RespondDay_NoRecordAtRelative", info.DisplayText, relativeDateReference);
						if (DateTimeUtil.IsWeekend(date)) {
							if (DateTimeUtil.IsWithinSameWeekend(date, DateTime.Today)) {
								response += GetString("ScheduleModule_ItIsWeekend");
							} else {
								response += GetString("ScheduleModule_ThatIsWeekend");
							}
						}
						m_Result.AddResult(new TextResult(null, response));
						m_LookedUpData = new LastScheduleCommandInfo(info, date);
					} else if (records.Length == 1) {
						string pretext = GetString("ScheduleModule_RespondDay_OnlyRecordForDay", info.DisplayText, relativeDateReference);
						await RespondRecord(pretext, info, records[0]);
					} else {
						string pretext = GetString("ScheduleModule_ResondDay_ScheduleForRelative", info.DisplayText, relativeDateReference);

						string[][] cells = new string[records.Length + 1][];
						cells[0] = new string[] {
							GetString("ScheduleModule_RespondDay_ColumnActivity"),
							GetString("ScheduleModule_RespondDay_ColumnTime"),
							GetString("ScheduleModule_RespondDay_ColumnStudentSets"),
							GetString("ScheduleModule_RespondDay_ColumnTeacher"),
							GetString("ScheduleModule_RespondDay_ColumnRoom")
						};

						int recordIndex = 1;
						foreach (ScheduleRecord record in records) {
							cells[recordIndex] = new string[] {
								record.Activity.DisplayText,
								$"{record.Start.ToShortTimeString(Culture)} - {record.End.ToShortTimeString(Culture)}",
								record.StudentSetsString,
								record.StaffMemberString,
								record.RoomString
							};

							recordIndex++;
						}
						m_LookedUpData = new LastScheduleCommandInfo(info, records.Last().End);
						m_Result.AddResult(new TableResult(pretext, cells));
					}
				}
			}
		}

		protected async Task RespondWeek(IdentifierInfo? info, int weeksFromNow) {
			info = await ResolveNullInfo(info);
			if (info != null) {
				ScheduleRecord[] weekRecords = await Schedules.GetWeekRecordsAsync(info, weeksFromNow, Context);
				if (weekRecords.Length > 0) {
					string caption;
					if (weeksFromNow == 0) {
						caption = GetString("ScheduleModule_RespondWeek_ScheduleThisWeek", info);
					} else if (weeksFromNow == 1) {
						caption = GetString("ScheduleModule_RespondWeek_ScheduleNextWeek", info);
					} else {
						caption = GetString("ScheduleModule_RespondWeek_ScheduleInXWeeks", info, weeksFromNow);
					}

					var dayRecords = weekRecords.GroupBy(record => record.Start.DayOfWeek).ToDictionary(
						/* Key select */ group => group.Key,
						/* Val select */ group => group.ToArray()
					);
					int longestColumn = dayRecords.Max(kvp => kvp.Value.Length);

					// Header
					string[][] cells = new string[longestColumn + 2][];
					cells[0] = Enumerable.Range(1, 5).Select(dow => ((DayOfWeek) dow).GetName(Culture)).ToArray(); // Outputs day names of Monday through Friday

					// Initialize cells to empty strings
					for (int i = 1; i < cells.Length; i++) {
						cells[i] = new string[5];
						for (int j = 0; j < cells[i].Length; j++) {
							cells[i][j] = "";
						}
					}

					foreach (KeyValuePair<DayOfWeek, ScheduleRecord[]> kvp in dayRecords) {
						for (int i = 0; i < kvp.Value.Length; i++) {
							cells[i + 2][(int) kvp.Key - 1] = kvp.Value[i].Activity.DisplayText;
						}
					}

					AvailabilityInfo[] availabilities;
					availabilities = new AvailabilityInfo[5];
					foreach (KeyValuePair<DayOfWeek, ScheduleRecord[]> kvp in dayRecords) {
						availabilities[(int) kvp.Key - 1] = new AvailabilityInfo(kvp.Value.First().Start, kvp.Value.Last().End);
					}

					// Time of day start/end, and set to "---" if empty
					for (DayOfWeek dow = DayOfWeek.Monday; dow <= DayOfWeek.Friday; dow++) {
						if (!dayRecords.ContainsKey(dow)) {
							cells[2][(int) dow - 1] = "---"; // dow - 1 because 0 is Sunday
						} else {
							AvailabilityInfo dayAvailability = availabilities[(int) dow - 1];
							cells[1][(int) dow - 1] = dayAvailability.StartOfAvailability.ToString("HH:mm") + " - " + dayAvailability.EndOfAvailability.ToString("HH:mm");
						}
					}

					m_Result.AddResult(new TableResult(caption, cells));
				} else {
					string response;
					if (weeksFromNow == 0) {
						response = GetString("ScheduleModule_RespondWorkingDays_NotOnScheduleThisWeek", info);
					} else if (weeksFromNow == 1) {
						response = GetString("ScheduleModule_RespondWorkingDays_NotOnScheduleNextWeek", info);
					} else {
						response = GetString("ScheduleModule_RespondWorkingDays_NotOnScheduleInXWeeks", info, weeksFromNow);
					}
					m_Result.AddResult(new TextResult(null, response));
				}
			}
		}

		protected async Task RespondAfter(int recursion = 0) {
			LastScheduleCommandInfo? query;
			if (m_LookedUpData != null) {
				query = new LastScheduleCommandInfo(m_LookedUpData.Identifier, m_LookedUpData.RecordEndTime);
			} else {
				query = UserConfig.GetLastScheduleCommand(Context.Channel);
			}
			if (query == null) {
				MinorError(GetString("ScheduleModule_GetAfterCommand_NoContext"));
			} else {
				ReturnValue<ScheduleRecord> nextRecord = await GetRecordAfterDateTime(query.Identifier, (query.RecordEndTime == null ? DateTime.Now : query.RecordEndTime.Value) - TimeSpan.FromSeconds(1));

				if (nextRecord.Success) {
					string pretext;
					if (query.RecordEndTime == null) {
						pretext = GetString("ScheduleModule_PretextNext", query.Identifier.DisplayText);
					} else if (query.RecordEndTime.Value.Date != nextRecord.Value.Start.Date) {
						pretext = GetString("ScheduleModule_Pretext_FirstOn", query.Identifier.DisplayText, DateTimeUtil.GetStringFromDayOfWeek(Culture, nextRecord.Value.Start.DayOfWeek));
					} else {
						pretext = GetString("ScheduleModule_PretextAfterPrevious", query.Identifier.DisplayText);
					}

					// Avoid RespondRecord automatically calling this function again because we do it ourselves
					// We don't use RespondRecord's handling because we have our own recursion limit, which RespondRecord can't use
					await RespondRecord(pretext, query.Identifier, nextRecord.Value, false);

					if (nextRecord.Value.ShouldCallNextCommand && recursion <= 5) {
						await RespondAfter(recursion + 1);
					}
				}
			}
		}
		#endregion

		#region Convenience
		private Task<IdentifierInfo?> ResolveNullInfo(IdentifierInfo? info) {
			if (info == null) {
				StudentSetInfo? ssi = Context.UserConfig.GetStudentSet();
				if (ssi != null) {
					return Task.FromResult((IdentifierInfo?) ssi);
				} else {
					MinorError(GetString("StudentSetInfoReader_CheckFailed_MentionSelf"));
					return Task.FromResult((IdentifierInfo?) null);
				}
			} else {
				return Task.FromResult((IdentifierInfo?) info);
			}
		}

		protected Task<ReturnValue<ScheduleRecord?>> GetRecordAtDateTime(IdentifierInfo identifier, DateTime datetime) {
			return HandleErrorAsync(() => Schedules.GetRecordAtDateTime(identifier, datetime, Context));
		}

		protected Task<ReturnValue<ScheduleRecord>> GetRecordAfterDateTime(IdentifierInfo identifier, DateTime datetime) {
			return HandleErrorAsync(() => Schedules.GetRecordAfterDateTime(identifier, datetime, Context));
		}

		protected Task<ReturnValue<ScheduleRecord[]>> GetSchedulesForDay(IdentifierInfo identifier, DateTime date) {
			return HandleErrorAsync(() => Schedules.GetSchedulesForDate(identifier, date, Context));
		}

		protected Task<ReturnValue<AvailabilityInfo[]>> GetWeekAvailability(IdentifierInfo identifier, int weeksFromNow) {
			return HandleErrorAsync(() => Schedules.GetWeekAvailability(identifier, weeksFromNow, Context));
		}

		private async Task<ReturnValue<T>> HandleErrorAsync<T>(Func<Task<T>> action) {
			try {
				return new ReturnValue<T>(await action());
			} catch (IdentifierNotFoundException) {
				MinorError(GetString("ScheduleModule_HandleError_NotFound"));
			} catch (RecordsOutdatedException) {
				MinorError(GetString("ScheduleModule_HandleError_RecordsOutdated"));
			} catch (NoAllowedGuildsException) {
				MinorError(GetString("ScheduleModule_HandleError_NoSchedulesAvailableForServer"));
			}
			return new ReturnValue<T>();
		}
		#endregion

		#region Overrides
		protected override TextResult MinorError(string message) {
			TextResult result = base.MinorError(message);
			m_Result.AddResult(result);
			return result;
		}

		protected override ValueTask AfterExecutedAsync() {
			if (m_LookedUpData != null) {
				UserConfig.OnScheduleRequestByUser(Context.Channel, m_LookedUpData);
			} else {
				UserConfig.RemoveLastScheduleCommand(Context.Channel);
			}
			return default;
		}
		#endregion
	}
}
