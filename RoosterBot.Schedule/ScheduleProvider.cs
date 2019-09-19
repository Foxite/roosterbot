using System;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public abstract class ScheduleProvider : GuildSpecificInfo {
		public ScheduleProvider(ulong[] allowedGuilds) : base(allowedGuilds) { }

		public abstract Task<ScheduleRecord> GetCurrentRecordAsync(IdentifierInfo identifier);
		public abstract Task<ScheduleRecord> GetNextRecordAsync(IdentifierInfo identifier);
		public abstract Task<ScheduleRecord> GetRecordAfterOtherAsync(IdentifierInfo identifier, ScheduleRecord givenRecord);
		public abstract Task<ScheduleRecord> GetRecordAfterTimeSpanAsync(IdentifierInfo identifier, TimeSpan timespan);
		public abstract Task<ScheduleRecord[]> GetSchedulesForDateAsync(IdentifierInfo identifier, DateTime date);
		public abstract Task<AvailabilityInfo[]> GetWeekAvailabilityAsync(IdentifierInfo identifier, int weeksFromNow);
	}
}