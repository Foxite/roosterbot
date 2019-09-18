using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	// TODO reduce the size of this file (it has 352 lines right now)
	[LogTag("ScheduleModule")]
	[Name("#DefaultScheduleModule_Name")]
	[Summary("#DefaultScheduleModule_Summary")]
	[Remarks("#DefaultScheduleModule_Remarks")]
	public class ScheduleModule : EditableCmdModuleBase {
		public LastScheduleCommandService LSCService { get; set; }
		public ScheduleService Schedules { get; set; }

		#region Commands
		[Command("nu", RunMode = RunMode.Async), Alias("rooster"), Summary("#DefaultScheduleModule_DefaultCurrentCommand_Summary")]
		public async Task CurrentCommand([Remainder] IdentifierInfo info) {
			ReturnValue<ScheduleRecord> result = await GetCurrentRecord(info);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = GetString("ScheduleModule_CurrentCommand_NoCurrentRecord", info.DisplayText);

					if (ScheduleUtil.IsWeekend(DateTime.Today)) {
						response += GetString("ScheduleModuleBase_ItIsWeekend");
					}

					ReplyDeferred(response, info, record);
				} else {
					await RespondRecord(GetString("ScheduleModuleBase_PretextNow", info.DisplayText), info, record);
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen"), Summary("#DefaultScheduleModule_DefaultNextCommand_Summary")]
		public async Task NextCommand([Remainder] IdentifierInfo info) {
			ReturnValue<ScheduleRecord> result = await GetNextRecord(info);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				string pretext;
				if (record.Start.Date == DateTime.Today) {
					pretext = GetString("ScheduleModuleBase_PretextNext", info.DisplayText);
				} else {
					pretext = GetString("ScheduleModuleBase_Pretext_FirstOn", info.DisplayText, record.Start.DayOfWeek.GetName(Culture));
				}
				await RespondRecord(pretext, info, record);
			}
		}

		[Command("dag", RunMode = RunMode.Async), Summary("#DefaultScheduleModule_DefaultWeekdayCommand_Summary")]
		public async Task WeekdayCommand(DayOfWeek day, [Remainder] IdentifierInfo info) {
			await RespondDay(info, ScheduleUtil.NextDayOfWeek(day, false));
		}

		[Command("vandaag", RunMode = RunMode.Async), Summary("#DefaultScheduleModule_DefaultTomorrowCommand_Summary")]
		public async Task TodayCommand([Remainder] IdentifierInfo info) {
			await RespondDay(info, DateTime.Today);
		}

		[Command("morgen", RunMode = RunMode.Async), Summary("#DefaultScheduleModule_DefaultTodayCommand_Summary")]
		public async Task TomorrowCommand([Remainder] IdentifierInfo info) {
			await RespondDay(info, DateTime.Today.AddDays(1));
		}

		[Command("deze week", RunMode = RunMode.Async), Summary("#ScheduleModuleBase_ShowThisWeekWorkingDays_Summary")]
		public async Task ShowThisWeekWorkingDaysCommand([Remainder] IdentifierInfo info) {
			await RespondWorkingDays(info, 0);
		}

		[Command("volgende week", RunMode = RunMode.Async), Summary("#ScheduleModuleBase_ShowNextWeekWorkingDays_Summary")]
		public async Task ShowNextWeekWorkingDaysCommand([Remainder] IdentifierInfo info) {
			await RespondWorkingDays(info, 1);
		}

		[Command("over", RunMode = RunMode.Async), Summary("#ScheduleModuleBase_ShowNWeeksWorkingDays_Summary")]
		public async Task ShowFutureCommand(int amount, [Name("#ScheduleModule_ShowFutureCommand_UnitParameterName")] string unit, [Remainder] IdentifierInfo info) {
			unit = unit.ToLower();
			if (GetString("ScheduleModule_ShowFutureCommand_UnitHours").Split('|').Contains(unit)) {
				ReturnValue <ScheduleRecord> result = await GetRecordAfterTimeSpan(info, TimeSpan.FromHours(amount));
				if (result.Success) {
					ScheduleRecord record = result.Value;
					if (record != null) {
						await RespondRecord(GetString("ScheduleModuleBase_InXHours", info.DisplayText, amount), info, record);
					} else {
						await ReplyAsync(GetString("ScheduleModuleBase_ShowFutureCommand_NoRecordAtThatTime"));
					}
				}
			} else if (GetString("ScheduleModule_ShowFutureCommand_UnitDays").Split('|').Contains(unit)) {
				await RespondDay(info, DateTime.Today.AddDays(amount));
			} else if (GetString("ScheduleModule_ShowFutureCommand_UnitWeeks").Split('|').Contains(unit)) {
				await RespondWorkingDays(info, amount);
			} else {
				await MinorError(GetString("ScheduleModuleBase_ShowFutureCommand_OnlySupportUnits"));
			}
		}

		[Command("daarna", RunMode = RunMode.Async), Summary("#ScheduleModuleBase_AfterCommand")]
		public async Task GetAfterCommand([Remainder] string ignored = "") {
			if (!string.IsNullOrWhiteSpace(ignored)) {
				ReplyDeferred(GetString("AfterScheduleModule_GetAfterCommand_ParameterHint"));
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
			ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(info, date);
			if (result.Success) {
				ScheduleRecord[] records = result.Value;

				string relativeDateReference;

				if (date == DateTime.Today) {
					relativeDateReference = GetString("ScheduleUtil_RelativeDateReference_Today");
				} else if (date == DateTime.Today.AddDays(1)) {
					relativeDateReference = GetString("ScheduleUtil_RelativeDateReference_Tomorrow");
				} else if ((date - DateTime.Today).TotalDays < 7) {
					relativeDateReference = GetString("ScheduleUtil_RelativeDateReference_DayName", date.DayOfWeek.GetName(Culture));
				} else {
					relativeDateReference = GetString("ScheduleUtil_RelativeDateReference_Date", date.ToShortDateString(Culture));
				}

				if (records.Length == 0) {
					string response = GetString("ScheduleModule_RespondDay_NoRecordAtRelative", info.DisplayText, relativeDateReference);
					if (ScheduleUtil.IsWeekend(date)) {
						if (ScheduleUtil.IsWithinSameWeekend(date, DateTime.Today)) {
							response += GetString("ScheduleModuleBase_ItIsWeekend");
						} else {
							response += GetString("ScheduleModuleBase_ThatIsWeekend");
						}
					}
					ReplyDeferred(response, info, null);
				} else if (records.Length == 1) {
					string pretext = GetString("ScheduleModule_RespondDay_OnlyRecordForDay", info.DisplayText, relativeDateReference);
					await RespondRecord(pretext, info, records[0]);
				} else {
					string response = GetString("ScheduleModuleBase_ResondDay_ScheduleForRelative", info.DisplayText, relativeDateReference);

					string[][] cells = new string[records.Length + 1][];
					cells[0] = new string[] {
						GetString("ScheduleModuleBase_RespondDay_ColumnActivity"),
						GetString("ScheduleModuleBase_RespondDay_ColumnTime"),
						GetString("ScheduleModuleBase_RespondDay_ColumnStudentSets"),
						GetString("ScheduleModuleBase_RespondDay_ColumnTeacher"),
						GetString("ScheduleModuleBase_RespondDay_ColumnRoom")
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

		private async Task RespondWorkingDays(IdentifierInfo info, int weeksFromNow) {
			ReturnValue<AvailabilityInfo[]> result = await GetWeekAvailability(info, weeksFromNow);
			if (result.Success) {
				AvailabilityInfo[] availability = result.Value;

				string response;

				if (availability.Length > 1) {
					if (weeksFromNow == 0) {
						response = GetString("ScheduleModuleBase_ScheduleThisWeek", info.DisplayText);
					} else if (weeksFromNow == 1) {
						response = GetString("ScheduleModuleBase_ScheduleNextWeek", info.DisplayText);
					} else {
						response = GetString("ScheduleModuleBase_ScheduleInXWeeks", info.DisplayText, weeksFromNow);
					}
					response += "\n";

					string[][] cells = new string[availability.Length + 1][];
					cells[0] = new[] {
						GetString("ScheduleModuleBase_RespondWorkingDays_ColumnDay"),
						GetString("ScheduleModuleBase_RespondWorkingDays_ColumnFrom"),
						GetString("ScheduleModuleBase_RespondWorkingDays_ColumnTo")
					};

					int i = 1;
					foreach (AvailabilityInfo item in availability) {
						cells[i] = new[] {
							item.StartOfAvailability.DayOfWeek.GetName(Culture),
							item.StartOfAvailability.ToShortTimeString(Culture),
							item.EndOfAvailability.ToShortTimeString(Culture)
						};
						i++;
					}
					response += Util.FormatTextTable(cells);
				} else {
					if (weeksFromNow == 0) {
						response = GetString("ScheduleModule_RespondWorkingDays_NotOnScheduleThisWeek", info.DisplayText);
					} else if (weeksFromNow == 1) {
						response = GetString("ScheduleModule_RespondWorkingDays_NotOnScheduleNextWeek", info.DisplayText);
					} else {
						response = GetString("ScheduleModule_RespondWorkingDays_NotOnScheduleInXWeeks", info.DisplayText, weeksFromNow);
					}
				}

				ReplyDeferred(response);
			}
		}

		protected async Task RespondAfter(int recursion = 0) {
			ScheduleCommandInfo query = LSCService.GetLastCommandForContext(Context);
			if (query.Equals(default(ScheduleCommandInfo))) {
				await MinorError(GetString("AfterScheduleModule_GetAfterCommand_NoContext"));
			} else {
				ScheduleRecord nextRecord;
				try {
					if (query.Record == null) {
						nextRecord = await Schedules.GetNextRecord(query.Identifier, Context);
					} else {
						nextRecord = await Schedules.GetRecordAfter(query.Identifier, query.Record, Context);
					}
				} catch (RecordsOutdatedException) {
					await MinorError(GetString("AfterScheduleModule_GetAfterCommand_RecordsOutdated"));
					return;
				} catch (IdentifierNotFoundException) {
					// This catch block scores 9 out of 10 on the "oh shit" scale
					// It should never happen and it indicates that something has really been messed up somewhere, but it's not quite bad enough for a 10.
					string report = $"daarna failed for query {query.Identifier.ScheduleField}:{query.Identifier}";
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
					pretext = GetString("ScheduleModuleBase_PretextNext", query.Identifier.DisplayText);
				} else {
					pretext = GetString("ScheduleModuleBase_PretextAfterPrevious", query.Identifier.DisplayText);
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
			try {
				return new ReturnValue<T>() {
					Success = true,
					Value = await action()
				};
			} catch (IdentifierNotFoundException) {
				await MinorError(GetString("ScheduleModuleBase_HandleError_NotFound"));
				return new ReturnValue<T>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError(GetString("ScheduleModuleBase_HandleError_RecordsOutdated"));
				return new ReturnValue<T>() {
					Success = false
				};
			} catch (NoAllowedGuildsException) {
				await MinorError(GetString("ScheduleModuleBase_HandleError_NoSchedulesAvailableForServer"));
				return new ReturnValue<T>() {
					Success = false
				};
			} catch (Exception ex) {
				await FatalError("Uncaught exception", ex);
				return new ReturnValue<T>() {
					Success = false
				};
			}
		}
		#endregion
	}
}
