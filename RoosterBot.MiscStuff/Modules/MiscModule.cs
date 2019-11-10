using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.MiscStuff {
	[HiddenFromList]
	public class MiscModule : RoosterModuleBase {
		public PrankService PrankService { get; set; }

		[Command("dank u", true), Alias(
			"danku", "dankje", "dankjewel", "dank je wel", "dank je", "bedankt",
			"goed", "goedzo", "good bot",
			"thanks", "thnx", "thx", "ty", "thank")]
		public async Task ThankYouCommand() {
			string response;
			if (PrankService.GetAlwaysJoram(Context.User.Id)) {
				response = "<:wsjoram:570601561072467969>";
			} else {
				string[] responses = new[] {
					":smile:",
					":thumbsup:",
					"<:wsjoram:570601561072467969>", // This probably can't make it into 2.0
					":blush:"
				};
				response = responses[Util.RNG.Next(0, responses.Length)];
			}

			await ReplyAsync(response);
		}

		[Command("altijdJoram"), RequireContext(ContextType.DM), HiddenFromList]
		public async Task AlwaysJoramCommand(bool value) {
			PrankService.SetAlwaysJoram(Context.User.Id, value);
			await ReplyAsync($"Je krijgt nu {(value ? "altijd" : "niet altijd")} <:wsjoram:570601561072467969> als je `!bedankt` gebruikt.");
		}
	}
}
