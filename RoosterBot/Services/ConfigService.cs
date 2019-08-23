using System.IO;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;

namespace RoosterBot {
	public class ConfigService {
		public   bool		  ErrorReactions { get; }
		public   string		  CommandPrefix { get; }
		public   ActivityType ActivityType { get; }
		public   string		  GameString { get; }
		public   IUser		  BotOwner { get; private set; }
		internal string		  SNSCriticalFailureARN { get; }
		internal bool		  ReportStartupVersionToOwner { get; }
		private  ulong		  m_BotOwnerId;

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
			ErrorReactions = jsonConfig["errorReactions"].ToObject<bool>();
			CommandPrefix = jsonConfig["commandPrefix"].ToObject<string>();
			ActivityType = jsonConfig["activityType"].ToObject<ActivityType>();
			GameString = jsonConfig["gameString"].ToObject<string>();
			SNSCriticalFailureARN = jsonConfig["snsCF_ARN"].ToObject<string>();
			ReportStartupVersionToOwner = jsonConfig["reportStartupVersionToOwner"].ToObject<bool>();
			m_BotOwnerId = jsonConfig["botOwnerId"].ToObject<ulong>();
		}
		
		/// <summary>
		/// Load Discord.NET objects based on config data.
		/// </summary>
		internal async Task LoadDiscordInfo(IDiscordClient client, string jsonPath) {
			// Load IUser belonging to owner
			BotOwner = await client.GetUserAsync(m_BotOwnerId);
		}
	}
}
