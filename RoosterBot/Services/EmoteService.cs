using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RoosterBot {
	/// <summary>
	/// Allows you to get an <see cref="IEmote"/> for a specific purpose on a specific platform.
	/// </summary>
	public sealed class EmoteService {
		private readonly Dictionary<(PlatformComponent Platform, string Name), IEmote> m_Emotes;

		internal EmoteService() {
			m_Emotes = new Dictionary<(PlatformComponent Platform, string Name), IEmote>();
		}

		/// <summary>
		/// Register an emote for your platform.
		/// </summary>
		public bool RegisterEmote(PlatformComponent platform, string name, IEmote emote) => m_Emotes.TryAdd((platform, name), emote);

		/// <summary>
		/// Try to get an emote for a platform.
		/// </summary>
		public bool TryGetEmote(PlatformComponent platform, string name, [MaybeNullWhen(false), NotNullWhen(true)] out IEmote? emote) => m_Emotes.TryGetValue((platform, name), out emote);

		/// <summary>
		/// Get the standard emote indicating success.
		/// </summary>
		public IEmote Success(PlatformComponent platform) => GetEmote(platform, "Success");
		/// <summary>
		/// Get the standard emote indicating an informational response.
		/// </summary>
		public IEmote Info   (PlatformComponent platform) => GetEmote(platform, "Info"   );
		/// <summary>
		/// Get the standard emote indicating an error.
		/// </summary>
		public IEmote Error  (PlatformComponent platform) => GetEmote(platform, "Error"  );
		/// <summary>
		/// Get the standard emote indicating a warning.
		/// </summary>
		public IEmote Warning(PlatformComponent platform) => GetEmote(platform, "Warning");
		/// <summary>
		/// Get the standard emote indicating an unknown result.
		/// </summary>
		public IEmote Unknown(PlatformComponent platform) => GetEmote(platform, "Unknown");

		private IEmote GetEmote(PlatformComponent platform, string name) {
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
