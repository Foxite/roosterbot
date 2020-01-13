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
			                                            // Hard-to-read expression - I've laid it out here:
			result = input as T ??                      // Simple, if result is T then return result as T.
				((input is global::RoosterBot.CompoundResult cr            // If it's not T: Is it a compound result...
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
			var embed = new EmbedBuilder()
				.WithTitle(alr.Caption)
				.WithFields(
					from aspect in alr
					select new EmbedFieldBuilder()
					.WithName(aspect.PrefixEmote.ToString() + aspect.Name)
					.WithValue(aspect.Value)
					.WithIsInline(true))
				.Build();
			if (alr.UploadFilePath == null) {
				return new DiscordMessage(await Channel.SendMessageAsync(embed: embed));
			} else {
				return new DiscordMessage(await Channel.SendFileAsync(alr.UploadFilePath, embed: embed));
			}
		}

		private async Task<IMessage> SendPaginatedResult(PaginatedResult pr) {
			if (!pr.MoveNext()) {
				return new DiscordMessage(await Channel.SendMessageAsync("Empty result!")); // TODO
			}

			IUserMessage message = await Channel.SendMessageAsync(pr.Current.ToString(this));
			bool stoppedAtEnd = false;
			bool stoppedAtStart = false;

			Task goPrevious() {
				if (!stoppedAtStart) {
					if (pr.MovePrevious()) {
						return message.ModifyAsync(props => {
							props.Content = pr.Current.ToString(this);
						});
					} else {
						stoppedAtStart = true;
					}
				}
				return Task.CompletedTask;
			}

			Task goNext() {
				if (!stoppedAtEnd) {
					if (pr.MoveNext()) {
						return message.ModifyAsync(props => {
							props.Content = pr.Current.ToString(this);
						});
					} else {
						stoppedAtEnd = true;
					}
				}
				return Task.CompletedTask;
			}

			Task reset() {
				pr.Reset();
				stoppedAtEnd = false;
				stoppedAtStart = false;
				return goNext();
			}

			new InteractiveMessageHandler(message, User, new Dictionary<Discord.IEmote, Func<Task>>() {
				{ new Discord.Emoji("⬅️"), goPrevious },
				{ new Discord.Emoji("➡️"), goNext },
				{ new Discord.Emoji("⏪"), reset }
			});
			return new DiscordMessage(message);
		}
	}
}
