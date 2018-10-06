using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace RoosterBot {
	public class ConfigService {
		private ConcurrentDictionary<ulong, CooldownData> m_CooldownList;

		public float  Cooldown { get; private set; }
		public ulong  BotOwnerId { get; private set; }
		public bool   ErrorReactions { get; private set; }
		public string CommandPrefix { get; private set; }
		public string GameString { get; private set; }
		public string SNSCriticalFailureARN { get; private set; }

		public ConfigService(string jsonPath, out string authToken, out Dictionary<string, string> schedules) {
			m_CooldownList = new ConcurrentDictionary<ulong, CooldownData>();
			LoadConfigInternal(jsonPath, out authToken, out schedules);
		}

		// The auth token will not be returned, because to take effect after changing it you would need to restart the bot.
		public void ReloadConfig(string jsonPath, out Dictionary<string, string> schedules) {
			LoadConfigInternal(jsonPath, out string unused, out schedules);
		}

		private void LoadConfigInternal(string jsonPath, out string authToken, out Dictionary<string, string> schedules) {
			string jsonFile = File.ReadAllText(jsonPath);
			JObject jsonConfig = JObject.Parse(jsonFile);

			JObject scheduleContainer = jsonConfig["schedules"].ToObject<JObject>();
			schedules = new Dictionary<string, string>();
			foreach (KeyValuePair<string, JToken> token in scheduleContainer) {
				schedules.Add(token.Key, token.Value.ToObject<string>());
			}

			Cooldown = jsonConfig["cooldown"].ToObject<float>();
			BotOwnerId = jsonConfig["botOwnerId"].ToObject<ulong>();
			authToken = jsonConfig["token"].ToObject<string>();
			ErrorReactions = jsonConfig["errorReactions"].ToObject<bool>();
			CommandPrefix = jsonConfig["commandPrefix"].ToObject<string>();
			GameString = jsonConfig["gameString"].ToObject<string>();
			SNSCriticalFailureARN = jsonConfig["snsCF_ARN"].ToObject<string>();
		}

		/// <summary>
		/// Call this before executing a command to see if the user is still in their cooldown period.
		/// </summary>
		/// <returns>1: true if the user is not in cooldown (in this case, the cooldown will be reset). 2: if the user was already warned for their cooldown excession.</returns>
		public Tuple<bool, bool> CheckCooldown(ulong userId) {
#if DEBUG
			return new Tuple<bool, bool>(true, false);
#else
			DateTime now = DateTime.UtcNow;
			if (m_CooldownList.ContainsKey(userId)) {
				if (now.AddSeconds(-Cooldown).Ticks > m_CooldownList[userId].TimeLastCommand) { // Note: DateTime.AddSeconds() returns a *new* DateTime object so this does not actually change {now}.
					m_CooldownList[userId].TimeLastCommand = now.Ticks;
					m_CooldownList[userId].Warned = false;
					return new Tuple<bool, bool>(true, false);
				} else {
					Tuple<bool, bool> ret = new Tuple<bool, bool>(false, m_CooldownList[userId].Warned);
					m_CooldownList[userId].Warned = true;
					return ret;
				}
			} else {
				m_CooldownList.TryAdd(userId, new CooldownData(userId, now.Ticks));
				return new Tuple<bool, bool>(true, false);
			}
#endif
		}

		private class CooldownData {
			public ulong UserId { get; set; }
			public long TimeLastCommand { get; set; }
			public bool Warned { get; set; }

			public CooldownData(ulong userId, long timeLastCommand, bool warned = false) {
				UserId = userId;
				TimeLastCommand = timeLastCommand;
				Warned = warned;
			}
		}
	}
}
