﻿using Qmmands;

namespace RoosterBot.GLU {
	[HiddenFromList]
	public class GLUModule : RoosterModule {
		[Priority(-1), Command(
			"danku", "dankje", "dankjewel", "bedankt", "dank",
			"goed", "goedzo", "goodbot", "good",
			"thanks", "thnx", "thx", "ty", "thank")]
		public CommandResult ThankYouCommand([Remainder] string post = "") {
			string response;
			if (post.ToLower() == "joram" || (UserConfig.TryGetData("misc.alwaysjoram", out bool alwaysJoram, false) && alwaysJoram)) {
				response = "<:wsjoram:570601561072467969>";
			} else {
				string[] responses = new[] {
					":smile:",
					":smiley:",
					":grin:",
					":blush:",
					":smiling_face_with_3_hearts:",
					":hugging:",
					":smiley_cat:",
					":thumbsup:",
					":love_you_gesture:",
					"<:wsjoram:570601561072467969>"
				};
				int selection = Util.RNG.Next(0, responses.Length + 1);
				if (selection == 0) {
					string[] hearts = new[] {
						":heart:",
						":orange_heart:",
						":yellow_heart:",
						":green_heart:",
						":blue_heart:",
						":purple_heart:",
						":brown_heart:",
						":white_heart:",
						":two_hearts:",
						":heartpulse:"
					};
					response = hearts[Util.RNG.Next(0, hearts.Length)];
				} else {
					response = responses[selection - 1];
				}
			}

			return new TextResult(null, response);
		}

		[Command("altijdJoram"), RequirePrivate(true)]
		public CommandResult AlwaysJoramCommand(bool value) {
			UserConfig.SetData("misc.alwaysjoram", value);
			return TextResult.Success($"Je krijgt nu {(value ? "altijd" : "niet altijd")} <:wsjoram:570601561072467969> als je `!bedankt` gebruikt.");
		}
	}
}
