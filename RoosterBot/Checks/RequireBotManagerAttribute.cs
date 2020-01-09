using System.Threading.Tasks;

namespace RoosterBot {
	public sealed class RequireBotManagerAttribute : RoosterPreconditionAttribute {
		public override string Summary => "#RequireBotManagerAttribute_Summary";

		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context) {
			return ValueTaskUtil.FromResult(context.User.IsBotAdmin ? RoosterCheckResult.Successful : RoosterCheckResult.UnsuccessfulBuiltIn("#RequireBotManagerAttribute_CheckFailed"));
		}
	}
}
