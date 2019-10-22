﻿using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;

namespace RoosterBot.Schedule.GLU {
	public class RoleAssignmentHandler {
		private IReadOnlyDictionary<string, ulong[]> m_Roles;

		public RoleAssignmentHandler(IUserClassesService ucs) {
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

		private async void OnUserChangedClass(IUser user, StudentSetInfo oldSSI, StudentSetInfo newSSI) {
			// Assign roles
			try {
				SocketGuild guild = (user as SocketUser)?.MutualGuilds.Where(thisGuild => thisGuild.Id == GLUScheduleComponent.GLUGuildId).SingleOrDefault();
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
			} catch (Exception e) {
				Logger.Error("GLU-Roles", $"Could not assign roles to user {user.Username}#{user.Discriminator}.", e);
			}
		}

		private IEnumerable<IRole> GetRolesForStudentSet(IGuild guild, StudentSetInfo info) {
			return m_Roles[info.ClassName.Substring(0, 3)].Select(roleId => guild.GetRole(roleId));
		}
	}
}
