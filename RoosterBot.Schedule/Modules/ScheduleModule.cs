using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoosterBot.DateTimeUtils;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	// TODO (refactoring) reduce the size of this file (it has 352 lines right now)
	[Name("#ScheduleModule_Name")]
	[Summary("#ScheduleModule_Summary")]
	[Remarks("#ScheduleModule_Remarks")]
	[LocalizedModule("nl-NL", "en-US")]
	public class ScheduleModule : RoosterModuleBase {
		public LastScheduleCommandService LSCService { get; set; }
		public ScheduleService Schedules { get; set; }

		#region Commands
		[Command("#ScheduleModule_NowCommand", RunMode = RunMode.Async), Summary("#ScheduleModule_DefaultCurrentCommand_Summary")]
		public async Task CurrentCommand([Remainder] IdentifierInfo info = null) {
			info = await ResolveNullInfo(info);
			if (info != null) {
				ReturnValue<ScheduleRecord> result = await GetCurrentRecord(info);
				if (result.Success) {
					ScheduleRecord record = result.Value;
					if (record == null) {
						string response = GetString("ScheduleModule_CurrentCommand_NoCurrentRecord", info.DisplayText);

						if (DateTimeUtil.IsWeekend(DateTime.Today)) {
							response += GetString("ScheduleModule_ItIsWeekend");
						}

						ReplyDeferred(response, info, record);
						await NextCommand(info);
					} else {
						await RespondRecord(GetString("ScheduleModule_PretextNow", info.DisplayText), info, record);
					}
				}
			}
		}

		[Command("#ScheduleModule_NextCommand", RunMode = RunMode.Async), Summary("#ScheduleModule_DefaultNextCommand_Summary")]
		public async Task NextCommand([Remainder] IdentifierInfo info = null) {
			info = await ResolveNullInfo(info);
			if (info != null) {
				ReturnValue<ScheduleRecord> result = await GetNextRecord(info);
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

		[Command("#ScheduleModule_DayCommand", RunMode = RunMode.Async), Summary("#ScheduleModule_DefaultWeekdayCommand_Summary")]
		public async Task WeekdayCommand(DayOfWeek day, [Remainder] IdentifierInfo info = null) {
			await RespondDay(info, DateTimeUtil.NextDayOfWeek(day, false));
		}

		[Command("#ScheduleModule_TodayCommand", RunMode = RunMode.Async), Summary("#ScheduleModule_DefaultTomorrowCommand_Summary")]
		public async Task TodayCommand([Remainder] IdentifierInfo info = null) {
			await RespondDay(info, DateTime.Today);
		}

		[Command("#ScheduleModule_TomorrowCommand", RunMode = RunMode.Async), Summary("#ScheduleModule_DefaultTodayCommand_Summary")]
		public async Task TomorrowCommand([Remainder] IdentifierInfo info = null) {
			await RespondDay(info, DateTime.Today.AddDays(1));
		}

		[Command("#ScheduleModule_ThisWeekCommand", RunMode = RunMode.Async), Summary("#ScheduleModule_ShowThisWeekWorkingDays_Summary")]
		public async Task ShowThisWeekWorkingDaysCommand([Remainder] IdentifierInfo info = null) {
			await RespondWeek(info, 0);
		}

		[Command("#ScheduleModule_NextWeekCommand", RunMode = RunMode.Async), Summary("#ScheduleModule_ShowNextWeekWorkingDays_Summary")]
		public async Task ShowNextWeekWorkingDaysCommand([Remainder] IdentifierInfo info = null) {
			await RespondWeek(info, 1);
		}

		[Command("#ScheduleModule_FutureCommand", RunMode = RunMode.Async), Summary("#ScheduleModule_ShowNWeeksWorkingDays_Summary")]
		public async Task ShowFutureCommand(int amount, [Name("#ScheduleModule_ShowFutureCommand_UnitParameterName")] string unit, [Remainder] IdentifierInfo info = null) {
			info = await ResolveNullInfo(info);
			if (info != null) {
				unit = unit.ToLower();
				if (GetString("ScheduleModule_ShowFutureCommand_UnitHours").Split('|').Contains(unit)) {
					ReturnValue<ScheduleRecord> result = await GetRecordAfterTimeSpan(info, TimeSpan.FromHours(amount));
					if (result.Success) {
						ScheduleRecord record = result.Value;
						if (record != null) {
							await RespondRecord(GetString("ScheduleModule_InXHours", info.DisplayText, amount), info, record);
						} else {
							await ReplyAsync(GetString("ScheduleModule_ShowFutureCommand_NoRecordAtThatTime"));
						}
					}
				} else if (GetString("ScheduleModule_ShowFutureCommand_UnitDays").Split('|').Contains(unit)) {
					await RespondDay(info, DateTime.Today.AddDays(amount));
				} else if (GetString("ScheduleModule_ShowFutureCommand_UnitWeeks").Split('|').Contains(unit)) {
					await RespondWeek(info, amount);
				} else {
					await MinorError(GetString("ScheduleModule_ShowFutureCommand_OnlySupportUnits"));
				}
			}
		}

		[Command("#ScheduleModule_AfterCommand", RunMode = RunMode.Async), Summary("#ScheduleModule_AfterCommand_Summary")]
		public async Task GetAfterCommand([Remainder] string ignored = "") {
			if (!string.IsNullOrWhiteSpace(ignored)) {
				ReplyDeferred(GetString("ScheduleModule_GetAfterCommand_ParameterHint"), GuildConfig.CommandPrefix);
			}
			// This allows us to call !daarna automatically in certain conditions, and prevents the recursion from causing problems.
			await RespondAfter();
		}
		#endregion

		#region Reply functions
		/// <summary>
		/// Posts a message in Context.Channel with the given text, and adds given schedule, identifier, and record to the LastScheduleCommandService for use in the !daarna command.
		/// </summary>
		protected async Task<IUserMessage> ReplyAsync(string message, IdentifierInfo identifier, ScheduleRecord record, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			IUserMessage ret = await base.ReplyAsync(message, isTTS, embed, options);
			LSCService.OnRequestByUser(Context, identifier, record);
			return ret;
		}

		/// <summary>
		/// Posts a message in Context.Channel with the given text, and adds given schedule, identifier, and record to the LastScheduleCommandService for use in the !daarna command.
		/// </summary>
		protected void ReplyDeferred(string message, IdentifierInfo identifier, ScheduleRecord record) {
			base.ReplyDeferred(message);
			LSCService.OnRequestByUser(Context, identifier, record);
		}

		protected async override Task MinorError(string message) {
			await base.MinorError(message);
			LSCService.RemoveLastQuery(Context);
		}

		protected async override Task FatalError(string message, Exception exception = null) {
			await base.FatalError(message, exception);
			LSCService.RemoveLastQuery(Context);
		}
		#endregion

		#region Record response functions
		protected async Task RespondRecord(string pretext, IdentifierInfo info, ScheduleRecord record, bool callNextIfBreak = true) {
			string response = pretext + "\n";
			response += await record.PresentAsync(info);
			ReplyDeferred(response, info, record);

			if (callNextIfBreak && record.ShouldCallNextCommand) {
				await RespondAfter(0);
			}
		}

		private async Task RespondDay(IdentifierInfo info, DateTime date) {
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
						ReplyDeferred(response, info, null);
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
							cells[recordIndex] = new string[5];
							cells[recordIndex][0] = record.Activity.DisplayText;
							cells[recordIndex][1] = $"{record.Start.ToShortTimeString(Culture)} - {record.End.ToShortTimeString(Culture)}";
							cells[recordIndex][2] = record.StudentSetsString;
							cells[recordIndex][3] = record.StaffMemberString;
							cells[recordIndex][4] = record.RoomString;

							recordIndex++;
						}
						response += Util.FormatTextTable(cells);
						ReplyDeferred(response, info, records.Last());
					}
				}
			}
		}

		protected async Task RespondWeek(IdentifierInfo info, int weeksFromNow) {
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

					response += Util.FormatTextTable(cells);
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
			ScheduleCommandInfo query = LSCService.GetLastCommandForContext(Context);
			if (query.Equals(default(ScheduleCommandInfo))) {
				await MinorError(GetString("ScheduleModule_GetAfterCommand_NoContext"));
			} else {
				ScheduleRecord nextRecord;
				try {
					if (query.Record == null) {
						nextRecord = await Schedules.GetNextRecord(query.Identifier, Context);
					} else {
						nextRecord = await Schedules.GetRecordAfter(query.Identifier, query.Record, Context);
					}
				} catch (RecordsOutdatedException) {
					await MinorError(GetString("ScheduleModule_GetAfterCommand_RecordsOutdated"));
					return;
				} catch (IdentifierNotFoundException) {
					// This catch block scores 9 out of 10 on the "oh shit" scale
					// It should never happen and it indicates that something has really been messed up somewhere, but it's not quite bad enough for a 10.
					string report = $"daarna failed for query {query.Identifier}";
					if (query.Record == null) {
						report += " with no record";
					} else {
						report += $" with record: {query.Record.ToString()}";
					}

					await FatalError(report);
					return;
				} catch (Exception ex) {
					await FatalError("Uncaught exception", ex);
					return;
				}

				string pretext;
				if (query.Record == null) {
					pretext = GetString("ScheduleModule_PretextNext", query.Identifier.DisplayText);
				} else if (query.Record.Start.Date != nextRecord.Start.Date) {
					pretext = GetString("ScheduleModule_Pretext_FirstOn", query.Identifier.DisplayText, DateTimeUtil.GetStringFromDayOfWeek(Culture, nextRecord.Start.DayOfWeek));
				} else {
					pretext = GetString("ScheduleModule_PretextAfterPrevious", query.Identifier.DisplayText);
				}
				
				// Avoid RespondRecord automatically calling this function again because we do it ourselves
				// We don't use RespondRecord's handling because we have our own recursion limit, which RespondRecord can't use
				await RespondRecord(pretext, query.Identifier, nextRecord, false);

				if (nextRecord.ShouldCallNextCommand && recursion <= 5) {
					await RespondAfter(recursion + 1);
				}
			}
		}
		#endregion

		#region Convenience
		private async Task<IdentifierInfo> ResolveNullInfo(IdentifierInfo info) {
			if (info == null) {
				TypeReaderResult infoResult = await ScheduleComponent.s_IdentifierReaders.ReadAsync(Context, "ik", Program.Instance.Components.Services);
				if (infoResult.IsSuccess) {
					return (IdentifierInfo) infoResult.BestMatch;
				} else {
					ReplyDeferred(Util.Error + infoResult.ErrorReason);
					return null;
				}
			} else {
				return info;
			}
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetCurrentRecord(IdentifierInfo identifier) {
			return await HandleErrorAsync(() => Schedules.GetCurrentRecord(identifier, Context));
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetNextRecord(IdentifierInfo identifier) {
			return await HandleErrorAsync(() => Schedules.GetNextRecord(identifier, Context));
		}

		protected async Task<ReturnValue<ScheduleRecord[]>> GetSchedulesForDay(IdentifierInfo identifier, DateTime date) {
			return await HandleErrorAsync(() => Schedules.GetSchedulesForDate(identifier, date, Context));
		}

		protected async Task<ReturnValue<AvailabilityInfo[]>> GetWeekAvailability(IdentifierInfo identifier, int weeksFromNow) {
			return await HandleErrorAsync(() => Schedules.GetWeekAvailability(identifier, weeksFromNow, Context));
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetRecordAfterTimeSpan(IdentifierInfo identifier, TimeSpan span) {
			return await HandleErrorAsync(() => Schedules.GetRecordAfterTimeSpan(identifier, span, Context));
		}

		private async Task<ReturnValue<T>> HandleErrorAsync<T>(Func<Task<T>> action) {
			ReturnValue<T> failure = new ReturnValue<T>() {
				Success = false
			};
			try {
				return new ReturnValue<T>() {
					Success = true,
					Value = await action()
				};
			} catch (IdentifierNotFoundException) {
				await MinorError(GetString("ScheduleModule_HandleError_NotFound"));
				return failure;
			} catch (RecordsOutdatedException) {
				await MinorError(GetString("ScheduleModule_HandleError_RecordsOutdated"));
				return failure;
			} catch (NoAllowedGuildsException) {
				await MinorError(GetString("ScheduleModule_HandleError_NoSchedulesAvailableForServer"));
				return failure;
			} catch (Exception ex) {
				await FatalError("Uncaught exception", ex);
				return failure;
			}
		}
		#endregion
	}
}
