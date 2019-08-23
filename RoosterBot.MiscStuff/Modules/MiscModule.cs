using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.MiscStuff {
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
	}
}
