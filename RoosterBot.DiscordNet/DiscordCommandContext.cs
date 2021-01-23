using System;
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
		public new IUserMessage? Response { get; }
		public IGuild? Guild { get; }

		public DiscordCommandContext(IServiceProvider isp, DiscordMessage message, UserConfig userConfig, ChannelConfig guildConfig)
			: base(isp, DiscordNetComponent.Instance, message, userConfig, guildConfig) {
			Client = DiscordNetComponent.Instance.Client;
			Message = message.DiscordEntity;
			User = Message.Author;
			Channel = Message.Channel;
			Response = ((DiscordMessage?) base.Response)?.DiscordEntity;

			Guild = Channel is SocketGuildChannel sgc ? sgc.Guild : null;
		}

		protected override Task<IMessage> SendResultAsync(RoosterCommandResult result) {
			return DiscordNetComponent.Instance.GetResultAdapter(this, result).First().HandleResult(this, result);

			/*
			if (result.Is<PaginatedResult>(out var pr)) {
				return await SendPaginatedResult(pr, existingResponse);
			} else if (result.Is<MediaResult>(out var mr)) {
				using System.IO.Stream stream = mr.GetStream();
				var message = await Channel.SendFileAsync(stream, mr.Filename, mr.Message, messageReference: Message.GetReference());
				return new DiscordMessage(message);
			}
		}

		private async Task<IMessage> SendPaginatedResult(PaginatedResult pr, IMessage? existingResponse) {
			// Some of this could be done in RoosterBot, only problem is that it can't add the buttons. Platform would have to take care of that.
			if (!pr.MoveNext()) {
				throw new InvalidOperationException("Tried sending a PaginatedResult that didn't have any pages!");
			}

			// Is the first page a TextResult with the error emote?
			EmoteService emoteService = ServiceProvider.GetService<EmoteService>();
			if (pr.Current is TextResult tr
				&& tr.PrefixEmoteName != null
				&& emoteService.TryGetEmote(DiscordNetComponent.Instance, tr.PrefixEmoteName, out IEmote? emote)
				&& emote == emoteService.Error(DiscordNetComponent.Instance)
			) {
				return new DiscordMessage(await Channel.SendMessageAsync(tr.ToString(this), messageReference: Message.GetReference()));
			} else {
				IUserMessage botMessage;
				RoosterCommandResult initial = pr.Current;
				if (initial is AspectListResult alr) {
					if (existingResponse == null) {
						botMessage = await Channel.SendMessageAsync(pr.Caption, embed: AspectListToEmbedBuilder(alr).Build(), messageReference: Message.GetReference());
					} else {
						botMessage = ((DiscordMessage) existingResponse).DiscordEntity;
						await botMessage.ModifyAsync(props => {
							props.Content = pr.Caption;
							props.Embed = AspectListToEmbedBuilder(alr).Build();
						});
					}
				} else {
					string text = pr.Current.ToString(this);
					if (pr.Caption != null) {
						text = pr.Caption + "\n" + text;
					}
					if (existingResponse == null) {
						botMessage = await Channel.SendMessageAsync(text, messageReference: Message.GetReference());
					} else {
						botMessage = ((DiscordMessage) existingResponse).DiscordEntity;
						await botMessage.ModifyAsync(props => {
							props.Content = text;
							props.Embed = null;
						});
					}
				}

				SocketGuildUser? currentGuildUser = ((SocketGuild?) Guild)?.GetUser(Client.CurrentUser.Id);
				if (currentGuildUser != null &&
					!currentGuildUser.GuildPermissions.AddReactions &&
					!currentGuildUser.GuildPermissions.ManageMessages) {
					Logger.Warning("Discord", "Insufficient permissions in guild " + currentGuildUser.Guild.Name + " for pagination. Require at least AddReactions and ManageMessages");
				} else {
					Task goTo(Func<bool> moveAction) {
						RoosterCommandResult current = pr.Current;
						if (moveAction()) {
							return botMessage.ModifyAsync(props => {
								if (pr.Current is AspectListResult alr) {
									props.Content = pr.Caption;
									props.Embed = AspectListToEmbedBuilder(alr).Build();
								} else {
									string text = pr.Current.ToString(this);
									if (pr.Caption != null) {
										text = pr.Caption + "\n" + text;
									}
									props.Content = text;
									props.Embed = null;
								}
							});
						} else {
							return botMessage.ModifyAsync(props => {
								if (pr.Current is AspectListResult alr) {
									props.Embed = props.Embed.Value.ToEmbedBuilder()
										.WithFooter((props.Embed.Value.Footer.HasValue ? props.Embed.Value.Footer.Value.Text + "\n" : null) + "No more results")
										.Build();
								} else {
									props.Content += "\nNo more results.";
								}
							});
						}
					}

					new InteractiveMessageHandler(botMessage, Message, User, new Dictionary<Discord.IEmote, Func<Task>>() {
					{ new Discord.Emoji("◀️"), () => goTo(pr.MovePrevious) },
					{ new Discord.Emoji("▶️"), () => goTo(pr.MoveNext) },
					//{ new Discord.Emoji("⏪"), reset }
				});
				}
				return new DiscordMessage(botMessage);
			}
			//*/
		}
	}
}
