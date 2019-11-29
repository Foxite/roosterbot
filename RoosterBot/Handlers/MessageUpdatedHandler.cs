using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot {
	internal sealed class MessageUpdatedHandler : RoosterHandler {
		public DiscordSocketClient Client { get; set; } = null!;
		public GuildConfigService GCS { get; set; } = null!;
		public UserConfigService UCS { get; set; } = null!;
		public CommandExecutionHandler CEH { get; set; } = null!;

		internal MessageUpdatedHandler(IServiceProvider isp, CommandExecutionHandler ceh) : base(isp) {
			CEH = ceh;
			Client.MessageUpdated += OnMessageUpdated;
		}

		private Task OnMessageUpdated(Cacheable<IMessage, ulong> messageBefore, SocketMessage messageAfter, ISocketMessageChannel channel) {
			if (messageAfter.Content != messageBefore.Value.Content && messageAfter is SocketUserMessage userMessageAfter && messageAfter.Source == MessageSource.User) {
				_ = Task.Run(async () => {
					GuildConfig guildConfig = await GCS.GetConfigAsync((channel as IGuildChannel)?.Guild ?? messageAfter.Author.MutualGuilds.First());
					UserConfig userConfig = await UCS.GetConfigAsync(messageAfter.Author);
					CommandResponsePair? crp = userConfig.GetResponse(userMessageAfter);

					if (CommandUtil.IsMessageCommand(userMessageAfter, guildConfig.CommandPrefix, out int argPos)) {
						//IResult result = await m_Commands.ExecuteAsync(userMessageAfter.Content.Substring(argPos + 1), context);
						await CEH.ExecuteCommandAsync(userMessageAfter.Content.Substring(argPos + 1), userMessageAfter, guildConfig, userConfig);
					} else if (crp != null) {
						// No longer a command
						await channel.DeleteMessageAsync(crp.ResponseId);
						userConfig.RemoveCommand(crp.CommandId);
					} // else: was not a command, is not a command
				});
			}
			return Task.CompletedTask;
		}
	}
}
