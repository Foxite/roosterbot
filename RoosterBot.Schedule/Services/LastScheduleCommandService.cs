using System;
using System.Collections.Concurrent;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	public class LastScheduleCommandService {
		private ConcurrentDictionary<SCIKey, LastScheduleCommandInfo> m_SCIs;

		public LastScheduleCommandService() {
			m_SCIs = new ConcurrentDictionary<SCIKey, LastScheduleCommandInfo>();
		}

		public LastScheduleCommandInfo? GetLastCommandForContext(ICommandContext context) {
			m_SCIs.TryGetValue(new SCIKey(context), out LastScheduleCommandInfo? previous);
			return previous;
		}

		public void OnRequestByUser(ICommandContext context, IdentifierInfo? identifier, ScheduleRecord? record) {
			if (identifier is null) {
				m_SCIs.TryRemove(new SCIKey(context), out _);
			} else {
				m_SCIs[new SCIKey(context)] = new LastScheduleCommandInfo(identifier, record);
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

			public override bool Equals(object? obj) {
				return
					obj is SCIKey key &&
					key.Channel.Id == Channel.Id &&
					key.User.Id == User.Id;
			}

			public override int GetHashCode() => HashCode.Combine(Channel, User);
		}
	}

	public class LastScheduleCommandInfo {
		public IdentifierInfo Identifier { get; set; }
		public ScheduleRecord? Record { get; set; }

		internal LastScheduleCommandInfo(IdentifierInfo identifier, ScheduleRecord? record) {
			Identifier = identifier;
			Record = record;
		}
	}
}
