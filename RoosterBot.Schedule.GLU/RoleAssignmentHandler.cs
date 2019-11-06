using System;
using System.Collections.Generic;
using System.Linq;
using Discord;

namespace RoosterBot.Schedule.GLU {
	public class RoleAssignmentHandler {
		private const long NewUserRanks = 278937741478330389;
		private IReadOnlyDictionary<string, ulong[]> m_Roles;
		private readonly ConfigService m_Config;

		public RoleAssignmentHandler(IUserClassesService ucs, ConfigService config) {
			m_Config = config;
			ulong[] yearRoles = new ulong[] { 494531025473503252, 494531131606040586, 494531205966987285, 494531269796036618 };

			Dictionary<string, ulong[]> roles = new Dictionary<string, ulong[]>();

			ulong dev = 278587815271464970;
			ulong art = 278587791141765121;

			void setupYearRoles(ulong courseRole) {
				for (int i = 0; i < yearRoles.Length; i++) {
					string key = (i + 1).ToString() + "G" + (courseRole == dev ? "D" : "A");
					roles[key] = new[] { yearRoles[i], courseRole };
				}
			}

			setupYearRoles(dev);
			setupYearRoles(art);

			m_Roles = roles;

			ucs.UserChangedClass += OnUserChangedClass;
		}

		private async void OnUserChangedClass(IGuildUser user, StudentSetInfo oldSSI, StudentSetInfo newSSI) {
			// Assign roles
			try {
				IEnumerable<IRole> newRoles = GetRolesForStudentSet(user.Guild, newSSI);
				IEnumerable<IRole> oldRoles;
				if (oldSSI != null) {
					oldRoles = GetRolesForStudentSet(user.Guild, oldSSI);
				} else {
					// Check if the user has any roles that belong to a student set, even though they didn't have a known student set
					List<ulong> oldRoleList = new List<ulong>();
					foreach (ulong role in m_Roles.SelectMany(kvp => kvp.Value)) {
						if (user.HasRole(role)) {
							oldRoleList.Add(role);
						}
					}
					if (user.HasRole(NewUserRanks)) {
						oldRoleList.Add(NewUserRanks);
					}

					oldRoles = oldRoleList.Select(role => user.Guild.GetRole(role));
				}
				IEnumerable<IRole> keptRoles = oldRoles.Intersect(newRoles);

				oldRoles = oldRoles.Except(keptRoles);
				newRoles = newRoles.Except(keptRoles);

				if (oldRoles.Any()) {
					await user.RemoveRolesAsync(oldRoles);
				}

				if (newRoles.Any()) {
					await user.AddRolesAsync(newRoles);
				}
			} catch (Exception e) {
				Logger.Error("GLU-Roles", $"Could not assign roles to user {user.Username}#{user.Discriminator}.", e);
				await m_Config.BotOwner.SendMessageAsync("Failed to assign role: " + e.ToString());
			}
		}

		private IEnumerable<IRole> GetRolesForStudentSet(IGuild guild, StudentSetInfo info) {
			return m_Roles[info.ClassName.Substring(0, 3)].Select(roleId => guild.GetRole(roleId));
		}
	}
}
