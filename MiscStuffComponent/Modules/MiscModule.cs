using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RoosterBot;
using RoosterBot.Attributes;
using RoosterBot.Modules;
using RoosterBot.Preconditions;

namespace MiscStuffComponent.Modules {
	[LogTag("MiscModule"), HiddenFromList]
	public class MiscModule : RoosterModuleBase {
		[Command("send"), RequireBotManager, HiddenFromList]
		public async Task SendMessageCommand(ulong channel, [Remainder] string message) {
			await (await Context.Client.GetChannelAsync(channel) as ITextChannel).SendMessageAsync(message);
		}

		[Command("delete"), RequireBotManager, HiddenFromList]
		public async Task DeleteMessageCommand(ulong channel, ulong msg) {
			await (await (await Context.Client.GetChannelAsync(channel) as ITextChannel).GetMessageAsync(msg)).DeleteAsync();
		}

		[Command("dank u"), Alias("danku", "dankje", "dankjewel", "dank je wel", "dank je", "bedankt", "thanks", "thx")]
		public async Task ThankYouCommand() {
			string[] responses = new[] {
				":smile:",
				":thumbsup:",
				"<:wsjoram:629327051723243551>", // This probably can't make it into 2.0
				":blush:"
			};

			await ReplyAsync(responses[Util.RNG.Next(0, responses.Length)]);
		}

		[Command("test")]
		public async Task TestCommand() {
			await ReplyAsync("Working! :tada:");
		}
	}
}
