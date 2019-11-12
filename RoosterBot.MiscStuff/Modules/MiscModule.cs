using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.MiscStuff {
	public class MiscModule : RoosterModuleBase {
		[Command("dank u", true), Alias("danku", "dankje", "dankjewel", "dank je wel", "dank je", "bedankt", "goed zo", "goedzo", "thanks", "thx")]
		public async Task ThankYouCommand() {
			string response;
			if (UserConfig.TryGetData("misc.alwaysjoram", out bool alwaysJoram, false) && alwaysJoram) {
				response = "<:wsjoram:570601561072467969>";
			} else {
				string[] responses = new[] {
					":smile:",
					":thumbsup:",
					"<:wsjoram:570601561072467969>",
					":blush:"
				};
				response = responses[Util.RNG.Next(0, responses.Length)];
			}

			await ReplyAsync(response);
		}

		[Command("altijdJoram"), RequireContext(ContextType.DM), HiddenFromList]
		public async Task AlwaysJoramCommand(bool value) {
			UserConfig.SetData("misc.alwaysjoram", value);
			await UserConfig.UpdateAsync();
			ReplyDeferred($"Je krijgt nu {(value ? "altijd" : "niet altijd")} <:wsjoram:570601561072467969> als je `!bedankt` gebruikt.");
		}
	}
}
