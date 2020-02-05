using Qmmands;

namespace RoosterBot.GLU {
	[HiddenFromList]
	public class GLUModule : RoosterModule {
		[Priority(-1),
		 Command("danku", "dankje", "dankjewel", "bedankt", "dank",
				 "goed", "goedzo", "goodbot", "good",
				 "thanks", "thnx", "thx", "ty", "thank")]
		public CommandResult ThankYouCommand([Remainder] string post = "") {
			string response;
			if (post.ToLower() == "joram" || (UserConfig.TryGetData("misc.alwaysjoram", out bool alwaysJoram, false) && alwaysJoram)) {
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

			return new TextResult(null, response);
		}

		[Command("altijdJoram"), RequirePrivate(true)]
		public CommandResult AlwaysJoramCommand(bool value) {
			UserConfig.SetData("misc.alwaysjoram", value);
			return TextResult.Success($"Je krijgt nu {(value ? "altijd" : "niet altijd")} <:wsjoram:570601561072467969> als je `!bedankt` gebruikt.");
		}

		[Command("kut"), IgnoresExtraArguments]
		public CommandResult BoazBas() {
			return new TextResult(null, "Kut " + Context.User.Mention);
		}
	}
}
