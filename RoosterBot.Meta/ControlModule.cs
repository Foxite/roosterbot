using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.Meta {
	[LogTag("ControlModule"), HiddenFromList]
	public class ControlModule : RoosterModuleBase {
		[Command("shutdown"), RequireBotManager]
		public Task ShutdownCommand() {
			Log.Info($"Shutdown command used by {Context.User.Username}");
			Program.Instance.Shutdown();
			return Task.CompletedTask;
		}

		}
	}
}
