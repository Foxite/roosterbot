using System;
using System.Threading.Tasks;
using Qmmands;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Schedule {
	public partial class ScheduleModule {
		// TODO pretext
		private CommandResult RespondSingle(IdentifierInfo info, ScheduleRecord record) {
			if (record == null) {
				m_ResponseCaption = GetString("ScheduleModule_CurrentCommand_NoCurrentRecord", info.DisplayText);
				if (DateTimeUtil.IsWeekend(DateTime.Today)) {
					m_ResponseCaption += GetString("ScheduleModule_ItIsWeekend");
				}
				return new TextResult(null, m_ResponseCaption);
			} else {
				return GetResult(record, info, info.DisplayText);
			}
		}

		private CommandResult RespondDay(IdentifierInfo? info, DateTime date) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			if (resolve.Success) {
				info = resolve.Value;
				return new PaginatedResult(new DayScheduleEnumerator(Context, info, date, new string[] {
					GetString("ScheduleModule_RespondDay_ColumnActivity"),
					GetString("ScheduleModule_RespondDay_ColumnTime"),
					GetString("ScheduleModule_RespondDay_ColumnStudentSets"),
					GetString("ScheduleModule_RespondDay_ColumnTeacher"),
					GetString("ScheduleModule_RespondDay_ColumnRoom")
				}));
			} else {
				return resolve.ErrorResult;
			}
		}
		
		protected CommandResult RespondWeek(IdentifierInfo? info, int weeksFromNow) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			return resolve.Success
				? new PaginatedResult(new WeekScheduleEnumerator(Context, resolve.Value, weeksFromNow))
				: resolve.ErrorResult;
		}
	}
}
