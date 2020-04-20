using System.Linq;
using System.Threading.Tasks;
using Discord;
using RoosterBot.DiscordNet;

namespace RoosterBot.GLU.Discord {
	public class RequireTeacherAttribute : RoosterCheckAttribute {
		public override string Summary => "Alleen leraren mogen deze command gebruiken.";
		
		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext ctx) {
			if (ctx.User is DiscordUser user && user.DiscordEntity is IGuildUser igu && igu.RoleIds.Contains(688839445981560883ul)) {
				return ValueTaskUtil.FromResult(RoosterCheckResult.Successful);
			} else {
				return ValueTaskUtil.FromResult(RoosterCheckResult.Unsuccessful(Summary, GLUDiscordComponent.Instance));
			}
		}
	}
}
