using Discord;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RoosterBot {
	// TODO ditch this service and use GuildConfigService
	public class GuildCultureService {
		private Dictionary<ulong, CultureInfo> m_GuildCultures; // Maps guild IDs to CultureInfo
		private CultureInfo m_DefaultCulture;

		internal GuildCultureService() {
			string culturesPath = Path.Combine(Program.DataPath, "Config", "Cultures.json");
			JObject jsonCultures = JObject.Parse(File.ReadAllText(culturesPath));

			m_DefaultCulture = CultureInfo.GetCultureInfo(jsonCultures["default"].ToObject<string>());

			m_GuildCultures = jsonCultures["cultures"].ToObject<JObject>().Properties().ToDictionary(
				/* Select key   */ prop => ulong.Parse(prop.Name),
				/* Select value */ prop => CultureInfo.GetCultureInfo(prop.Value.ToObject<string>())
			);
		}

		public CultureInfo GetCultureForGuild(ulong guildId) {
			if (m_GuildCultures.TryGetValue(guildId, out CultureInfo value)) {
				return value;
			} else {
				return m_DefaultCulture;
			}
		}

		public CultureInfo GetCultureForGuild(IGuild guild) => GetCultureForGuild(guild.Id);
	}
}
