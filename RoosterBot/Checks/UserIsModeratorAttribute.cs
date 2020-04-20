using System.Threading.Tasks;

namespace RoosterBot {
	/// <summary>
	/// Require that the user has <see cref="IUser.IsChannelAdmin(IChannel)"/> for the context's channel.
	/// </summary>
	public sealed class UserIsModeratorAttribute : RoosterCheckAttribute {
		/// 
		public override string Summary => "#UserIsModeratorAttribute_Summary";

		/// 
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
