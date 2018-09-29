using System;
using System.Collections.Concurrent;

namespace RoosterBot {
	public class ConfigService {
		private ConcurrentDictionary<ulong, Cooldown> m_CooldownList;
		private float m_Cooldown;

		public ConfigService(float cooldown) {
			m_CooldownList = new ConcurrentDictionary<ulong, Cooldown>();
			m_Cooldown = cooldown;
		}

		/// <summary>
		/// Call this before executing a command to see if the user is still in their cooldown period.
		/// </summary>
		/// <returns>1: true if the user is not in cooldown (in this case, the cooldown will be reset). 2: if the user was already warned for their cooldown excession.</returns>
		public Tuple<bool, bool> CheckCooldown(ulong userId) {
			DateTime now = DateTime.UtcNow;
			if (m_CooldownList.ContainsKey(userId)) {
				if (now.AddSeconds(-m_Cooldown).Ticks > m_CooldownList[userId].TimeLastCommand) { // Note: DateTime.AddSeconds() returns a *new* DateTime object so this does not actually change {now}.
					m_CooldownList[userId].TimeLastCommand = now.Ticks;
					m_CooldownList[userId].Warned = false;
					return new Tuple<bool, bool>(true, false);
				} else {
					Tuple<bool, bool> ret = new Tuple<bool, bool>(false, m_CooldownList[userId].Warned);
					m_CooldownList[userId].Warned = true;
					return ret;
				}
			} else {
				m_CooldownList.TryAdd(userId, new Cooldown(userId, now.Ticks));
				return new Tuple<bool, bool>(true, false);
			}
		}

		private class Cooldown {
			public ulong UserId { get; set; }
			public long TimeLastCommand { get; set; }
			public bool Warned { get; set; }

			public Cooldown(ulong userId, long timeLastCommand, bool warned = false) {
				UserId = userId;
				TimeLastCommand = timeLastCommand;
				Warned = warned;
			}
		}
	}
}
