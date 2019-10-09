using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;

namespace RoosterBot {
	public sealed class ConfigService {
		public   string DefaultCommandPrefix { get; } // TODO command prefixes should be per server
		public   string GameString { get; }
		public   ActivityType ActivityType { get; }
		public   MultiMatchHandling MultiMatchHandling { get; }
		public   IUser BotOwner { get; private set; }
		public   IReadOnlyCollection<ulong> StaffRoles { get; }
		internal bool ReportStartupVersionToOwner { get; }
		internal int MinimumMemorySeconds { get; }
		public CultureInfo DefaultCulture { get; internal set; }

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
			
			authToken = jsonConfig["token"].ToObject<string>();
			DefaultCommandPrefix = jsonConfig["commandPrefix"].ToObject<string>();
			GameString = jsonConfig["gameString"].ToObject<string>();
			ActivityType = jsonConfig["activityType"].ToObject<ActivityType>();
			MultiMatchHandling = jsonConfig["multiMatchHandling"].ToObject<MultiMatchHandling>();
			ReportStartupVersionToOwner = jsonConfig["reportStartupVersionToOwner"].ToObject<bool>();
			m_BotOwnerId = jsonConfig["botOwnerId"].ToObject<ulong>();
			MinimumMemorySeconds = jsonConfig["minimumMemorySeconds"].ToObject<int>();
			StaffRoles = jsonConfig["staffRoles"].ToObject<JArray>().Select(jt => jt.ToObject<ulong>()).ToList().AsReadOnly();
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
