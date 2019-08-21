using Discord;
using ScheduleComponent.DataTypes;
using System.Collections.Generic;

namespace ScheduleComponent.Services {
	public class ActivityNameService {
		private List<GuildActivityLookup> m_Lookups;

		public delegate string LookupActivity(string abbreviation);

		public ActivityNameService() {
			m_Lookups = new List<GuildActivityLookup>();
		}

		public void RegisterLookup(ulong[] guilds, LookupActivity lookup) {
			m_Lookups.Add(new GuildActivityLookup(guilds, lookup));
		}

		public string GetActivityFromAbbreviation(IGuild guild, string abbreviation) {
			foreach (GuildActivityLookup gal in m_Lookups) {
				if (gal.IsGuildAllowed(guild)) {
					return gal.LookupFunction(abbreviation);
				}
			}
			throw new NoAllowedGuildsException($"No abbreviation info visible to the given guild ({guild.Name}) was found.");
		}

		private class GuildActivityLookup : GuildSpecificInfo {
			public LookupActivity LookupFunction { get; }

			public GuildActivityLookup(ulong[] allowedGuilds, LookupActivity lookupFunction) : base(allowedGuilds) {
				LookupFunction = lookupFunction;
			}
		}
	}
}
