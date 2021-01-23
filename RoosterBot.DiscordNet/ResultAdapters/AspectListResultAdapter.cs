using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot.DiscordNet {
	public class AspectListResultAdapter : DiscordResultAdapter<AspectListResult> {
		protected override Task<IUserMessage> HandleResult(DiscordCommandContext context, AspectListResult result) {
			return SendMessage(context, embed: AspectListToEmbedBuilder(context, result));
		}

		private EmbedBuilder AspectListToEmbedBuilder(DiscordCommandContext context, AspectListResult alr) {
			string title = alr.Caption;
			string? description = null;
			int colonIndex = title.IndexOf(':');
			if (colonIndex != -1) {
				title = title.Substring(0, colonIndex);
				description = title[(colonIndex + 1)..].Trim();
			}

			return new EmbedBuilder() {
				Title = title,
				Description = description,
				Fields = alr.Select(aspect => new EmbedFieldBuilder() {
					Name = aspect.PrefixEmote.ToString() + " " + aspect.Name,
					Value = aspect.Value,
					IsInline = aspect.Value.Length < 80
				}).ToList(),
				Author = new EmbedAuthorBuilder() {
					IconUrl = context.User.GetAvatarUrl(),
					Name = context.User is IGuildUser igu ? igu.Nickname : (context.User.Username + "#" + context.User.Discriminator)
				},
				Timestamp = DateTimeOffset.UtcNow
			};
		}
	}
}
