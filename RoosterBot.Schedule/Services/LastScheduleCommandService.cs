using System.Collections.Concurrent;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	// TODO (refactor) This service is superseded by the new UserConfigService
	public class LastScheduleCommandService {
		private ConcurrentDictionary<SCIKey, ScheduleCommandInfo> m_SCIs;

		public LastScheduleCommandService() {
			m_SCIs = new ConcurrentDictionary<SCIKey, ScheduleCommandInfo>();
		}

		public ScheduleCommandInfo GetLastCommandForContext(ICommandContext context) {
			if (m_SCIs.TryGetValue(new SCIKey(context), out ScheduleCommandInfo previous)) {
				return previous;
			} else {
				return default(ScheduleCommandInfo);
			}
		}

		public void OnRequestByUser(ICommandContext context, IdentifierInfo identifier, ScheduleRecord record) {
			if (identifier != null) {
				m_SCIs[new SCIKey(context)] = new ScheduleCommandInfo(identifier, record);
			} else {
				m_SCIs.TryRemove(new SCIKey(context), out _);
			}
		}

		public bool RemoveLastQuery(ICommandContext context) {
			return m_SCIs.TryRemove(new SCIKey(context), out _);
		}

		private struct SCIKey {
			public IMessageChannel Channel { get; }
			public IUser User { get; }

			public SCIKey(ICommandContext context) {
				Channel = context.Channel;
				User = context.User;
			}

			public override bool Equals(object obj) {
				return
					obj is SCIKey key &&
					key.Channel.Id == Channel.Id &&
					key.User.Id == User.Id;
			}

			public override int GetHashCode() {
				var hashCode = -400689418;
				hashCode = hashCode * -1521134295 + Channel.Id.GetHashCode(); // It turns out that no Discord entity implements GetHashCode, but SnowflakeEntites can be uniquely identified
				hashCode = hashCode * -1521134295 + User.Id.GetHashCode();    //  by their ID. The only thing is that the ID is a long, and we must return int. But that's not a real problem.
				return hashCode;
			}
		}
	}

	public struct ScheduleCommandInfo {
		public ScheduleRecord Record;
		public IdentifierInfo Identifier;

		public ScheduleCommandInfo(IdentifierInfo identifier, ScheduleRecord record) {
			Identifier = identifier;
			Record = record;
		}
	}
}
