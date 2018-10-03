using System.Collections.Concurrent;
using Discord;

namespace RoosterBot {
	public class AfterRecordService {
		// ulong: ID of IGuildUser who requested the ScheduleRecord given in the ScheduleQueryContext.
		// It is used by the !daarna command that looks up the schedule
		private ConcurrentDictionary<ulong, ScheduleQueryContext> m_SQCs;
		private ScheduleService m_Schedules;

		public AfterRecordService(ScheduleService schedules) {
			m_SQCs = new ConcurrentDictionary<ulong, ScheduleQueryContext>();
			m_Schedules = schedules;
		}

		public ScheduleQueryContext GetRecordAfter(IGuildUser user) {
			if (m_SQCs.TryGetValue(user.Id, out ScheduleQueryContext previous)) {
				return new ScheduleQueryContext(m_Schedules.GetRecordAfter(previous.SourceSchedule, previous.Record), previous.SourceSchedule);
			} else {
				return default;
			}
		}

		public void OnRequestByUser(IGuildUser user, ScheduleRecord record, string schedule) {
			ScheduleQueryContext ctx = new ScheduleQueryContext(record, schedule);
			m_SQCs.AddOrUpdate(user.Id, ctx, (key, existing) => { return ctx; });
		}
	}

	public struct ScheduleQueryContext {
		public ScheduleRecord Record;
		public string SourceSchedule;

		public ScheduleQueryContext(ScheduleRecord record, string sourceSchedule) {
			Record = record;
			SourceSchedule = sourceSchedule;
		}
	}
}
