using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot.Schedule.GLU {
	public class RoleAssignmentHandler {
		private const long NewUserRank = 278937741478330389;
		private readonly IReadOnlyDictionary<string, ulong[]> m_Roles;
		private readonly ConfigService m_Config;
		private readonly IDiscordClient m_Client;

		public RoleAssignmentHandler(IDiscordClient client, ConfigService config) {
			m_Client = client;
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

			ScheduleUtil.UserChangedClass += OnUserChangedClass;
		}

		private async Task OnUserChangedClass(ulong userId, StudentSetInfo? oldSSI, StudentSetInfo newSSI) {
			if ((await m_Client.GetUserAsync(userId)) is SocketUser socketUser && socketUser.MutualGuilds.Count != 0) {
				IGuildUser? user = socketUser.MutualGuilds.FirstOrDefault(guild => guild.Id == GLUScheduleComponent.GLUGuildId)?.GetUser(userId);
				// Assign roles
				if (user != null) {
					// Assign roles
					try {
						IEnumerable<IRole> newRoles = GetRolesForStudentSet(user.Guild, newSSI);
						if (oldSSI != null) {
							IEnumerable<IRole> oldRoles = GetRolesForStudentSet(user.Guild, oldSSI);
							IEnumerable<IRole> keptRoles = oldRoles.Intersect(newRoles);

							oldRoles = oldRoles.Except(keptRoles);
							newRoles = newRoles.Except(keptRoles);

							foreach (IRole role in oldRoles) {
								if (user.HasRole(role.Id)) {
									await user.RemoveRolesAsync(oldRoles);
								}
							}
							if (user.HasRole(NewUserRank)) {
								await user.RemoveRoleAsync(user.Guild.GetRole(NewUserRank));
							}
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
		}

		private IEnumerable<IRole> GetRolesForStudentSet(IGuild guild, StudentSetInfo info) {
			return m_Roles[info.ScheduleCode.Substring(0, 3)].Select(roleId => guild.GetRole(roleId));
		}
	}
}
