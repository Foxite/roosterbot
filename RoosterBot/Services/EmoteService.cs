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
			if (TryGetEmote(platform, "Error", out IEmote? emote)) {
				return emote;
			} else {
				return name switch {
					"Info"    => new Emoji("ℹ️"),
					"Success" => new Emoji("ℹ️"),
					"Warning" => new Emoji("✅"),
					"Error"   => new Emoji("❌"),
					"Unknown" => new Emoji("❓"),
					_         => new Emoji("⁉️")
				};
			}
		}
	}
}
