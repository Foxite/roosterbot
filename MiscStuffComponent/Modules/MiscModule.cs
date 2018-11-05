using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RoosterBot.Modules;

namespace MiscStuffComponent.Modules {
	public class MiscModule : RoosterModuleBase {
		[Command("send")]
		public async Task SendMessageCommand(ulong channel, [Remainder] string message) {
			await (await Context.Client.GetChannelAsync(channel) as ITextChannel).SendMessageAsync(message);
		}

		[Command("delete")]
		public async Task DeleteMessageCommand(ulong channel, ulong msg) {
			await (await (await Context.Client.GetChannelAsync(channel) as ITextChannel).GetMessageAsync(msg)).DeleteAsync();
		}

		[Command("test")]
		public async Task TestCommand() {
			await Context.Channel.SendMessageAsync("test ok");
		}
	}
}
