using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	public class PaginatedResultAdapter : DiscordResultAdapter<PaginatedResult> {
		protected async override Task<IUserMessage> HandleResult(DiscordCommandContext context, PaginatedResult pr) {
			// Some of this could be done in RoosterBot, only problem is that it can't add the buttons. Platform would have to take care of that.
			if (!pr.MoveNext()) {
				throw new InvalidOperationException("Tried sending a PaginatedResult that didn't have any pages!");
			}

			RoosterCommandResult initial = pr.Current;
			IUserMessage botMessage = context.Response ?? ((DiscordMessage) await context.Platform.GetResultAdapter(context, initial).First().HandleResult(context, initial)).DiscordEntity;

			SocketGuildUser? currentGuildUser = ((SocketGuild?) context.Guild)?.GetUser(context.Client.CurrentUser.Id);
			if (currentGuildUser != null &&
				!currentGuildUser.GuildPermissions.AddReactions &&
				!currentGuildUser.GuildPermissions.ManageMessages) {
				Logger.Warning(DiscordNetComponent.LogTag, "Insufficient permissions in guild " + currentGuildUser.Guild.Name + " for pagination. Require at least AddReactions and ManageMessages");
			} else {
				Task goTo(Func<bool> moveAction) {
					RoosterCommandResult current = pr.Current;
					if (moveAction()) {
						return context.Platform.GetResultAdapter(context, current).First().HandleResult(context, current);
					} else {
						return botMessage.ModifyAsync(props => props.Content = "Als je dit ziet mag je de developer slaan."); // TODO no more results
					}
				}

				new InteractiveMessageHandler(botMessage, context.Message, context.User, new Dictionary<Discord.IEmote, Func<Task>>() {
					{ new Discord.Emoji("◀️"), () => goTo(pr.MovePrevious) },
					{ new Discord.Emoji("▶️"), () => goTo(pr.MoveNext) },
					//{ new Discord.Emoji("⏪"), reset }
				});
			}
			return botMessage;
		}
	}
}
