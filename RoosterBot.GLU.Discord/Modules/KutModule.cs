using Discord;
using Qmmands;
using RoosterBot.DiscordNet;

namespace RoosterBot.GLU.Discord {
	[HiddenFromList]
	public class KutModule : RoosterModule<DiscordCommandContext> {
		[Command("kut"), IgnoresExtraArguments]
		public CommandResult Kut() {
			if (!ChannelConfig.TryGetData("glu.jokes.kut", out string? mention)) {
				mention = Context.User.Mention;
			}
			if (MentionUtils.ParseUser(mention) == Context.User.Id) {
				mention = MentionUtils.MentionUser(133798410024255488); // Foxite
			}
			return new TextResult(null, "Kut " + mention);
		}
	}
}
