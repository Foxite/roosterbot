using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using RoosterBot.DiscordNet;

namespace RoosterBot.GLU.Discord {
	internal sealed class NicknameChangedHandler {
		private readonly UserConfigService m_UCS;

		public NicknameChangedHandler(UserConfigService userConfigService) {
			m_UCS = userConfigService;

			DiscordNetComponent.Instance.Client.GuildMemberUpdated += OnUserUpdated;
		}

		private async Task OnUserUpdated(SocketGuildUser previous, SocketGuildUser current) {
			UserConfig userConfig = await m_UCS.GetConfigAsync(new SnowflakeReference(DiscordNetComponent.Instance, current.Id));
			if (current is IGuildUser igu && igu.Nickname != null) {
				userConfig.SetData("glu.discord.nickname", igu.Nickname);
				await userConfig.UpdateAsync();
			}
		}
	}
}