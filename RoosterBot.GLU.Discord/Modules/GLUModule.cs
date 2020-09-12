using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using RoosterBot.DiscordNet;
using RoosterBot.GLU.Discord;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	[HiddenFromList]
	public class GLUModule : RoosterModule<DiscordCommandContext> {
		public Random RNG { get; set; } = null!;
		public UserConfigService UCS { get; set; } = null!;

		[Priority(-1), Command(
			"danku", "dankje", "dankjewel", "bedankt", "dank",
			"goed", "goedzo", "goodbot", "good",
			"thanks", "thnx", "thx", "ty", "thank")]
		public CommandResult ThankYouCommand([Remainder] string post = "") {
			string response;
			double joramChance = Context.User.Id.Equals(244147515375484928ul) ? 0.1d : 0.2d;
			if (RNG.NextDouble() < joramChance || post.ToLower() == "joram" || (UserConfig.TryGetData("misc.alwaysjoram", out bool alwaysJoram, false) && alwaysJoram)) {
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
					"<:wsjoram:570601561072467969>",
					":blush:",
					":heart:"
				};
				response = responses[RNG.Next(0, responses.Length)];
			}

			return new TextResult(null, response);
		}

		[Command("altijdJoram"), RequirePrivate(true)]
		public CommandResult AlwaysJoramCommand(bool value) {
			UserConfig.SetData("misc.alwaysjoram", value);
			return TextResult.Success($"Je krijgt nu {(value ? "altijd" : "niet altijd")} <:wsjoram:570601561072467969> als je `!bedankt` gebruikt.");
		}

		[Command("users with mismatched student set roles"), UserIsModerator]
		public async Task<CommandResult> CheckMismatchedRoles() {
			return UserListModule.ReplyList(Context, (await UserListModule.GetList(Context))
				.Where(user => Task.Run(async () => {
					var config = await UCS.GetConfigAsync(user.GetReference());
					if (config.GetIdentifier() is StudentSetInfo ssi) {
						return ((IGuildUser) user.DiscordEntity).StudentSetRoles(ssi).Any();
					} else {
						return false;
					}
				}).Result)
			);
		}
	}
}
