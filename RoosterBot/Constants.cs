using Discord;

namespace RoosterBot {
	public static class Constants {
		public static readonly Version RoosterBotVersion = new Version(2, 0, 0);
		public static string VersionString => RoosterBotVersion.ToString();

		public static readonly Emote Error   = Emote.Parse("<:error:636213609919283238>");
		public static readonly Emote Success = Emote.Parse("<:ok:636213617825546242>");
		public static readonly Emote Warning = Emote.Parse("<:warning:636213630114856962>");
		public static readonly Emote Unknown = Emote.Parse("<:unknown:636213624460935188>");
		public static readonly Emote Info    = Emote.Parse("<:info:644251874010202113>");
	}
}
