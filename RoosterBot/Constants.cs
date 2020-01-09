namespace RoosterBot {
	public static class Constants {
		public static readonly Version RoosterBotVersion = new Version(2, 2, 0);
		public static string VersionString => RoosterBotVersion.ToString();

		// TODO Get standard emotes from platform
		public static readonly IEmote Error   = null!;//Emote.Parse("<:error:636213609919283238>");
		public static readonly IEmote Success = null!;//Emote.Parse("<:ok:636213617825546242>");
		public static readonly IEmote Warning = null!;//Emote.Parse("<:warning:636213630114856962>");
		public static readonly IEmote Unknown = null!;//Emote.Parse("<:unknown:636213624460935188>");
		public static readonly IEmote Info    = null!;//Emote.Parse("<:info:644251874010202113>");
	}
}
