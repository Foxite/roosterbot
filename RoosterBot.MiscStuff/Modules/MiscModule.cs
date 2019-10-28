using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.MiscStuff {
	public class MiscModule : RoosterModuleBase {
		[Command("dank u"), Alias("danku", "dankje", "dankjewel", "dank je wel", "dank je", "bedankt", "goed zo", "goedzo", "thanks", "thx")]
		public async Task ThankYouCommand() {
			string[] responses = new[] {
				":smile:",
				":thumbsup:",
				"<:wsjoram:570601561072467969>", // This probably can't make it into 2.0
				":blush:"
			};

			await ReplyAsync(responses[Util.RNG.Next(0, responses.Length)]);
		}
	}
}
