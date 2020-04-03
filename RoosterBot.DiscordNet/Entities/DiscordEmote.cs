namespace RoosterBot.DiscordNet {
	public class DiscordEmote : IEmote {
		public Discord.IEmote Emote { get; }

		public DiscordEmote(string emoteString) {
			Emote = Discord.Emote.TryParse(emoteString, out Discord.Emote result) ? ((Discord.IEmote) result) : new Discord.Emoji(emoteString);
		}

		public override string ToString() => Emote.ToString()!;
	}
}
