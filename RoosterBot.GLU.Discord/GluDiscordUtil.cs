using System.Collections.Generic;
using System.Linq;
using Discord;
using RoosterBot.Schedule;

namespace RoosterBot.GLU.Discord {
	internal static class GluDiscordUtil {
		private static readonly IReadOnlyDictionary<string, ulong[]> Roles;

		static GluDiscordUtil() {
			var yearRoles = new ulong[] { 494531025473503252, 494531131606040586, 494531205966987285, 494531269796036618 };
			//ulong[] yearRoles = new ulong[] { 689201556234829861, 689201576228945998, 689201580825903211, 689201581765296132 };

			var roles = new Dictionary<string, ulong[]>();

			ulong dev = 278587815271464970;
			ulong art = 278587791141765121;
			//ulong dev = 689201635331014743;
			//ulong art = 689201622911287401;

			void setupYearRoles(ulong courseRole) {
				for (int i = 0; i < yearRoles.Length; i++) {
					string key = (i + 1).ToString() + "G" + (courseRole == dev ? "D" : "A");
					roles[key] = new[] { yearRoles[i], courseRole };
				}
			}

			setupYearRoles(dev);
			setupYearRoles(art);

			Roles = roles;
		}

		public static IEnumerable<IRole> GetRolesForStudentSet(IGuild guild, StudentSetInfo? info) {
			IEnumerable<ulong> ids;
			if (info is null) {
				ids = Roles.Values.SelectMany(i => i).Distinct();
			} else {
				ids = Roles[info.ScheduleCode.Substring(0, 3)];
			}
			return ids.Select(roleId => guild.GetRole(roleId));
		}
	}
}
