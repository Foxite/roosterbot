using System.Threading.Tasks;

namespace RoosterBot {
	public sealed class UserIsModeratorAttribute : RoosterPreconditionAttribute {
		public override string Summary => "#UserIsModeratorAttribute_Summary";

		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context) {
			if (context.IsPrivate) {
				return ValueTaskUtil.FromResult(RoosterCheckResult.UnsuccessfulBuiltIn("#UserIsModeratorAttribute_PublicOnly"));
			} else if (context.User.IsChannelAdmin(context.Channel)) {
				return ValueTaskUtil.FromResult(RoosterCheckResult.Successful);
			} else {
				return ValueTaskUtil.FromResult(RoosterCheckResult.UnsuccessfulBuiltIn("#UserIsModeratorAttribute_CheckFailed"));
			}
		}
	}
}
