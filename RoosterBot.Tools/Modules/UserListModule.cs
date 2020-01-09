/* TODO discord
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot.Tools {
	[HiddenFromList, Group("users"), RequireContext(ContextType.Guild), UserIsModerator]
	public class UserListModule : RoosterModule {
		[Command("with no nickname")]
		public async Task<CommandResult> GetUnnamedUsers() {
			return ReplyList(
				from user in await Context.Guild!.GetUsersAsync()
				where !user.IsBot && user.Nickname == null
				select user
			);
		}

		[Command("with role count")]
		public async Task<CommandResult> UsersWithRolecount(string comparison, int count, params IRole[] roles) {
			if (!TryGetCompareFunc(comparison, out Func<int, int, bool>? compare)) {
				return TextResult.Error("Invalid comparison.");
			}

			return ReplyList(
				from user in await Context.Guild!.GetUsersAsync()
				where !user.IsBot
				let roleCount =
					roles.Length == 0
						? (user.RoleIds.Count - 1) // Everyone has a role called "@everyone" so ignore that one
						: user.RoleIds.Intersect(roles.Select(role => role.Id)).Count()
				where compare(roleCount, count)
				select user
			);
		}

		private bool TryGetCompareFunc(string name, [NotNullWhen(true)] out Func<int, int, bool>? func) {
			switch (name) {
				case "==": func = (a, b) => a == b; return true;
				case "!=": func = (a, b) => a != b; return true;
				case ">" : func = (a, b) => a >  b; return true;
				case ">=": func = (a, b) => a >= b; return true;
				case "<" : func = (a, b) => a <  b; return true;
				case "<=": func = (a, b) => a <= b; return true;
				default:   func = null;             return false;
			}
		}

		private TableResult ReplyList(IEnumerable<IGuildUser> unnamedUsers) {
			IEnumerable<string[]> userRows =
				from user in unnamedUsers
				orderby user.JoinedAt?.Date
				select new[] {
					$"@{user.Username}#{user.Discriminator}",
					user.JoinedAt?.ToString("yyyy-MM-dd") ?? "Unknown",
					string.Join(", ", user.RoleIds.Select(roleId => Context.Guild!.GetRole(roleId).Name).Where(roleName => roleName != "@everyone"))
				};

			string[][] table = new string[userRows.Count() + 1][];
			table[0] = new[] { "Username", "Joined", "Roles" };
			userRows.CopyTo(table, 1);

			return new TableResult("", table);
		}
	}
}
*/