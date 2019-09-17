using Discord;
using ScheduleComponent.DataTypes;
using System.Collections.Generic;
using System.Linq;

namespace ScheduleComponent.Services {
	// A pretty big hack, should touch up as soon as possible
	public class UserClassRoleService {
		private IReadOnlyDictionary<string, ulong[]> m_Roles;

		public UserClassRoleService() {
			ulong[] yearRoles = new ulong[] { 494531025473503252, 494531131606040586, 494531205966987285, 494531269796036618 };

			Dictionary<string, ulong[]> roles = new Dictionary<string, ulong[]>();

			ulong dev = 278587815271464970;
			ulong art = 278587928173740032;

			void setupYearRoles(ulong courseRole) {
				for (int i = 0; i < yearRoles.Length; i++) {
					string key = (i + 1).ToString() + "G" + (courseRole == dev ? "D" : "A");
					roles[key] = new[] { yearRoles[i], courseRole };
				}
			}

			setupYearRoles(dev);
			setupYearRoles(art);

			m_Roles = roles;
		}

		public IEnumerable<IRole> GetRolesForStudentSet(IGuild guild, StudentSetInfo info) {
			return m_Roles[info.ClassName.Substring(0, 3)].Select(roleId => guild.GetRole(roleId));
		}
	}
}
