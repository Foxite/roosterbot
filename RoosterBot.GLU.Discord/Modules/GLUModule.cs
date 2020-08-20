using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using RoosterBot.DiscordNet;
using RoosterBot.GLU.Discord;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	[HiddenFromList]
	// TODO only accept discord context, provide proper mechanism for this (currently you get an exception if the platform is not compatible)
	public class GLUModule : RoosterModule<DiscordCommandContext> {
		public UserConfigService UCS { get; set; } = null!;

		[Priority(-1), Command(
			"danku", "dankje", "dankjewel", "bedankt", "dank",
			"goed", "goedzo", "goodbot", "good",
			"thanks", "thnx", "thx", "ty", "thank")]
		public CommandResult ThankYouCommand([Remainder] string post = "") {
			string response;
			double joramChance = Context.User.Id.Equals(244147515375484928ul) ? 0.1d : 0.2d;
			if (Util.RNG.NextDouble() < joramChance || post.ToLower() == "joram" || (UserConfig.TryGetData("misc.alwaysjoram", out bool alwaysJoram, false) && alwaysJoram)) {
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
					":love_you_gesture:"
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

		[Command("users with mismatched student set roles"), UserIsModerator]
		public async Task<CommandResult> CheckMismatchedRoles() {
			var ucs = (UserConfigService) Context.ServiceProvider.GetService(typeof(UserConfigService));
			return UserListModule.ReplyList(Context, (await UserListModule.GetList(Context))
				.Where(user => Task.Run(async () => {
					var config = await ucs.GetConfigAsync(user.GetReference());
					StudentSetInfo? ssi = config.GetStudentSet();
					if (ssi != null) {
						return ((IGuildUser) user.DiscordEntity).StudentSetRoles(ssi).Any();
					} else {
						return false;
					}
				}).Result)
			);
		}
	}
}
