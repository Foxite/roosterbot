using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;

namespace RoosterBot {
	public sealed class ConfigService {
		public   string DefaultCommandPrefix { get; }
		public   string GameString { get; }
		public   ActivityType ActivityType { get; }
		public   IReadOnlyCollection<ulong> StaffRoles { get; }
		internal bool ReportStartupVersionToOwner { get; }
		public   CultureInfo DefaultCulture { get; }
#nullable disable
		public   IUser BotOwner { get; private set; }
#nullable restore

		private  ulong m_BotOwnerId;

		internal ConfigService(string jsonPath, out string authToken) {
			if (!Directory.Exists(Program.DataPath)) {
				throw new DirectoryNotFoundException("Data folder did not exist.");
			}

			if (!File.Exists(jsonPath)) {
				throw new FileNotFoundException("Config file did not exist.");
			}

			string jsonFile = File.ReadAllText(jsonPath);
			JObject jsonConfig = JObject.Parse(jsonFile);
			
			authToken                   = jsonConfig["token"]                      .ToObject<string>();
			GameString                  = jsonConfig["gameString"]                 .ToObject<string>();
			m_BotOwnerId                = jsonConfig["botOwnerId"]                 .ToObject<ulong>();
			StaffRoles                  = jsonConfig["staffRoles"]                 .ToObject<JArray>().Select(jt => jt.ToObject<ulong>()).ToList().AsReadOnly();
			ActivityType                = jsonConfig["activityType"]               .ToObject<ActivityType>();
			DefaultCommandPrefix        = jsonConfig["defaultCommandPrefix"]       .ToObject<string>();
			ReportStartupVersionToOwner = jsonConfig["reportStartupVersionToOwner"].ToObject<bool>();
			DefaultCulture              = CultureInfo.GetCultureInfo(jsonConfig["defaultCulture"].ToObject<string>());
		}
		
		/// <summary>
		/// Load Discord.NET objects based on config data.
		/// </summary>
		internal async Task LoadDiscordInfo(IDiscordClient client) {
			// Load IUser belonging to owner
			BotOwner = await client.GetUserAsync(m_BotOwnerId);
		}
	}
}
