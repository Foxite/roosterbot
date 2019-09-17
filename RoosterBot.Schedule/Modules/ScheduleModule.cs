using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	// TODO reduce the size of this file (it has 352 lines right now)
	public class ScheduleModule : EditableCmdModuleBase {
		public LastScheduleCommandService LSCService { get; set; }
		public ScheduleService Schedules { get; set; }

		#region Commands
		[Command("nu", RunMode = RunMode.Async)]
		public async Task StudentCurrentCommand([Remainder] IdentifierInfo info) {
			ReturnValue<ScheduleRecord> result = await GetRecord(info);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = string.Format(ResourcesService.GetString(Culture, "ScheduleModule_CurrentCommand_NoCurrentRecord"), info.DisplayText);

					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += ResourcesService.GetString(Culture, "ScheduleModuleBase_ItIsWeekend");
					}

					ReplyDeferred(response, info, record);
				} else {
					await RespondRecord(string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_PretextNow"), info.DisplayText), info, record);
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen")]
		public async Task StudentNextCommand([Remainder] IdentifierInfo info) {
			ReturnValue<ScheduleRecord> result = await GetNextRecord(info);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				string pretext;
				if (record.Start.Date == DateTime.Today) {
					pretext = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_PretextNext"), info.DisplayText);
				} else {
					pretext = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_Pretext_FirstOn"), info.DisplayText, ScheduleUtil.GetStringFromDayOfWeek(Culture, record.Start.DayOfWeek));
				}
				await RespondRecord(pretext, info, record);
			}
		}

		[Command("dag", RunMode = RunMode.Async)]
		public async Task StudentWeekdayCommand(DayOfWeek day, [Remainder] IdentifierInfo info) {
			await RespondDay(info, ScheduleUtil.NextDayOfWeek(day, false));
		}

		[Command("vandaag", RunMode = RunMode.Async)]
		public async Task StudentTodayCommand([Remainder] IdentifierInfo info) {
			await RespondDay(info, DateTime.Today);
		}

		[Command("morgen", RunMode = RunMode.Async)]
		public async Task StudentTomorrowCommand([Remainder] IdentifierInfo info) {
			await RespondDay(info, DateTime.Today.AddDays(1));
		}

		[Command("deze week", RunMode = RunMode.Sync)]
		public async Task ShowThisWeekWorkingDaysCommand([Remainder] IdentifierInfo info) {
			await RespondWorkingDays(info, 0);
		}

		[Command("volgende week", RunMode = RunMode.Sync)]
		public async Task ShowNextWeekWorkingDaysCommand([Remainder] IdentifierInfo info) {
			await RespondWorkingDays(info, 1);
		}

		[Command("over", RunMode = RunMode.Sync)]
		public async Task ShowFutureCommand([Range(1, 52)] int amount, string unit, [Remainder] IdentifierInfo info) {
			if (unit == "uur") { // TODO units need to be localized
				ReturnValue<ScheduleRecord> result = await GetRecordAfterTimeSpan(info, TimeSpan.FromHours(amount));
				if (result.Success) {
					ScheduleRecord record = result.Value;
					if (record != null) {
						await RespondRecord(string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_InXHours"), info.DisplayText, amount), info, record);
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

		[Command("daarna", RunMode = RunMode.Async)]
		public async Task GetAfterCommand([Remainder] string ignored = "") {
			if (!string.IsNullOrWhiteSpace(ignored)) {
				ReplyDeferred(ResourcesService.GetString(Culture, "AfterScheduleModule_GetAfterCommand_ParameterHint"));
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
		private async Task RespondDay(IdentifierInfo info, DateTime date) {
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

						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondDay_ColumnTeacher"),
						ResourcesService.GetString(Culture, "ScheduleModuleBase_RespondDay_ColumnRoom")
					};

					int recordIndex = 1;
					foreach (ScheduleRecord record in records) {
						cells[recordIndex] = new string[4];
						cells[recordIndex][0] = record.Activity.DisplayText;
						cells[recordIndex][1] = $"{record.Start.ToString("HH:mm")} - {record.End.ToString("HH:mm")}";

						cells[recordIndex][2] = record.StaffMemberString;
						cells[recordIndex][3] = record.RoomString;

						recordIndex++;
					}
					response += Util.FormatTextTable(cells);
					ReplyDeferred(response, info, records.Last());
				}
			}
		}

		private async Task RespondWorkingDays(IdentifierInfo info, int weeksFromNow) {
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

		protected async Task RespondAfter(int recursion = 0) {
			ScheduleCommandInfo query = LSCService.GetLastCommandForContext(Context);
			if (query.Equals(default(ScheduleCommandInfo))) {
				await MinorError(ResourcesService.GetString(Culture, "AfterScheduleModule_GetAfterCommand_NoContext"));
			} else {
				ScheduleRecord nextRecord;
				try {
					if (query.Record == null) {
						nextRecord = await Schedules.GetNextRecord(query.Identifier, Context);
					} else {
						nextRecord = await Schedules.GetRecordAfter(query.Identifier, query.Record, Context);
					}
				} catch (RecordsOutdatedException) {
					await MinorError(ResourcesService.GetString(Culture, "AfterScheduleModule_GetAfterCommand_RecordsOutdated"));
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
					pretext = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_PretextNext"), query.Identifier.DisplayText);
				} else {
					pretext = string.Format(ResourcesService.GetString(Culture, "ScheduleModuleBase_PretextAfterPrevious"), query.Identifier.DisplayText);
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
		protected async Task<ReturnValue<ScheduleRecord>> GetRecord(IdentifierInfo identifier) {
			if (ScheduleUtil.IsSummerBreak()) {
				await MinorError(ResourcesService.GetString(Culture, "ScheduleModuleBase_SummerBreakGoHome"));
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			}

			return await HandleErrorAsync(() => Schedules.GetCurrentRecord(identifier, Context));
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetNextRecord(IdentifierInfo identifier) {
			return await HandleErrorAsync(() => Schedules.GetNextRecord(identifier, Context));
		}

		protected async Task<ReturnValue<ScheduleRecord[]>> GetSchedulesForDay(IdentifierInfo identifier, DateTime date) {
			if (ScheduleUtil.IsSummerBreak(date)) {
				await MinorError(ResourcesService.GetString(Culture, "ScheduleModuleBase_SummerBreakGoHome"));
				return new ReturnValue<ScheduleRecord[]>() {
					Success = false
				};
			}

			return await HandleErrorAsync(() => Schedules.GetSchedulesForDate(identifier, date, Context));
		}

		protected async Task<ReturnValue<AvailabilityInfo[]>> GetWeekAvailabilityInfo(IdentifierInfo identifier, int weeksFromNow) {
			return await HandleErrorAsync(() => Schedules.GetWeekAvailability(identifier, weeksFromNow, Context));
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetRecordAfterTimeSpan(IdentifierInfo identifier, TimeSpan span) {
			return await HandleErrorAsync(() => Schedules.GetRecordAfterTimeSpan(identifier, span, Context));
		}

		protected async Task RespondRecord(string pretext, IdentifierInfo info, ScheduleRecord record, bool callNextIfBreak = true) {
			string response = pretext + "\n";
			response += await record.PresentAsync(info);
			ReplyDeferred(response, info, record);

			if (callNextIfBreak && record.ShouldCallNextCommand) {
				await RespondAfter(0);
			}
		}

		private async Task<ReturnValue<T>> HandleErrorAsync<T>(Func<Task<T>> action) {
			try {
				return new ReturnValue<T>() {
					Success = true,
					Value = await action()
				};
			} catch (IdentifierNotFoundException) {
				await MinorError(ResourcesService.GetString(Culture, "ScheduleModuleBase_HandleError_NotFound"));
				return new ReturnValue<T>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError(ResourcesService.GetString(Culture, "ScheduleModuleBase_HandleError_RecordsOutdated"));
				return new ReturnValue<T>() {
					Success = false
				};
			} catch (NoAllowedGuildsException) {
				await MinorError(ResourcesService.GetString(Culture, "ScheduleModuleBase_HandleError_NoSchedulesAvailableForServer"));
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
