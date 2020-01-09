/* // TODO Discord
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot {
	internal sealed class ReadyHandler : RoosterHandler {
		public ConfigService Config { get; set; } = null!;
		public DiscordSocketClient Client { get; set; } = null!;

		private bool m_VersionNotReported;

		public ReadyHandler(IServiceProvider isp) : base(isp) {
			Client.Ready += OnClientReady;
		}

		private async Task OnClientReady() {
			await Config.LoadDiscordInfo(Client);
			await Client.SetGameAsync(Config.GameString, type: Config.ActivityType);
			Logger.Info("Main", $"Username is {Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}");

			if (m_VersionNotReported && Config.ReportStartupVersionToOwner) {
				m_VersionNotReported = false;
				IDMChannel ownerDM = await Config.BotOwner.GetOrCreateDMChannelAsync();
				string startReport = $"RoosterBot version: {Constants.VersionString}\n";
				startReport += "Components:\n";
				foreach (Component component in Program.Instance.Components.GetComponents()) {
					startReport += $"- {component.Name}: {component.ComponentVersion.ToString()}\n";
				}

				await ownerDM.SendMessageAsync(startReport);
			}

			// Find an open Ready pipe and report
			NamedPipeClientStream? pipeClient = null;
			try {
				pipeClient = new NamedPipeClientStream(".", "roosterbotReady", PipeDirection.Out);
				await pipeClient.ConnectAsync(1);
				using var sw = new StreamWriter(pipeClient);
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
*/