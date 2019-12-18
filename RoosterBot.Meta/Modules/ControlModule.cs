using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	[HiddenFromList]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Command system cannot use static members")]
	public class ControlModule : RoosterModule {
		[Command("shutdown"), RequireBotManager]
		public Task ShutdownCommand() {
			Log.Info($"Shutdown command used by {Context.User.Name}");
			Program.Instance.Shutdown();
			return Task.CompletedTask;
		}

		[Command("restart"), RequireBotManager]
		public Task RestartCommand() {
			Log.Info($"Restart command used by {Context.User.Name}");
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
