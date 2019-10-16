using Discord;
using Newtonsoft.Json.Linq;
using RoosterBot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MiscStuffComponent.Services {
	public class PropagationService {
		private readonly Dictionary<ulong, ulong> m_PropagatedRoles;

		public PropagationService(string configPath) {
			m_PropagatedRoles = new Dictionary<ulong, ulong>();

			JObject jsonRoles = JObject.Parse(File.ReadAllText(Path.Combine(configPath, "Propagation.json")));

			m_PropagatedRoles = new Dictionary<ulong, ulong>();
			
			foreach (KeyValuePair<string, JToken> item in jsonRoles["roles"].ToObject<JObject>()) {
				m_PropagatedRoles.Add(ulong.Parse(item.Key), item.Value.ToObject<ulong>());
			}
		}

		public ulong? GetPropagatedRoleId(IGuild guild) {
			if (m_PropagatedRoles.TryGetValue(guild.Id, out ulong roleId)) {
				return roleId;
			} else {
				return null;
			}
		}

		public async Task<PropagationStats> GetPropagationStats(IGuild guild) {
			ulong? propagatedRoleId = GetPropagatedRoleId(guild);
			if (propagatedRoleId.HasValue) {
				IReadOnlyCollection<IGuildUser> users = await guild.GetUsersAsync();

				var stats = new PropagationStats();

				foreach (IGuildUser user in users) {
					UserCount count = user.RoleIds.Any(roleId => roleId == propagatedRoleId) ? stats.Infected : stats.Clean;

					if (user.IsBot) {
						count.Bots++;
					} else if (UserIsStaff(user)) {
						count.Staff++;
					} else if (user.HasRole(278587815271464970)) {
						count.Developers++;
					} else if (user.HasRole(278587791141765121)) {
						count.Artists++;
					} else if (user.HasRole(278587837551607809)) {
						count.Teachers++;
					} else {
						count.OtherUsers++;
					}
				}

				return stats;
			} else {
				throw new ArgumentException("Guild " + guild.Name + " does not have a propagating role.");
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
		public PropagationStats() {
			Clean = new UserCount();
			Infected = new UserCount();
		}

		public UserCount Clean { get; }
		public UserCount Infected { get; }

		public string Present() {
			string ret = ":blue_heart: " + Clean.Present("clean");
			ret += "\n\n:biohazard: " + Infected.Present("infected");

			int infectedPercent = (100 * Infected.Total) / (Clean.Total + Infected.Total);
			// Round to 5% accuracy
			infectedPercent = (int) (Math.Round(infectedPercent / 5.0f) * 5);
			ret += $"\n\nApproximately {infectedPercent}% of users are infected.";

			return ret;
		}
	}

	public class UserCount {
		public int Bots { get; set; }
		public int Staff { get; set; }
		public int Teachers { get; set; }
		public int Developers { get; set; }
		public int Artists { get; set; }
		public int OtherUsers { get; set; }
		public int Total => Bots + Staff + Teachers + Developers + Artists + OtherUsers;

		internal string Present(string label) {
			string ret = $"{Total.ToString()} {label} members\n";
			ret += $"- {Staff.ToString()} staff\n";
			ret += $"- {Teachers.ToString()} teachers\n";
			ret += $"- {Artists.ToString()} artists\n";
			ret += $"- {Developers.ToString()} developers\n";
			ret += $"- {Bots.ToString()} bots\n";
			ret += $"- {OtherUsers.ToString()} other users";
			return ret;
		}
	}
}
