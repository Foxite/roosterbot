using Discord;
using System.Collections.Generic;

namespace RoosterBot.MiscStuff {
	public sealed class PrankService {
		private Dictionary<ulong, bool> m_AlwaysJoram;

		public PrankService() {
			m_AlwaysJoram = new Dictionary<ulong, bool>();
		}

		public void SetAlwaysJoram(ulong user, bool value) {
			m_AlwaysJoram[user] = value;
		}

		public bool GetAlwaysJoram(ulong user) {
			if (m_AlwaysJoram.TryGetValue(user, out bool value)) {
				return value;
			} else {
				return false;
			}
		}
	}
}
