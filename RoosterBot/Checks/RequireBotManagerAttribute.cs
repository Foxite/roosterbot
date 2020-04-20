using System.Threading.Tasks;

namespace RoosterBot {
	/// <summary>
	/// Require that the user has <see cref="IUser.IsBotAdmin"/> <see langword="true"/>.
	/// </summary>
	public sealed class RequireBotManagerAttribute : RoosterCheckAttribute {
		/// 
		public override string Summary => "#RequireBotManagerAttribute_Summary";

		/// 
		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context) {
			return ValueTaskUtil.FromResult(context.User.IsBotAdmin ? RoosterCheckResult.Successful : RoosterCheckResult.UnsuccessfulBuiltIn("#RequireBotManagerAttribute_CheckFailed"));
		}
	}
}
