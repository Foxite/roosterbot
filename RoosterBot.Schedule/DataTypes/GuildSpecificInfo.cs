﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Discord;

namespace RoosterBot.Schedule {
	public abstract class GuildSpecificInfo {
		private readonly IEnumerable<ulong> m_AllowedGuilds;

		protected GuildSpecificInfo(IEnumerable<ulong> allowedGuilds) {
			m_AllowedGuilds = allowedGuilds;
		}

		public bool IsGuildAllowed(ulong guildId) => m_AllowedGuilds.Contains(guildId);
		public bool IsGuildAllowed(IGuild guild) => IsGuildAllowed(guild.Id);
	}


	[Serializable]
	public class NoAllowedGuildsException : Exception {
		public NoAllowedGuildsException() { }
		public NoAllowedGuildsException(string message) : base(message) { }
		public NoAllowedGuildsException(string message, Exception inner) : base(message, inner) { }
		protected NoAllowedGuildsException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
