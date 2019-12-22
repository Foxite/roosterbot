using System;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public abstract class ScheduleProvider : ChannelSpecificInfo {
		protected ScheduleProvider(object[] allowedGuilds) : base(allowedGuilds) { }

		public abstract Task<ScheduleRecord?> GetRecordAtDateTimeAsync(IdentifierInfo identifier, DateTime datetime);
		public abstract Task<ScheduleRecord> GetRecordAfterDateTimeAsync(IdentifierInfo identifier, DateTime datetime);
		public abstract Task<ScheduleRecord[]> GetSchedulesForDateAsync(IdentifierInfo identifier, DateTime date);
		public abstract Task<AvailabilityInfo[]> GetWeekAvailabilityAsync(IdentifierInfo identifier, int weeksFromNow);
		public abstract Task<ScheduleRecord[]> GetWeekRecordsAsync(IdentifierInfo identifier, int weeksFromNow = 0);
	}
}
