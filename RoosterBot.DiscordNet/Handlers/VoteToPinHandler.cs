using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DiscordNet {
	public class VoteToPinHandler {
		private const string EmoteIdKey = "discord.voteToPin.emoteId";
		private const string VoteCountKey = "discord.voteToPin.minVotes";

		public ChannelConfigService CCS { get; set; } = null!;

		public VoteToPinHandler(IServiceProvider isp) {
			CCS = isp.GetRequiredService<ChannelConfigService>();

			DiscordNetComponent.Instance.Client.ReactionAdded += HandleReaction;
		}

		private Task HandleReaction(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3) {
			Task.Run(async () => { // Run off thread
				ChannelConfig channelConfig = await CCS.GetConfigAsync(new SnowflakeReference(DiscordNetComponent.Instance, ((IGuildChannel) arg2).Guild.Id));

				if (channelConfig.TryGetData(EmoteIdKey, out string? emoteName)) {
					if (channelConfig.TryGetData(VoteCountKey, out int minVotes)) {
						if (arg3.Emote.Name == emoteName) {
							if ((await arg1.GetOrDownloadAsync()).Reactions[arg3.Emote].ReactionCount >= minVotes) {
								await (await arg1.GetOrDownloadAsync()).PinAsync();
							}
						}
					}
				}
			});
			return Task.CompletedTask;
		}
	}
}
