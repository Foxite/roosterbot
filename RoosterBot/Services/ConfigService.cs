using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Services {
	public class ConfigService {
		public   bool         ErrorReactions { get; }
		public   string       CommandPrefix { get; }
		public   ActivityType ActivityType { get; }
		public   string       GameString { get; }
		public   IUser        BotOwner { get; private set; }
		public   ITextChannel LogChannel { get; private set; }

		internal string       SNSCriticalFailureARN { get; }
		internal bool		  ReportStartupVersionToOwner { get; }
		
		[Obsolete("Use " + nameof(BotOwner))]
		public	 ulong        BotOwnerId => BotOwner.Id;
		
		internal ConfigService(string jsonPath, out string authToken) {
			string jsonFile = File.ReadAllText(jsonPath);
			JObject jsonConfig = JObject.Parse(jsonFile);
			
			authToken = jsonConfig["token"].ToObject<string>();
			ErrorReactions = jsonConfig["errorReactions"].ToObject<bool>();
			CommandPrefix = jsonConfig["commandPrefix"].ToObject<string>();
			ActivityType = jsonConfig["activityType"].ToObject<ActivityType>();
			GameString = jsonConfig["gameString"].ToObject<string>();
			SNSCriticalFailureARN = jsonConfig["snsCF_ARN"].ToObject<string>();
			ReportStartupVersionToOwner = jsonConfig["reportStartupVersionToOwner"].ToObject<bool>();
		}
		
		/// <summary>
		/// Load Discord.NET objects based on config data.
		/// </summary>
		internal async Task LoadDiscordInfo(IDiscordClient client, string jsonPath) {
			string jsonFile = File.ReadAllText(Path.Combine(jsonPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);

			// Load IUser belonging to owner
			ulong ownerId = jsonConfig["botOwnerId"].ToObject<ulong>();
			BotOwner = await client.GetUserAsync(ownerId);

			// Load LogChannel
			LogChannel = await client.GetChannelAsync(jsonConfig["logChannelId"].ToObject<ulong>()) as ITextChannel;
			if (LogChannel == null) {
				Logger.Log(LogSeverity.Info, "Config", "LogChannel could not be found.");
			}
		}
	}
}
