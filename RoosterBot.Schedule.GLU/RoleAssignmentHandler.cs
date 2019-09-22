using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace RoosterBot.Schedule.GLU {
	public class RoleAssignmentHandler {
		private IReadOnlyDictionary<string, ulong[]> m_Roles;
		private readonly IDiscordClient m_Client;

		// TODO actually start using this
		// The problem is that it needs both a service and a config path, which are mutually incompatible given the component initialization process.
		// We probably need to load the config during AddServices and then get the UCS during AddModules, but that's for Tomorrow-Me to program.
		public RoleAssignmentHandler(IDiscordClient client, IUserClassesService ucs) {
			// TODO replace with config file
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

			ucs.UserChangedClass += OnUserChangedClass;
			m_Client = client;
		}

		private async void OnUserChangedClass(IUser user, StudentSetInfo oldSSI, StudentSetInfo newSSI) {
			// Assign roles
			try {
				SocketGuild guild = (user as SocketUser)?.MutualGuilds.Where(thisGuild => thisGuild.Id == 278586698877894657).Single();
				if (guild != null) {
					IGuildUser guildUser = guild.GetUser(user.Id);
					IEnumerable<IRole> newRoles = GetRolesForStudentSet(guild, newSSI);
					if (oldSSI != null) {
						IEnumerable<IRole> oldRoles = GetRolesForStudentSet(guild, oldSSI);
						IEnumerable<IRole> keptRoles = oldRoles.Intersect(newRoles);

						oldRoles = oldRoles.Except(keptRoles);
						newRoles = newRoles.Except(keptRoles);

						if (oldRoles.Any()) {
							await guildUser.RemoveRolesAsync(oldRoles);
						}
					}

					if (newRoles.Any()) {
						await guildUser.AddRolesAsync(newRoles);
					}
				}
			} catch (Exception) {
				// Ignore, either we did not have permission or the roles were not found. In either case, it doesn't matter.
			}
		}

		public IEnumerable<IRole> GetRolesForStudentSet(IGuild guild, StudentSetInfo info) {
			return m_Roles[info.ClassName.Substring(0, 3)].Select(roleId => guild.GetRole(roleId));
		}
	}
}
