﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoosterBot.DateTimeUtils;
using Discord;
using Qmmands;

namespace RoosterBot.Schedule {
	// TODO (refactor) reduce the size of this file (it has 353 lines right now)
	[Name("#ScheduleModule_Name")]
	[Description("#ScheduleModule_Summary")]
	[Remarks("#ScheduleModule_Remarks")]
	[LocalizedModule("nl-NL", "en-US")]
	public class ScheduleModule : RoosterModuleBase {
		private IdentifierInfo? m_LookedUpIdentifier;
		private DateTime? m_LookedUpRecordEndTime;

		public ScheduleService Schedules { get; set; } = null!;

		#region Commands
		[Command("#ScheduleModule_NowCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_DefaultCurrentCommand_Summary")]
		public async Task CurrentCommand([Remainder] IdentifierInfo? info = null) {
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

						ReplyDeferred(response, info, DateTime.Now);
						await NextCommand(info);
					} else {
						await RespondRecord(GetString("ScheduleModule_PretextNow", info.DisplayText), info, record);
					}
				}
			}
		}

		[Command("#ScheduleModule_NextCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_DefaultNextCommand_Summary")]
		public async Task NextCommand([Remainder] IdentifierInfo? info = null) {
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
		}

		[Command("#ScheduleModule_DayCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_DefaultWeekdayCommand_Summary")]
		public async Task WeekdayCommand(DayOfWeek day, [Remainder] IdentifierInfo? info = null) {
			await RespondDay(info, DateTimeUtil.NextDayOfWeek(day, false));
		}

		[Command("#ScheduleModule_TodayCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_DefaultTomorrowCommand_Summary")]
		public async Task TodayCommand([Remainder] IdentifierInfo? info = null) {
			await RespondDay(info, DateTime.Today);
		}

		[Command("#ScheduleModule_TomorrowCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_DefaultTodayCommand_Summary")]
		public async Task TomorrowCommand([Remainder] IdentifierInfo? info = null) {
			await RespondDay(info, DateTime.Today.AddDays(1));
		}

		[Command("#ScheduleModule_ThisWeekCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_ShowThisWeekWorkingDays_Summary")]
		public async Task ShowThisWeekWorkingDaysCommand([Remainder] IdentifierInfo? info = null) {
			await RespondWeek(info, 0);
		}

		[Command("#ScheduleModule_NextWeekCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_ShowNextWeekWorkingDays_Summary")]
		public async Task ShowNextWeekWorkingDaysCommand([Remainder] IdentifierInfo? info = null) {
			await RespondWeek(info, 1);
		}

		[Command("#ScheduleModule_FutureCommand"), RunMode(RunMode.Parallel), Description("#ScheduleModule_ShowNWeeksWorkingDays_Summary")]
		public async Task ShowFutureCommand([Name("#ScheduleModule_ShowFutureCommand_AmountParameterName")] int amount, [Name("#ScheduleModule_ShowFutureCommand_UnitParameterName"), TypeDisplay("#ScheduleModule_ShowFutureCommand_UnitTypeDisplayName")] string unit, [Remainder] IdentifierInfo? info = null) {
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
							ReplyDeferred(GetString("ScheduleModule_ShowFutureCommand_NoRecordAtThatTime"), info, DateTime.Now + TimeSpan.FromHours(amount));
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
		}

		[Command("#ScheduleModule_AfterCommand"), RunMode(RunMode.Parallel), IgnoresExtraArguments, Description("#ScheduleModule_AfterCommand_Summary")]
		public async Task GetAfterCommand() {
			// This allows us to call !daarna automatically in certain conditions, and prevents the recursion from causing problems.
			await RespondAfter();
		}
		#endregion

		#region Record response functions
		protected async Task RespondRecord(string pretext, IdentifierInfo info, ScheduleRecord record, bool callNextIfBreak = true) {
			string response = pretext + "\n";
			response += record.Present(info);
			ReplyDeferred(response, info, record.End);

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
						ReplyDeferred(response, info, date);
					} else if (records.Length == 1) {
						string pretext = GetString("ScheduleModule_RespondDay_OnlyRecordForDay", info.DisplayText, relativeDateReference);
						await RespondRecord(pretext, info, records[0]);
					} else {
						string response = GetString("ScheduleModule_ResondDay_ScheduleForRelative", info.DisplayText, relativeDateReference);

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
						response += StringUtil.FormatTextTable(cells);
						ReplyDeferred(response, info, records.Last().End);
					}
				}
			}
		}

		protected async Task RespondWeek(IdentifierInfo? info, int weeksFromNow) {
			info = await ResolveNullInfo(info);
			if (info != null) {
				string response;
				ScheduleRecord[] weekRecords = await Schedules.GetWeekRecordsAsync(info, weeksFromNow, Context);
				if (weekRecords.Length > 0) {
					if (weeksFromNow == 0) {
						response = GetString("ScheduleModule_RespondWeek_ScheduleThisWeek", info);
					} else if (weeksFromNow == 1) {
						response = GetString("ScheduleModule_RespondWeek_ScheduleNextWeek", info);
					} else {
						response = GetString("ScheduleModule_RespondWeek_ScheduleInXWeeks", info, weeksFromNow);
					}

					Dictionary<DayOfWeek, ScheduleRecord[]> dayRecords = weekRecords.GroupBy(record => record.Start.DayOfWeek).ToDictionary(
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

					response += StringUtil.FormatTextTable(cells);
				} else {
					if (weeksFromNow == 0) {
						response = GetString("ScheduleModule_RespondWorkingDays_NotOnScheduleThisWeek", info);
					} else if (weeksFromNow == 1) {
						response = GetString("ScheduleModule_RespondWorkingDays_NotOnScheduleNextWeek", info);
					} else {
						response = GetString("ScheduleModule_RespondWorkingDays_NotOnScheduleInXWeeks", info, weeksFromNow);
					}
				}
				ReplyDeferred(response);

			}
		}

		protected async Task RespondAfter(int recursion = 0) {
			LastScheduleCommandInfo? query;
			if (m_LookedUpIdentifier != null) {
				query = new LastScheduleCommandInfo(m_LookedUpIdentifier, m_LookedUpRecordEndTime);
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
		/// <summary>
		/// Posts a message in Context.Channel with the given text, and records the given identifier and record end time for use in the !daarna command.
		/// </summary>
		protected void ReplyDeferred(string message, IdentifierInfo identifier, DateTime recordEndTime) {
			base.ReplyDeferred(message);

			m_LookedUpIdentifier = identifier;
			m_LookedUpRecordEndTime = recordEndTime;
		}

		protected async override Task<IUserMessage?> SendDeferredResponseAsync() {
			if (m_LookedUpIdentifier != null && m_LookedUpRecordEndTime != null) {
				UserConfig.OnScheduleRequestByUser(Context.Channel, m_LookedUpIdentifier, m_LookedUpRecordEndTime.Value);
			} else {
				UserConfig.RemoveLastScheduleCommand(Context.Channel);
			}

			return await base.SendDeferredResponseAsync();
		}
		#endregion
	}
}
