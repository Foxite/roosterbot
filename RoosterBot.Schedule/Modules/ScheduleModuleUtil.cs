using System;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public partial class ScheduleModule {
		private PaginatedResult GetSingleResult(ScheduleRecord record, IdentifierInfo identifier, string caption) =>
			new PaginatedResult(new SingleScheduleEnumerator(Context, record, identifier, caption));

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
	}
}
