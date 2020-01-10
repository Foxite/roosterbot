using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RoosterBot {
	public sealed class EmoteService {
		private readonly Dictionary<(PlatformComponent Platform, string Name), IEmote> m_Emotes;

		internal EmoteService() {
			m_Emotes = new Dictionary<(PlatformComponent Platform, string Name), IEmote>();
		}

		public bool RegisterEmote(PlatformComponent platform, string name, IEmote emote) => m_Emotes.TryAdd((platform, name), emote);
		public bool TryGetEmote(PlatformComponent platform, string name, [MaybeNullWhen(false), NotNullWhen(true)] out IEmote? emote) => m_Emotes.TryGetValue((platform, name), out emote);

		public IEmote Success(PlatformComponent platform) => GetStandardEmote(platform, "Success");
		public IEmote Info   (PlatformComponent platform) => GetStandardEmote(platform, "Info"   );
		public IEmote Error  (PlatformComponent platform) => GetStandardEmote(platform, "Error"  );
		public IEmote Warning(PlatformComponent platform) => GetStandardEmote(platform, "Warning");
		public IEmote Unknown(PlatformComponent platform) => GetStandardEmote(platform, "Unknown");

		private IEmote GetStandardEmote(PlatformComponent platform, string name) {
			IEmote ret;
			if (TryGetEmote(platform, "Error", out IEmote? emote)) {
				ret = emote;
			} else {
				Logger.Error("EmoteService", "PlatformComponent " + platform.Name + " does not define an emote named " + name + ". This is required of all platforms.");
				ret = new MissingEmote();
			}
			return ret;
		}

		private class MissingEmote : IEmote {
			public override string ToString() => "MISSING EMOTE";
		}
	}
}
