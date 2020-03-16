using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using RoosterBot.DiscordNet;

namespace RoosterBot.GLU {
	[HiddenFromList]
	// TODO only accept discord context, provide proper mechanism for this (currently you get an exception if the platform is not compatible)
	public class GLUModule : RoosterModule {
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

		[RequireBotManager]
		public async void IndexUserNicknames() {
			IReadOnlyCollection<IGuildUser> users = await (Context as DiscordCommandContext)!.Guild!.GetUsersAsync();
			int i = 0;
			foreach (IGuildUser user in users) {
				if (user.Nickname != null) {
					UserConfig userConfig = await UCS.GetConfigAsync(new SnowflakeReference(DiscordNetComponent.Instance, user.Id));
					userConfig.SetData("glu.discord.nickname", user.Nickname);
					await userConfig.UpdateAsync();
				}
				i++;
				await Task.Delay(500);
				if (i % 25 == 0) {
					await Context.Channel.SendMessageAsync(Context.User.Mention + " " + i + " / " + users.Count);
				}
			}
			await Context.Channel.SendMessageAsync(Context.User.Mention + " done");
		}
	}
}
