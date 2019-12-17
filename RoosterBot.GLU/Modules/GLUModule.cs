using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.GLU {
	[HiddenFromList]
	public class GLUModule : RoosterModule {
		[Command("danku", "dankje", "dankjewel", "bedankt", "dank",
				 "goed", "goedzo", "goodbot",
				 "thanks", "thnx", "thx", "ty", "thank"), IgnoresExtraArguments]
		public Task<CommandResult> ThankYouCommand() {
			string response;
			if (UserConfig.TryGetData("misc.alwaysjoram", out bool alwaysJoram, false) && alwaysJoram) {
				response = "<:wsjoram:570601561072467969>";
			} else {
				string[] responses = new[] {
					":smile:",
					":thumbsup:",
					"<:wsjoram:570601561072467969>",
					":blush:",
					":heart:"
				};
				response = responses[Util.RNG.Next(0, responses.Length)];
			}

			return Result(new TextResult(null, response));
		}

		[Command("altijdJoram"), RequireContext(ContextType.DM)]
		public Task<CommandResult> AlwaysJoramCommand(bool value) {
			UserConfig.SetData("misc.alwaysjoram", value);
			return Result(TextResult.Success($"Je krijgt nu {(value ? "altijd" : "niet altijd")} <:wsjoram:570601561072467969> als je `!bedankt` gebruikt."));
		}

		[Command("kut"), IgnoresExtraArguments]
		public Task<CommandResult> BoazBas() {
			return Result(new TextResult(null, "Kut " + Context.User.Mention));
		}
	}
}
