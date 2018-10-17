using System.Collections.Concurrent;
using Discord;

namespace ScheduleComponent.Services {
	public class LastScheduleCommandService {
		// ulong: ID of IGuildUser who requested the ScheduleRecord given in the ScheduleQueryContext.
		// It is used by the !daarna command that looks up the schedule that takes place after the last one the user received.
		private ConcurrentDictionary<ulong, ScheduleCommandInfo> m_SCIs;
		private ScheduleService m_Schedules;

		public LastScheduleCommandService(ScheduleService schedules) {
			m_SCIs = new ConcurrentDictionary<ulong, ScheduleCommandInfo>();
			m_Schedules = schedules;
		}

		public ScheduleCommandInfo GetLastCommandFromUser(IUser user) {
			if (m_SCIs.TryGetValue(user.Id, out ScheduleCommandInfo previous)) {
				return previous;
			} else {
				return default;
			}
		}

		public void OnRequestByUser(IUser user, string schedule, string identifier, ScheduleRecord record) {
			ScheduleCommandInfo ctx = new ScheduleCommandInfo(schedule, identifier, record);
			m_SCIs.AddOrUpdate(user.Id, ctx, (key, existing) => { return ctx; });
		}

		public bool RemoveLastQuery(IUser user) {
			return m_SCIs.TryRemove(user.Id, out ScheduleCommandInfo unused);
		}
	}

	public struct ScheduleCommandInfo {
		public ScheduleRecord Record;
		public string SourceSchedule;
		public string Identifier;

		public ScheduleCommandInfo(string sourceSchedule, string identifier, ScheduleRecord record) {
			SourceSchedule = sourceSchedule;
			Identifier = identifier;
			Record = record;
		}
	}
}
