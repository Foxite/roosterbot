using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Services {
	public class ConfigService {
		public	 ulong        BotOwnerId { get; private set; }
		public	 bool         ErrorReactions { get; private set; }
		public	 string       CommandPrefix { get; private set; }
		public	 string       GameString { get; private set; }
		public   ITextChannel LogChannel { get; private set; }
		internal string       SNSCriticalFailureARN { get; private set; }

		internal ConfigService(string jsonPath, out string authToken) {  
			LoadConfigInternal(jsonPath, out authToken);
		}

		// The auth token will not be returned, because to take effect after changing it you would need to restart the bot.
		internal void ReloadConfig(string jsonPath) {
			string unused;
			LoadConfigInternal(jsonPath, out unused);
		}

		private void LoadConfigInternal(string jsonPath, out string authToken) {
			string jsonFile = File.ReadAllText(jsonPath);
			JObject jsonConfig = JObject.Parse(jsonFile);
			
			BotOwnerId = jsonConfig["botOwnerId"].ToObject<ulong>();
			authToken = jsonConfig["token"].ToObject<string>();
			ErrorReactions = jsonConfig["errorReactions"].ToObject<bool>();
			CommandPrefix = jsonConfig["commandPrefix"].ToObject<string>();
			GameString = jsonConfig["gameString"].ToObject<string>();
			SNSCriticalFailureARN = jsonConfig["snsCF_ARN"].ToObject<string>();
		}

		internal async Task SetLogChannelAsync(IDiscordClient client, string jsonPath) {
			string jsonFile = File.ReadAllText(Path.Combine(jsonPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);
			LogChannel = await client.GetChannelAsync(jsonConfig["logChannelId"].ToObject<ulong>(), CacheMode.AllowDownload) as ITextChannel;
			if (LogChannel == null) {
				Logger.Log(LogSeverity.Info, "Config", "LogChannel could not be found.");
			}
		}
	}
}
