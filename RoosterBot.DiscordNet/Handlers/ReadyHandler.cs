using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	internal sealed class ReadyHandler : RoosterHandler {
		public DiscordSocketClient Client { get; set; } = null!;

		private bool m_VersionNotReported = true;
		private readonly string m_GameString;
		private readonly ActivityType m_ActivityType;
		private readonly bool m_ReportVersion;
		private readonly ulong m_BotOwnerId;

		public ReadyHandler(IServiceProvider isp, string gameString, ActivityType activityType, bool reportVersion, ulong botOwnerId) : base(isp) {
			Client.Ready += OnClientReady;

			m_GameString = gameString;
			m_ActivityType = activityType;
			m_ReportVersion = reportVersion;
			m_BotOwnerId = botOwnerId;
		}

		private async Task OnClientReady() {
			await Client.SetGameAsync(m_GameString, type: m_ActivityType);
			Logger.Info("Main", $"Username is {Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}");

			if (m_VersionNotReported && m_ReportVersion) {
				m_VersionNotReported = false;
				IDMChannel ownerDM = await Client.GetUser(m_BotOwnerId).GetOrCreateDMChannelAsync();
				string startReport = $"RoosterBot version: {Constants.VersionString}\n";
				startReport += "Components:\n";
				foreach (Component component in Program.Instance.Components.GetComponents()) {
					startReport += $"- {component.Name}: {component.ComponentVersion.ToString()}\n";
				}

				await ownerDM.SendMessageAsync(startReport);
			}
		}
	}
}
