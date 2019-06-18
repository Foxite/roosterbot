using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Services {
	public class ConfigService {
		public	 IUser        BotOwner { get; private set; }
		public	 bool         ErrorReactions { get; private set; }
		public	 string       CommandPrefix { get; private set; }
		public	 string       GameString { get; private set; }
		public   ITextChannel LogChannel { get; private set; }
		internal string       SNSCriticalFailureARN { get; private set; }

		[Obsolete("Use ConfigService.BotOwner")]
		public	 ulong        BotOwnerId => BotOwner.Id;

		internal ConfigService(string jsonPath, out string authToken) {
			string jsonFile = File.ReadAllText(jsonPath);
			JObject jsonConfig = JObject.Parse(jsonFile);

			authToken = jsonConfig["token"].ToObject<string>();
			ErrorReactions = jsonConfig["errorReactions"].ToObject<bool>();
			CommandPrefix = jsonConfig["commandPrefix"].ToObject<string>();
			GameString = jsonConfig["gameString"].ToObject<string>();
			SNSCriticalFailureARN = jsonConfig["snsCF_ARN"].ToObject<string>();
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
