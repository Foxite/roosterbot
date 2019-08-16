﻿using System.Collections.Concurrent;
using Discord;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Services {
	public class LastScheduleCommandService {
		// TODO: This should be specific to guilds
		// ulong: ID of IGuildUser who requested the ScheduleRecord given in the ScheduleQueryContext.
		// It is used by the !daarna command that looks up the schedule that takes place after the last one the user received.
		private ConcurrentDictionary<ulong, ScheduleCommandInfo> m_SCIs;

		public LastScheduleCommandService() {
			m_SCIs = new ConcurrentDictionary<ulong, ScheduleCommandInfo>();
		}

		public ScheduleCommandInfo GetLastCommandFromUser(IUser user) {
			if (m_SCIs.TryGetValue(user.Id, out ScheduleCommandInfo previous)) {
				return previous;
			} else {
				return default(ScheduleCommandInfo);
			}
		}

		public void OnRequestByUser(IUser user, IdentifierInfo identifier, ScheduleRecord record) {
			if (identifier != null) {
				ScheduleCommandInfo ctx = new ScheduleCommandInfo(identifier, record);
				m_SCIs.AddOrUpdate(user.Id, ctx, (key, existing) => { return ctx; });
			} else {
				m_SCIs.TryRemove(user.Id, out ScheduleCommandInfo unused);
			}
		}

		public bool RemoveLastQuery(IUser user) {
			return m_SCIs.TryRemove(user.Id, out ScheduleCommandInfo unused);
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
