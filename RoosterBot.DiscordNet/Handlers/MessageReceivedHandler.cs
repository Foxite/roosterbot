/* // TODO Discord
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot {
	internal sealed class MessageReceivedHandler : RoosterHandler {
		public GuildConfigService GCS { get; set; } = null!;
		public UserConfigService UCS { get; set; } = null!;
		public DiscordSocketClient Client { get; set; } = null!;

		private readonly CommandExecutionHandler m_CEH;

		internal MessageReceivedHandler(IServiceProvider isp, CommandExecutionHandler ceh) : base(isp) {
			m_CEH = ceh;

			Client.MessageReceived += HandleNewCommand;
		}

		private Task HandleNewCommand(SocketMessage socketMessage) {
			// Only process commands from users
			// Other cases include bots, webhooks, and system messages (such as "X started a call" or welcome messages)
			if (socketMessage is IUserMessage userMessage) {
				_ = Task.Run(async () => {
					GuildConfig guildConfig = await GCS.GetConfigAsync((socketMessage.Channel as IGuildChannel)?.Guild ?? socketMessage.Author.MutualGuilds.First());
					if (CommandUtil.IsMessageCommand(userMessage, guildConfig.CommandPrefix, out int argPos)) {
						UserConfig userConfig = await UCS.GetConfigAsync(userMessage.Author);
						await m_CEH.ExecuteCommandAsync(userMessage.Content.Substring(argPos + 1), userMessage, guildConfig, userConfig);
					}
				});
			}
			return Task.CompletedTask;
		}
	}
}
*/