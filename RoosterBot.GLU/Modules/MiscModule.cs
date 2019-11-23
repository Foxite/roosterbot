using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.GLU {
	[HiddenFromList]
	public class MiscModule : RoosterModuleBase {
		[Command("danku", "dankje", "dankjewel", "bedankt", "dank",
				 "goed", "goedzo", "goodbot",
				 "thanks", "thnx", "thx", "ty", "thank"), IgnoresExtraArguments]
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

		[Command("altijdJoram"), RequireContext(ContextType.DM)]
		public async Task AlwaysJoramCommand(bool value) {
			UserConfig.SetData("misc.alwaysjoram", value);
			await UserConfig.UpdateAsync();
			ReplyDeferred($"Je krijgt nu {(value ? "altijd" : "niet altijd")} <:wsjoram:570601561072467969> als je `!bedankt` gebruikt.");
		}

		[Command("kut"), IgnoresExtraArguments]
		public Task BoazBas() {
			ReplyDeferred("Kut " + Context.User.Mention);
			return Task.CompletedTask;
		}
	}
}
