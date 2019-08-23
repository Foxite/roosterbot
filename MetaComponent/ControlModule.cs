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

		[Command("send"), RequireBotManager]
		public async Task SendMessageCommand(IMessageChannel channel, [Remainder] string message) {
			try {
				await channel.SendMessageAsync(message);
			} catch {
				await ReplyAsync("Error sending message");
			}
		}

		[Command("delete"), RequireBotManager]
		public async Task DeleteMessageCommand(IMessageChannel channel, ulong msg) {
			IMessage message = await channel.GetMessageAsync(msg);
			if (message != null) {
				try {
					await message.DeleteAsync();
				} catch {
					await ReplyAsync("Error deleting message");
				}
			} else {
				await ReplyAsync("Message not found");
			}
		}
	}
}
