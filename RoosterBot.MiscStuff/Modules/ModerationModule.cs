using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.MiscStuff {
	[HiddenFromList]
	public class ModerationModule : RoosterModuleBase {
		[Command("users unnamed"), UserIsModerator]
		public async Task GetUnnamedUsers() {
			IEnumerable<string[]> unnamedUsers =
				from user in await Context.Guild.GetUsersAsync()
				where !user.IsBot && user.Nickname == null
				orderby user.JoinedAt?.Date
				select new[] { user.JoinedAt?.ToString("yyyy-MM-dd") ?? "Unknown", $"@{user.Username}#{user.Discriminator}" };

			string[][] table = new string[unnamedUsers.Count() + 1][];
			table[0] = new[] { "Joined", "Username" };
			unnamedUsers.CopyTo(table, 1);

			/*foreach (IGuildUser user in unnamedUsers) {
				response += "`";
				if (user.JoinedAt != null) {
					response += user.JoinedAt.Value.ToString("yyyy-MM-dd");
				} else {
					response += "Unknown   ";
				}
				response += $"`: @{user.Username}#{user.Discriminator}\n";
			}*/
			await base.ReplyAsync(Util.FormatTextTable(table));
		}
	}
}
