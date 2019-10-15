using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiscStuffComponent.Services {
	public class PropagationService {
		private readonly IDiscordClient m_Client;
		private readonly Dictionary<ulong, ulong> m_PropagatedRoles;

		public PropagationService(IDiscordClient client) {
			m_Client = client;
			m_PropagatedRoles = new Dictionary<ulong, ulong>();
			m_PropagatedRoles[278586698877894657] = 633610934979395584; // TODO config file
		}

		public ulong GetPropagatedRoleId(IGuild guild) {
			return m_PropagatedRoles[guild.Id];
		}

		public async Task<PropagationStats> GetPropagationStats(IGuild guild) {
			ulong propagatedRoleId = GetPropagatedRoleId(guild);
			IReadOnlyCollection<IGuildUser> users = await guild.GetUsersAsync();

			var stats = new PropagationStats();

			foreach (IGuildUser user in users) {
				if (user.RoleIds.Any(roleId => roleId == propagatedRoleId)) {
					if (user.IsBot) {
						stats.InfectedBots++;
					} else if (UserIsStaff(user)) {
						stats.InfectedStaff++;
					} else {
						stats.InfectedOtherUsers++;
					}
				} else {
					
				}
			}
		}

		private bool UserIsStaff(IGuildUser user) {
			GuildPermissions perms = user.GuildPermissions;
			return
				perms.Administrator ||
				perms.BanMembers ||
				perms.KickMembers ||
				perms.ManageChannels ||
				perms.ManageEmojis ||
				perms.ManageGuild ||
				perms.ManageMessages ||
				perms.ManageNicknames ||
				perms.ManageRoles ||
				perms.ManageWebhooks;
		}
	}

	public class PropagationStats {
		public UserCount Clean { get; }
		public UserCount Infected { get; }

	}

	public class UserCount {
		public int Bots { get; set; }
		public int Staff { get; set; }
		public int OtherUsers { get; set; }
		public int Total => Bots + Staff + OtherUsers;
	}
}
