using System.Collections.Concurrent;
using Discord;

namespace ScheduleComponent.Services {
	public class LastScheduleCommandService {
		// ulong: ID of IGuildUser who requested the ScheduleRecord given in the ScheduleQueryContext.
		// It is used by the !daarna command that looks up the schedule that takes place after the last one the user received.
		private ConcurrentDictionary<ulong, ScheduleCommandInfo> m_SCIs;

		public LastScheduleCommandService() {
			m_SCIs = new ConcurrentDictionary<ulong, ScheduleCommandInfo>();
		}

		public ScheduleCommandInfo GetLastCommandFromUser(IUser user) {
			ScheduleCommandInfo previous;
			if (m_SCIs.TryGetValue(user.Id, out previous)) {
				return previous;
			} else {
				return default(ScheduleCommandInfo);
			}
		}

		public void OnRequestByUser(IUser user, IdentifierInfo identifier, ScheduleRecord record) {
			ScheduleCommandInfo ctx = new ScheduleCommandInfo(identifier, record);
			m_SCIs.AddOrUpdate(user.Id, ctx, (key, existing) => { return ctx; });
		}

		public bool RemoveLastQuery(IUser user) {
			ScheduleCommandInfo unused;
			return m_SCIs.TryRemove(user.Id, out unused);
		}
	}

	public struct ScheduleCommandInfo {
		public ScheduleRecord Record;
		public IdentifierInfo Identifier;
		public string SourceSchedule;

		public ScheduleCommandInfo(IdentifierInfo identifier, ScheduleRecord record) {
			SourceSchedule = identifier.TypeName;
			Identifier = identifier;
			Record = record;
		}
	}
}
