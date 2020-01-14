using System;
using Qmmands;

namespace RoosterBot.Schedule {
	public partial class ScheduleModule {
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
		
		private CommandResult RespondWeek(IdentifierInfo? info, int weeksFromNow) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			return resolve.Success
				? new PaginatedResult(new WeekScheduleEnumerator(Context, resolve.Value, weeksFromNow))
				: resolve.ErrorResult;
		}
	}
}
