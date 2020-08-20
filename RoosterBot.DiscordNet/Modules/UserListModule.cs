using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot.DiscordNet {
	[HiddenFromList, Group("users"), RequirePrivate(false), UserIsModerator]
	public class UserListModule : RoosterModule<DiscordCommandContext> {
		private const string UserConfigListKey = "discord.tools.userList";

		private async Task<IEnumerable<DiscordUser>> GetList() {
			IEnumerable<DiscordUser> userList = (await Context.Guild!.GetUsersAsync()).Select(igu => new DiscordUser(igu));

			return UserConfig.TryGetData<IEnumerable<ulong>>(UserConfigListKey, out IEnumerable<ulong>? ret)
				? userList.Join(ret, user => user.Id, id => id, (igu, id) => igu)
				: userList.Where(user => !user.DiscordEntity.IsBot);
		}

		[Command("clear")]
		public CommandResult ClearContext() {
			UserConfig.RemoveData(UserConfigListKey);
			return TextResult.Success("Context has been cleared.");
		}

		[Command("with no nickname")]
		public async Task<CommandResult> GetUnnamedUsers() {
			return ReplyList((await GetList()).Where(user => ((IGuildUser) user.DiscordEntity).Nickname == null));
		}

		[Command("with role count")]
		public async Task<CommandResult> UsersWithRolecount(string comparison, int count, params IRole[] roles) {
			if (!TryGetCompareFunc(comparison, out Func<int, int, bool>? compare)) {
				return TextResult.Error("Invalid comparison.");
			}

			return ReplyList(
				from user in await GetList()
				let roleCount =
					roles.Length == 0
						? (((IGuildUser) user.DiscordEntity).RoleIds.Count - 1) // Everyone has a role called "@everyone" so ignore that one
						: ((IGuildUser) user).RoleIds.Intersect(roles.Select(role => role.Id)).Count()
				where compare(roleCount, count)
				select user
			);
		}

		[Command("with status")]
		public async Task<CommandResult> UsersWithStatus(UserStatus status) {
			return ReplyList((await GetList()).Where(user => user.DiscordEntity.Status == status));
		}

		[Command("with config value")]
		public async Task<CommandResult> UsersWithConfigValue(string key, string value) {
			var ucs = (UserConfigService) Context.ServiceProvider.GetService(typeof(UserConfigService));

			return ReplyList((await GetList()).Where(user => {
				Newtonsoft.Json.Linq.JObject? data = ucs.GetConfigAsync(user.GetReference()).Result?.GetRawData();
				if (data != null) {
					Newtonsoft.Json.Linq.JToken? jt = data.SelectToken(key);
					string? v = jt?.ToObject<string>();
					return v == value;
				} else {
					return false;
				}
			}));
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

		private RoosterCommandResult ReplyList(IEnumerable<DiscordUser> users) {
			if (users.Any()) {
				IEnumerable<string[]> userRows =
					from rbUser in users
					let user = (IGuildUser) rbUser.DiscordEntity
					orderby user.JoinedAt?.Date
					select new[] {
						$"@{user.Username}#{user.Discriminator}",
						user.JoinedAt?.ToString("yyyy-MM-dd") ?? "Unknown",
						string.Join(", ", user.RoleIds.Select(roleId => Context.Guild!.GetRole(roleId).Name).Where(roleName => roleName != "@everyone"))
					};

				string[][] table = new string[userRows.Count() + 1][];
				table[0] = new[] { "Username", "Joined", "Roles" };
				userRows.CopyTo(table, 1);

				UserConfig.SetData(UserConfigListKey, users.Select(user => user.Id));

				return new TableResult("", table);
			} else {
				UserConfig.RemoveData(UserConfigListKey);

				return TextResult.Info("No results.");
			}
		}
	}
}
