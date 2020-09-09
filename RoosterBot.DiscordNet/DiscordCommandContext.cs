using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DiscordNet {
	public class DiscordCommandContext : RoosterCommandContext {
		public BaseSocketClient Client { get; }
		public new IUserMessage Message { get; }
		public new Discord.IUser User { get; }
		public new IMessageChannel Channel { get; }
		public IGuild? Guild { get; }

		public DiscordCommandContext(IServiceProvider isp, DiscordMessage message, UserConfig userConfig, ChannelConfig guildConfig)
			: base(isp, DiscordNetComponent.Instance, message, userConfig, guildConfig) {
			Client = DiscordNetComponent.Instance.Client;
			Message = message.DiscordEntity;
			User = Message.Author;
			Channel = Message.Channel;
			Guild = Channel is SocketGuildChannel sgc ? sgc.Guild : null;
		}

		protected override Task<IMessage> SendResultAsync(RoosterCommandResult result, IMessage? existingResponse) {
			if (result.Is<AspectListResult>(out var alr)) {
				return SendAspectList(alr, existingResponse);
			} else if (result.Is<PaginatedResult>(out var pr)) {
				return SendPaginatedResult(pr, existingResponse);
			} else {
				return base.SendResultAsync(result, existingResponse);
			}
		}

		private async Task<IMessage> SendAspectList(AspectListResult alr, IMessage? existingResponse) {
			Embed embed = AspectListToEmbedBuilder(alr).Build();
			if (existingResponse == null) {
				if (alr.UploadFilePath == null) {
					return new DiscordMessage(await Channel.SendMessageAsync(embed: embed));
				} else {
					return new DiscordMessage(await Channel.SendFileAsync(alr.UploadFilePath, embed: embed));
				}
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
						Name = aspect.PrefixEmote.ToString() + " " + aspect.Name,
						Value = aspect.Value,
						IsInline = aspect.Value.Length < 80
					}
				).ToList(),
				Author = new EmbedAuthorBuilder() {
					IconUrl = User.GetAvatarUrl(),
					Name = (User as IGuildUser)?.Nickname ?? (User.Username + "#" + User.Discriminator)
				},
				Timestamp = DateTimeOffset.UtcNow
			};
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
				return new DiscordMessage(await Channel.SendMessageAsync(tr.ToString(this)));
			} else {
				IUserMessage botMessage;
				RoosterCommandResult initial = pr.Current;
				if (initial is AspectListResult alr) {
					if (existingResponse == null) {
						botMessage = await Channel.SendMessageAsync(pr.Caption, embed: AspectListToEmbedBuilder(alr).Build());
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
						botMessage = await Channel.SendMessageAsync(text);
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
										.WithFooter(props.Embed.Value.Footer + "\nNo more results")
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
		}
	}
}
