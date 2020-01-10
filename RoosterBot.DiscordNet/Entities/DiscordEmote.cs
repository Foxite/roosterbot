namespace RoosterBot.DiscordNet {
	public class DiscordEmote : IEmote {
		public Discord.Emote Emote { get; }

		public DiscordEmote(string emoteString) {
			Emote = Discord.Emote.Parse(emoteString);
		}

		public override string ToString() => Emote.ToString();
	}
}
