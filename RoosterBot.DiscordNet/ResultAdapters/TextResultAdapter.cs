using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DiscordNet {
	// TODO move to universal adapter in RoosterBot
	public class TextResultAdapter : DiscordResultAdapter<TextResult> {
		protected async override Task<IUserMessage> HandleResult(DiscordCommandContext context, TextResult result) {
			EmoteService emotes = context.ServiceProvider.GetRequiredService<EmoteService>();

			string text = result.Response;
			if (result.PrefixEmoteName != null) {
				if (emotes.TryGetEmote(context.Platform, result.PrefixEmoteName, out IEmote? emote)) {
					text = emote.ToString() + " " + text;
				} else {
					Logger.Error(DiscordNetComponent.LogTag, "PlatformComponent " + context.Platform.Name + " does not define an emote named " + result.PrefixEmoteName + ". No emote will be used.");
				}
			}
			return await SendMessage(context, text);
		}
	}
}
