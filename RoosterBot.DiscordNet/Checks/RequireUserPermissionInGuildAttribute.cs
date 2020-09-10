using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	internal sealed class RequireUserPermissionInGuildAttribute : RoosterPreconditionAttribute {
		public override string Summary => "";

		public GuildPermission Permission { get; }
		public ulong GuildId { get; }

		public RequireUserPermissionInGuildAttribute(GuildPermission permission, ulong guildId) {
			Permission = permission;
			GuildId = guildId;
		}

		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext rcontext) {
			if (rcontext is DiscordCommandContext context) {
				SocketGuildUser? guildUser = context.Client.GetGuild(GuildId).GetUser(context.User.Id);
				if (guildUser != null && guildUser.GuildPermissions.Has(Permission)) {
					return ValueTaskUtil.FromResult(RoosterCheckResult.Successful);
				} else {
					return ValueTaskUtil.FromResult(RoosterCheckResult.Unsuccessful("#RequireUserPermissionInGuild_CheckFailed", DiscordNetComponent.Instance));
				}
			} else {
				return ValueTaskUtil.FromResult(RoosterCheckResult.Unsuccessful("If you see this, you may slap the developer.", DiscordNetComponent.Instance));
			}
		}
	}
}