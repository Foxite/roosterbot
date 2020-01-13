using System;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public partial class ScheduleModule {
		private PaginatedResult GetResult(ScheduleRecord record, IdentifierInfo identifier, string caption) =>
			new PaginatedResult(new ScheduleResultPaginator(Context, record, identifier, m_ResponseCaption + "\n" + caption));

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
				return ReturnValue<T>.Successful(await action());
			} catch (Exception e) {
				return ReturnValue<T>.Unsuccessful(TextResult.Error(GetString(e switch {
					IdentifierNotFoundException inf => "ScheduleModule_HandleError_NotFound",
					RecordsOutdatedException re => "ScheduleModule_HandleError_RecordsOutdated",
					NoAllowedChannelsException nac => "ScheduleModule_HandleError_NoSchedulesAvailableForServer",
					_ => throw e
				})));
			}
		}
	}
}
