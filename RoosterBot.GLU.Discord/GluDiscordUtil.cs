using System.Collections.Generic;
using System.Linq;
using Discord;

namespace RoosterBot.GLU.Discord {
	internal static class GluDiscordUtil {
		private static readonly IReadOnlyDictionary<string, ulong[]> Roles;

		public const ulong NewUserRank = 278937741478330389; //669257158969524244;

		const ulong DevRole =
			278587815271464970;
			//689201635331014743;

		const ulong ArtRole =
			278587791141765121;
			//689201622911287401;

		private static readonly IReadOnlyList<ulong> YearRoles =
			new ulong[] { 494531025473503252, 494531131606040586, 494531205966987285, 494531269796036618 };
			//new ulong[] { 689201556234829861, 689201576228945998, 689201580825903211, 689201581765296132 };

		static GluDiscordUtil() {
			var roles = new Dictionary<string, ulong[]>();

			void setupYearRoles(ulong courseRole) {
				for (int i = 0; i < YearRoles.Count; i++) {
					string key = (i + 1).ToString() + "G" + (courseRole == DevRole ? "D" : "A");
					roles[key] = new[] { YearRoles[i], courseRole };
				}
			}

			setupYearRoles(DevRole);
			setupYearRoles(ArtRole);

			Roles = roles;
		}

		public static IEnumerable<(IRole Role, RemoveOrAdd)> StudentSetRoles(this IGuildUser user, StudentSetInfo ssi) {
			var neededRoles = Roles[ssi.ScheduleCode.Substring(0, 3)].ToHashSet();

			foreach (ulong role in user.RoleIds) {
				if (neededRoles.Contains(role)) {
					// Keep
					neededRoles.Remove(role);
				} else {
					if (role == DevRole || role == ArtRole || YearRoles.Contains(role)) {
						yield return (user.Guild.GetRole(role), RemoveOrAdd.Remove);
					}
				}
			}

			foreach (ulong role in neededRoles) {
				yield return (user.Guild.GetRole(role), RemoveOrAdd.Add);
			}
		}

		public enum RemoveOrAdd {
			Remove, Add
		}
	}
}
