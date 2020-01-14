using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	public class DiscordCommandContext : RoosterCommandContext {
		public BaseSocketClient Client { get; }
		public new IUserMessage Message { get; }
		public new Discord.IUser User { get; }
		public new IMessageChannel Channel { get; }
		public IGuild? Guild { get; }

		public DiscordCommandContext(DiscordMessage message, UserConfig userConfig, ChannelConfig guildConfig) : base(DiscordNetComponent.Instance, message, userConfig, guildConfig) {
			Client = DiscordNetComponent.Instance.Client;
			Message = message.DiscordEntity;
			User = Message.Author;
			Channel = Message.Channel;
			Guild = Channel is SocketGuildChannel sgc ? sgc.Guild : null;
		}

		// I'd make this a local, but then I couldn't have the null-awareness attributes on the parameters.
		private bool IsResult<T>(RoosterCommandResult input, [MaybeNullWhen(false), NotNullWhen(true)] out T? result) where T : RoosterCommandResult {
			result =                                    // Hard-to-read expression - I've laid it out here:
				input as T ??                           // Simple, if result is T then return result as T.
				((input is CompoundResult cr            // If it's not T: Is it a compound result...
				&& cr.IndividualResults.CountEquals(1)) //  with only one item?
				? cr.IndividualResults.First() as T     //   Then return the first (and only) item as T, returning null if it's not T.
				: null);                                // otherwise return null.
			return result != null;
		}

		protected override Task<IMessage> SendResultAsync(RoosterCommandResult result) {
			if (IsResult<AspectListResult>(result, out var alr)) {
				return SendAspectList(alr);
			} else if (IsResult<PaginatedResult>(result, out var pr)) {
				return SendPaginatedResult(pr);
			} else {
				return base.SendResultAsync(result);
			}
		}

		private async Task<IMessage> SendAspectList(AspectListResult alr) {
			Embed embed = AspectListToEmbedBuilder(alr).Build();
			if (alr.UploadFilePath == null) {
				return new DiscordMessage(await Channel.SendMessageAsync(embed: embed));
			} else {
				return new DiscordMessage(await Channel.SendFileAsync(alr.UploadFilePath, embed: embed));
			}
		}

		private EmbedBuilder AspectListToEmbedBuilder(AspectListResult alr) {
			string title = alr.Caption;
			string? description = null;
			int colonIndex = title.IndexOf(':');
			if (colonIndex != -1) {
				title = title.Substring(0, colonIndex);
				description = title.Substring(colonIndex + 1).Trim();
			}

			return new EmbedBuilder() {
				Title = alr.Caption,
				Description = description,
				Fields = (
					from aspect in alr
					select new EmbedFieldBuilder() {
						Name = aspect.PrefixEmote.ToString() + aspect.Name,
						Value = aspect.Value,
						IsInline = true
					}
				).ToList(),
				Author = new EmbedAuthorBuilder() {
					IconUrl = User.GetAvatarUrl(),
					Name = (User as IGuildUser)?.Nickname ?? (User.Username + "#" + User.Discriminator)
				},
				Timestamp = DateTimeOffset.UtcNow
			};
		}

		private async Task<IMessage> SendPaginatedResult(PaginatedResult pr) {
			// TODO some of this could be done in RoosterBot, only problem is that it can't add the buttons. Platform would have to take care of that.
			if (!pr.MoveNext()) {
				return new DiscordMessage(await Channel.SendMessageAsync("Empty result!")); // TODO
			}

			IUserMessage message = await Channel.SendMessageAsync(pr.Current.ToString(this));

			Task goTo(Func<bool> moveAction) {
				RoosterCommandResult current = pr.Current;
				if (moveAction()) {
					return message.ModifyAsync(props => {
						if (pr.Current is AspectListResult alr) {
							props.Content = null;
							props.Embed = AspectListToEmbedBuilder(alr).Build();
						} else {
							props.Content = current.ToString(this);
							props.Embed = null;
						}
					});
				} else {
					return message.ModifyAsync(props => {
						if (current is AspectListResult alr) {
							props.Embed = props.Embed.Value.ToEmbedBuilder()
								.WithFooter(props.Embed.Value.Footer + "\nNo more results")
								.Build();
						} else {
							props.Content += "\nNo more results.";
						}
					});
				}
				return Task.CompletedTask;
			}

			new InteractiveMessageHandler(message, User, new Dictionary<Discord.IEmote, Func<Task>>() {
				{ new Discord.Emoji("⬅️"), () => goTo(pr.MoveNext) },
				{ new Discord.Emoji("➡️"), () => goTo(pr.MovePrevious) },
				//{ new Discord.Emoji("⏪"), reset }
			});
			return new DiscordMessage(message);
		}
	}
}
