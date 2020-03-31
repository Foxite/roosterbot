using System.Threading.Tasks;

namespace RoosterBot {
	/// <summary>
	/// Implicit on all modules added through <see cref="RoosterCommandService"/>.
	/// </summary>
	internal class ModuleEnabledInChannelAttribute : RoosterPreconditionAttribute {
		public override string Summary => "";

		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context) {
			if (context.ChannelConfig.DisabledModules.Contains(context.Command.Module.Type.Name)) {
				return ValueTaskUtil.FromResult(RoosterCheckResult.UnsuccessfulBuiltIn("#ModuleEnabledInChannelAttribute_Unsuccessful"));
			} else {
				return ValueTaskUtil.FromResult(RoosterCheckResult.Successful);
			}
		}
	}
}
