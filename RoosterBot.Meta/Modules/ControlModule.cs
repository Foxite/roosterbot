using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	[HiddenFromList]
	public class ControlModule : RoosterModule {
		[Command("shutdown"), RequireBotManager]
		public Task ShutdownCommand() {
			Logger.Info("ControlModule", $"Shutdown command used by {Context.User.UserName}");
			Program.Instance.Shutdown();
			return Task.CompletedTask;
		}

		[Command("restart"), RequireBotManager]
		public Task RestartCommand() {
			Logger.Info("ControlModule", $"Restart command used by {Context.User.UserName}");
			Program.Instance.Restart();
			return Task.CompletedTask;
		}

		[Command("send"), RequireBotManager]
		public async Task SendMessageCommand(IChannel channel, [Remainder] string message) {
			await channel.SendMessageAsync(message);
		}

		[Command("delete"), RequireBotManager]
		public async Task SendMessageCommand(IMessage message) {
			await message.DeleteAsync();
		}
	}
}
