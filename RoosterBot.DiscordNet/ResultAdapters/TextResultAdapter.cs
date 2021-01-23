using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DiscordNet {
	public class TextResultAdapter : DiscordResultAdapter<TextResult> {
		protected async override Task<IUserMessage> HandleResult(DiscordCommandContext context, TextResult result, IUserMessage? existingResponse) {
			EmoteService emotes = context.ServiceProvider.GetRequiredService<EmoteService>();

			string text = result.Response;
			if (result.PrefixEmoteName != null) {
				if (emotes.TryGetEmote(context.Platform, result.PrefixEmoteName, out IEmote? emote)) {
					text = emote.ToString() + " " + text;
				} else {
					// TODO change message
					Logger.Error("TextResult", "PlatformComponent " + context.Platform.Name + " does not define an emote named " + result.PrefixEmoteName + ". No emote will be used.");
				}
			}
			return await SendMessage(context, existingResponse, text);
		}
	}

	public class AspectListResultAdapter : DiscordResultAdapter<AspectListResult> {
		protected override Task<IUserMessage> HandleResult(DiscordCommandContext context, AspectListResult result, IUserMessage? existingResponse) => throw new System.NotImplementedException();

		
		private async Task<IMessage> SendAspectList(DiscordCommandContext context, AspectListResult alr, IMessage? existingResponse) {
			Embed embed = AspectListToEmbedBuilder(context, alr).Build();
			if (existingResponse == null) {
				return new DiscordMessage(await context.Channel.SendMessageAsync(embed: embed, messageReference: context.Message.GetReference()));
			} else {
				var discordResponse = (DiscordMessage) existingResponse;
				await discordResponse.DiscordEntity.ModifyAsync(props => {
					props.Content = "";
					props.Embed = embed;
				});
				// TODO (block) Can't change file attachment
				return discordResponse;
			}
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
				Fields = (
					from aspect in alr
					select new EmbedFieldBuilder() {
						Name = aspect.PrefixEmote.ToString() + " " + aspect.Name,
						Value = aspect.Value,
						IsInline = aspect.Value.Length < 80
					}
				).ToList(),
				Author = new EmbedAuthorBuilder() {
					IconUrl = context.User.GetAvatarUrl(),
					Name = (context.User as IGuildUser)?.Nickname ?? (context.User.Username + "#" + context.User.Discriminator)
				},
				Timestamp = DateTimeOffset.UtcNow
			};
		}
	}
}
