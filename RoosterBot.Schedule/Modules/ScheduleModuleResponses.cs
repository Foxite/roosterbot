using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Schedule {
	public partial class ScheduleModule {
		private async Task<CommandResult> RespondDay(IdentifierInfo? info, DateTime date) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			if (resolve.Success) {
				info = resolve.Value;
				ReturnValue<ScheduleRecord[]> recordResult = await GetSchedulesForDay(info, date);
				if (recordResult.Success) {
					return new PaginatedResult(new DayScheduleEnumerator(Context, info, date, new string[] {
						GetString("ScheduleModule_RespondDay_ColumnActivity"),
						GetString("ScheduleModule_RespondDay_ColumnTime"),
						GetString("ScheduleModule_RespondDay_ColumnStudentSets"),
						GetString("ScheduleModule_RespondDay_ColumnTeacher"),
						GetString("ScheduleModule_RespondDay_ColumnRoom")
					}));
				} else {
					return recordResult.ErrorResult;
				}
			} else {
				return resolve.ErrorResult;
			}
		}
		
		protected async Task<CommandResult> RespondWeek(IdentifierInfo? info, int weeksFromNow) {
			ReturnValue<IdentifierInfo> resolve = ResolveNullInfo(info);
			return resolve.Success
				? new PaginatedResult(new WeekScheduleEnumerator(Context, resolve.Value, weeksFromNow))
				: resolve.ErrorResult;
		}
	}
}
