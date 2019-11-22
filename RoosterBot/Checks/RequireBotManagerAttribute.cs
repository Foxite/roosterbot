using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public sealed class RequireBotManagerAttribute : RoosterPreconditionAttribute {
		public override string Summary => "#RequireBotManagerAttribute_Summary";

		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context) {
			if (context.User.Id == context.ServiceProvider.GetService<ConfigService>().BotOwner.Id) {
				return new ValueTask<RoosterCheckResult>(RoosterCheckResult.Successful);
			} else {
				return new ValueTask<RoosterCheckResult>(RoosterCheckResult.UnsuccessfulBuiltIn("#RequireBotManagerAttribute_CheckFailed"));
			}
		}
	}
}
