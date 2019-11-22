using System.Threading.Tasks;

namespace RoosterBot {
	/// <summary>
	/// This precondition will compare the value of the parameter with the Min and Max values using the >= and <= operators defined in the type of the parameter.
	/// </summary>
	public sealed class RangeAttribute : RoosterParameterCheckAttribute {
		public object Min { get; }
		public object Max { get; }

		public RangeAttribute(object min, object max) {
			Min = min;
			Max = max;
		}

		protected override ValueTask<RoosterCheckResult> CheckAsync(object value, RoosterCommandContext context) {
			bool ret = (dynamic) value >= Min && (dynamic) value <= Max;
			if (ret) {
				return new ValueTask<RoosterCheckResult>(RoosterCheckResult.FromSuccess());
			} else {
				return new ValueTask<RoosterCheckResult>(RoosterCheckResult.FromErrorBuiltin("#RangeAttribute_CheckFailed"));
			}
		}
	}
}
