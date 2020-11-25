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

		public UserConfigService UCS { get; set; } = null!;

		public static async Task<IEnumerable<DiscordUser>> GetList(DiscordCommandContext context) {
			IEnumerable<DiscordUser> userList = (await context.Guild!.GetUsersAsync()).Select(igu => new DiscordUser(igu));

			return context.UserConfig.TryGetData<IEnumerable<ulong>>(UserConfigListKey, out IEnumerable<ulong>? ret)
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
			return ReplyList(Context, (await GetList(Context)).Where(user => ((IGuildUser) user.DiscordEntity).Nickname == null));
		}

		[Command("with role count")]
		public async Task<CommandResult> UsersWithRolecount(string comparison, int count, params IRole[] roles) {
			if (!TryGetCompareFunc(comparison, out Func<int, int, bool>? compare)) {
				return TextResult.Error("Invalid comparison.");
			}

			return ReplyList(Context,
				from user in await GetList(Context)
				let roleCount =
					roles.Length == 0
						? (((IGuildUser) user.DiscordEntity).RoleIds.Count - 1) // Everyone has a role called "@everyone" so ignore that one
						: ((IGuildUser) user.DiscordEntity).RoleIds.Intersect(roles.Select(role => role.Id)).Count()
				where compare(roleCount, count)
				select user
			);
		}

		[Command("with status")]
		public async Task<CommandResult> UsersWithStatus(UserStatus status) {
			return ReplyList(Context, (await GetList(Context)).Where(user => user.DiscordEntity.Status == status));
		}

		[Command("with config value")]
		public async Task<CommandResult> UsersWithConfigValue(string key, string value) {

			return ReplyList(Context, (await GetList(Context)).Where(user => {
				Newtonsoft.Json.Linq.JObject? data = UCS.GetConfigAsync(user.GetReference()).Result?.GetRawData();
				if (data != null) {
					Newtonsoft.Json.Linq.JToken? jt = data.SelectToken(key);
					string? v = jt?.ToObject<string>();
					return v == value;
				} else {
					return false;
				}
			}));
		}

		private static bool TryGetCompareFunc(string name, [NotNullWhen(true)] out Func<int, int, bool>? func) {
			func = name switch {
				"==" => (a, b) => a == b,
				"!=" => (a, b) => a != b,
				">"  => (a, b) => a >  b,
				">=" => (a, b) => a >= b,
				"<"  => (a, b) => a <  b,
				"<=" => (a, b) => a <= b,
				_    => null
			};
			return func != null;
		}

		public static RoosterCommandResult ReplyList(DiscordCommandContext context, IEnumerable<DiscordUser> users) {
			if (users.Any()) {
				IEnumerable<string[]> userRows =
					from rbUser in users
					let user = (IGuildUser) rbUser.DiscordEntity
					orderby user.JoinedAt?.Date
					select new[] {
						$"@{user.Username}#{user.Discriminator}",
						user.JoinedAt?.ToString("yyyy-MM-dd") ?? "Unknown",
						string.Join(", ", user.RoleIds.Select(roleId => context.Guild!.GetRole(roleId).Name).Where(roleName => roleName != "@everyone"))
					};

				string[][] table = new string[userRows.Count() + 1][];
				table[0] = new[] { "Username", "Joined", "Roles" };
				userRows.CopyTo(table, 1);

				context.UserConfig.SetData(UserConfigListKey, users.Select(user => user.Id));

				return new TableResult("", table);
			} else {
				context.UserConfig.RemoveData(UserConfigListKey);

				return TextResult.Info("No results.");
			}
		}
	}
}
