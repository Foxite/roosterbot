using System.Collections.Concurrent;
using Discord;
using Discord.Commands;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Services {
	public class LastScheduleCommandService {
		private ConcurrentDictionary<IMessageChannel, ConcurrentDictionary<IUser, ScheduleCommandInfo>> m_SCIs;

		public LastScheduleCommandService() {
			m_SCIs = new ConcurrentDictionary<IMessageChannel, ConcurrentDictionary<IUser, ScheduleCommandInfo>>();
		}

		public ScheduleCommandInfo GetLastCommandForContext(ICommandContext context) {
			if (m_SCIs.TryGetValue(context.Channel, out ConcurrentDictionary<IUser, ScheduleCommandInfo> users) &&
				users.TryGetValue(context.User, out ScheduleCommandInfo previous)) {
				return previous;
			} else {
				return default(ScheduleCommandInfo);
			}
		}

		public void OnRequestByUser(ICommandContext context, IdentifierInfo identifier, ScheduleRecord record) {
			if (identifier != null) {
				ScheduleCommandInfo sci = new ScheduleCommandInfo(identifier, record);

				ConcurrentDictionary<IUser, ScheduleCommandInfo> users = m_SCIs.GetOrAdd(context.Channel, new ConcurrentDictionary<IUser, ScheduleCommandInfo>());

				users.AddOrUpdate(context.User, sci, (key, existing) => { return sci; });
			} else {
				if (m_SCIs.TryGetValue(context.Channel, out ConcurrentDictionary<IUser, ScheduleCommandInfo> users)) {
					users.TryRemove(context.User, out ScheduleCommandInfo unused);
				}
			}
		}

		public bool RemoveLastQuery(ICommandContext context) {
			if (m_SCIs.TryGetValue(context.Channel, out ConcurrentDictionary<IUser, ScheduleCommandInfo> users)) {
				return users.TryRemove(context.User, out ScheduleCommandInfo unused);
			} else {
				return false;
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
