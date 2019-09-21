using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Meta {
	[HiddenFromList]
	public class ControlModule : RoosterModuleBase {
		[Command("shutdown"), RequireBotManager]
		public Task ShutdownCommand() {
			Log.Info($"Shutdown command used by {Context.User.Username}");
			Program.Instance.Shutdown();
			return Task.CompletedTask;
		}

		[Command("restart"), RequireBotManager]
		public Task RestartCommand() {
			Log.Info($"Restart command used by {Context.User.Username}");
			Program.Instance.Restart();
			return Task.CompletedTask;
		}
	}
}
