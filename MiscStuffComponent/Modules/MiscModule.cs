using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RoosterBot.Attributes;
using RoosterBot.Modules;
using RoosterBot.Preconditions;

namespace MiscStuffComponent.Modules {
	[LogTag("MiscModule")]
	public class MiscModule : RoosterModuleBase {
		[Command("send"), RequireBotManager, HiddenFromList]
		public async Task SendMessageCommand(ulong channel, [Remainder] string message) {
			await (await Context.Client.GetChannelAsync(channel) as ITextChannel).SendMessageAsync(message);
		}

		[Command("delete"), RequireBotManager, HiddenFromList]
		public async Task DeleteMessageCommand(ulong channel, ulong msg) {
			await (await (await Context.Client.GetChannelAsync(channel) as ITextChannel).GetMessageAsync(msg)).DeleteAsync();
		}

		[Command("test"), RequireBotManager, HiddenFromList]
		public async Task TestCommand([Remainder] string ignored = "") {
			await Context.Channel.SendMessageAsync("test ok");
		}
	}
}
