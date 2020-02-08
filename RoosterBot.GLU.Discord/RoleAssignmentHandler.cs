using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using RoosterBot.DiscordNet;
using RoosterBot.Schedule;

namespace RoosterBot.GLU.Discord {
	internal sealed class RoleAssignmentHandler {
		private const ulong NewUserRank = 278937741478330389;
		private readonly IReadOnlyDictionary<string, ulong[]> m_Roles;

		public RoleAssignmentHandler() {
			ulong[] yearRoles = new ulong[] { 494531025473503252, 494531131606040586, 494531205966987285, 494531269796036618 };

			var roles = new Dictionary<string, ulong[]>();

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

			ScheduleUtil.UserChangedIdentifier += OnUserChangedClass;
		}

		private async Task OnUserChangedClass(UserChangedIdentifierEventArgs args) {
			if (DiscordNetComponent.Instance.Client.GetUser((ulong) args.UserReference.Id) is SocketUser socketUser) {
				IGuildUser? user = socketUser.MutualGuilds.FirstOrDefault(guild => guild.Id == GLUDiscordComponent.GLUGuildId)?.GetUser((ulong) args.UserReference.Id);
				// Assign roles
				if (user != null) {
					// Assign roles
					try {
						IEnumerable<IRole> newRoles = GetRolesForStudentSet(user.Guild, args.NewIdentifier);
						if (args.OldIdentifier != null) {
							IEnumerable<IRole> oldRoles = GetRolesForStudentSet(user.Guild, args.OldIdentifier);
							IEnumerable<IRole> keptRoles = oldRoles.Intersect(newRoles);

							oldRoles = oldRoles.Except(keptRoles);
							newRoles = newRoles.Except(keptRoles);

							foreach (IRole role in oldRoles) {
								if (user.RoleIds.Contains(role.Id)) {
									await user.RemoveRolesAsync(oldRoles);
								}
							}
						}
						if (user.RoleIds.Contains(NewUserRank)) {
							await user.RemoveRoleAsync(user.Guild.GetRole(NewUserRank));
						}

						if (newRoles.Any()) {
							await user.AddRolesAsync(newRoles);
						}
					} catch (Exception e) {
						Logger.Error("GLU-Roles", $"Could not assign roles to user {user.Username}#{user.Discriminator}.", e);
						await DiscordNetComponent.Instance.BotOwner.SendMessageAsync("Failed to assign role: " + e.ToString());
					}
				}
			}
		}

		private IEnumerable<IRole> GetRolesForStudentSet(IGuild guild, IdentifierInfo info) {
			return m_Roles[info.ScheduleCode.Substring(0, 3)].Select(roleId => guild.GetRole(roleId));
		}
	}
}
