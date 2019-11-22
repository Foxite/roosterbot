using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot {
	internal sealed class ReadyHandler {
		private readonly ConfigService m_ConfigService;
		private readonly DiscordSocketClient m_Client;

		private bool m_VersionNotReported;

		public ReadyHandler(ConfigService configService, DiscordSocketClient client) {
			m_ConfigService = configService;
			m_Client = client;

			client.Ready += OnClientReady;
		}

		private async Task OnClientReady() {
			await m_ConfigService.LoadDiscordInfo(m_Client);
			await m_Client.SetGameAsync(m_ConfigService.GameString, type: m_ConfigService.ActivityType);
			Logger.Info("Main", $"Username is {m_Client.CurrentUser.Username}#{m_Client.CurrentUser.Discriminator}");

			if (m_VersionNotReported && m_ConfigService.ReportStartupVersionToOwner) {
				m_VersionNotReported = false;
				IDMChannel ownerDM = await m_ConfigService.BotOwner.GetOrCreateDMChannelAsync();
				string startReport = $"RoosterBot version: {Constants.VersionString}\n";
				startReport += "Components:\n";
				foreach (ComponentBase component in Program.Instance.Components.GetComponents()) {
					startReport += $"- {component.Name}: {component.ComponentVersion.ToString()}\n";
				}

				await ownerDM.SendMessageAsync(startReport);
			}

			// Find an open Ready pipe and report
			NamedPipeClientStream? pipeClient = null;
			try {
				pipeClient = new NamedPipeClientStream(".", "roosterbotReady", PipeDirection.Out);
				await pipeClient.ConnectAsync(1);
				using StreamWriter sw = new StreamWriter(pipeClient);
				pipeClient = null;
				sw.WriteLine("ready");
			} catch (TimeoutException) {
				// Pass
			} finally {
				if (pipeClient != null) {
					pipeClient.Dispose();
				}
			}
		}
	}
}
