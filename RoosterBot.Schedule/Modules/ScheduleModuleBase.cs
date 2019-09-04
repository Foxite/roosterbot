using System;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot.Schedule {
	public class ScheduleModuleBase : EditableCmdModuleBase {
		public LastScheduleCommandService LSCService { get; set; }
		public ScheduleService Schedules { get; set; }
		public ActivityNameService Activities { get; set; }
		
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

		protected async Task GetAfterCommand() {
			await SendDeferredResponseAsync();
			await Program.Instance.CommandHandler.ExecuteSpecificCommand(Context.OriginalResponse, "daarna", Context.Message, "SMB daarna");
		}
		
		protected async Task<ReturnValue<ScheduleRecord>> GetRecord(IdentifierInfo identifier) {
			if (ScheduleUtil.IsSummerBreak()) {
				await MinorError(Resources.ScheduleModuleBase_SummerBreakGoHome);
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
				await MinorError(Resources.ScheduleModuleBase_SummerBreakGoHome);
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
			response += await record.PresentAsync();
			ReplyDeferred(response, info, record);

			if (callNextIfBreak && record.Activity == "pauze") {
				await GetAfterCommand();
			}
		}

		private async Task<ReturnValue<T>> HandleError<T>(Func<T> action) {
			try {
				return new ReturnValue<T>() {
					Success = true,
					Value = action()
				};
			} catch (IdentifierNotFoundException) {
				await MinorError(Resources.ScheduleModuleBase_HandleError_NotFound);
				return new ReturnValue<T>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError(Resources.ScheduleModuleBase_HandleError_RecordsOutdated);
				return new ReturnValue<T>() {
					Success = false
				};
			} catch (NoAllowedGuildsException) {
				await MinorError(Resources.ScheduleModuleBase_HandleError_NoSchedulesAvailableForServer);
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

		private async Task<ReturnValue<T>> HandleErrorAsync<T>(Func<Task<T>> action) {
			try {
				return new ReturnValue<T>() {
					Success = true,
					Value = await action()
				};
			} catch (IdentifierNotFoundException) {
				await MinorError(Resources.ScheduleModuleBase_HandleError_NotFound);
				return new ReturnValue<T>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError(Resources.ScheduleModuleBase_HandleError_RecordsOutdated);
				return new ReturnValue<T>() {
					Success = false
				};
			} catch (NoAllowedGuildsException) {
				await MinorError(Resources.ScheduleModuleBase_HandleError_NoSchedulesAvailableForServer);
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
	}
}
