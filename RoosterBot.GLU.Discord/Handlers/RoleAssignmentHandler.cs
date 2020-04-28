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
		private const ulong NewUserRank = 278937741478330389; //669257158969524244;

		public RoleAssignmentHandler() {
			ScheduleUtil.UserChangedIdentifier += OnUserChangedClass;
		}

		private void OnUserChangedClass(object? sender, UserChangedIdentifierEventArgs args) => Task.Run(async () => {
			if (DiscordNetComponent.Instance.Client.GetUser((ulong) args.UserReference.Id) is SocketUser socketUser) {
				IGuildUser? user = socketUser.MutualGuilds.FirstOrDefault(guild => guild.Id == GLUDiscordComponent.GLUGuildId)?.GetUser((ulong) args.UserReference.Id);
				// Assign roles
				if (user != null && args.NewIdentifier is StudentSetInfo newSet) {
					var oldSet = args.OldIdentifier as StudentSetInfo;
					// Assign roles
					try {
						IEnumerable<IRole> newRoles = GluDiscordUtil.GetRolesForStudentSet(user.Guild, newSet);
						if (oldSet != null) {
							IEnumerable<IRole> oldRoles = GluDiscordUtil.GetRolesForStudentSet(user.Guild, oldSet);
							IEnumerable<IRole> keptRoles = oldRoles.Intersect(newRoles);

							oldRoles = oldRoles.Except(keptRoles);
							newRoles = newRoles.Except(keptRoles);

							await user.RemoveRolesAsync(oldRoles);
						}

						if (user.RoleIds.Contains(NewUserRank)) {
							await user.RemoveRoleAsync(user.Guild.GetRole(NewUserRank));
						}

						if (newRoles.Any()) {
							await user.AddRolesAsync(newRoles);
						}
					} catch (Exception e) {
						Logger.Error("GLU-Roles", $"Could not assign roles to user {user.Username}#{user.Discriminator}.");
						foreach (SocketUser admin in DiscordNetComponent.Instance.BotAdminIds.Select(id => DiscordNetComponent.Instance.Client.GetUser(id))) {
							await admin.SendMessageAsync("Failed to assign role: " + e.ToString());
						}
					}
				}
			}
		});
	}
}
