using System;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public partial class ScheduleModule {
		private IdentifierInfo? ResolveNullInfo(IdentifierInfo? info) {
			if (info == null) {
				StudentSetInfo? ssi = Context.UserConfig.GetStudentSet();
				if (ssi != null) {
					return ssi;
				} else {
					MinorError(GetString("StudentSetInfoReader_CheckFailed_MentionSelf", GuildConfig.CommandPrefix));
					return null;
				}
			} else {
				return info;
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

		private void MinorError(string message) {
			m_Result.AddResult(TextResult.Error(message));
		}

		protected override ValueTask AfterExecutedAsync() {
			if (m_LookedUpData != null) {
				UserConfig.OnScheduleRequestByUser(Context.Channel, m_LookedUpData);
			} else {
				UserConfig.RemoveLastScheduleCommand(Context.Channel);
			}
			return default;
		}
	}
}
