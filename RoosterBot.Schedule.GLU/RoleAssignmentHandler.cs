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
			if (user.GuildId == GLUScheduleComponent.GLUGuildId) {
				// Assign roles
				try {
					// Get roles they're supposed to have
					IEnumerable<IRole> newRoles = GetRolesForStudentSet(user.Guild, newSSI);
					// Roles that they're not supposed to have
					IEnumerable<IRole> oldRoles = m_Roles.Values.Flatten().Distinct().Where(role => user.HasRole(role)).Select(role => user.Guild.GetRole(role));

					oldRoles = oldRoles.Where(role =>  user.HasRole(role.Id)); // Roles to remove
					newRoles = newRoles.Where(role => !user.HasRole(role.Id)); // Roles to add

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
		}

		private IEnumerable<IRole> GetRolesForStudentSet(IGuild guild, StudentSetInfo info) {
			return m_Roles[info.ClassName.Substring(0, 3)].Select(roleId => guild.GetRole(roleId));
		}
	}
}
