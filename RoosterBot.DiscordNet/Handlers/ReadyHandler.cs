using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	internal sealed class ReadyHandler {
		private DiscordSocketClient Client => (DiscordSocketClient) DiscordNetComponent.Instance.Client;

		private bool m_VersionNotReported = true;
		private readonly string m_GameString;
		private readonly ActivityType m_ActivityType;
		private readonly ulong[] m_NotifyReady;

		public ReadyHandler(string gameString, ActivityType activityType, ulong[] notifyReady) {
			Client.Ready += OnClientReady;

			m_GameString = gameString;
			m_ActivityType = activityType;
			m_NotifyReady = notifyReady;
		}

		private async Task OnClientReady() {
			await Client.SetGameAsync(m_GameString, type: m_ActivityType);
			Logger.Info("Discord", $"Username is {Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}");

			if (m_VersionNotReported && m_NotifyReady.Length != 0) {
				m_VersionNotReported = false;
				string startReport = $"RoosterBot version: {Program.Version}\n";
				startReport += "Components:\n";
				foreach (Component component in Program.Instance.Components.GetComponents()) {
					startReport += $"- {component.Name}: {component.ComponentVersion}\n";
				}

				foreach (Task<IDMChannel> ownerDMTask in m_NotifyReady.Select(notificant => DiscordNetComponent.Instance.Client.GetUser(notificant).GetOrCreateDMChannelAsync())) {
					await (await ownerDMTask).SendMessageAsync(startReport);
				}
			}
		}
	}
}
