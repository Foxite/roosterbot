using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	internal sealed class ReadyHandler {
		private DiscordSocketClient Client => (DiscordSocketClient) DiscordNetComponent.Instance.Client;

		private bool m_VersionNotReported = true;
		private readonly string m_GameString;
		private readonly ActivityType m_ActivityType;
		private readonly bool m_ReportVersion;

		public ReadyHandler(string gameString, ActivityType activityType, bool reportVersion) {
			Client.Ready += OnClientReady;

			m_GameString = gameString;
			m_ActivityType = activityType;
			m_ReportVersion = reportVersion;
		}

		private async Task OnClientReady() {
			await Client.SetGameAsync(m_GameString, type: m_ActivityType);
			Logger.Info("Main", $"Username is {Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}");

			if (m_VersionNotReported && m_ReportVersion) {
				m_VersionNotReported = false;
				IDMChannel ownerDM = await DiscordNetComponent.Instance.BotOwner.GetOrCreateDMChannelAsync();
				string startReport = $"RoosterBot version: {Program.Version.ToString()}\n";
				startReport += "Components:\n";
				foreach (Component component in Program.Instance.Components.GetComponents()) {
					startReport += $"- {component.Name}: {component.ComponentVersion.ToString()}\n";
				}

				await ownerDM.SendMessageAsync(startReport);
			}
		}
	}
}
