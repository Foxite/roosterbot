using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	/// <summary>
	/// This precondition will compare the value of the parameter with the Min and Max values using the >= and <= operators defined in the type of the parameter.
	/// </summary>
	public sealed class RangeAttribute : RoosterParameterPreconditionAttribute {
		public object Min { get; }
		public object Max { get; }

		public RangeAttribute(object min, object max) {
			Min = min;
			Max = max;
		}

		protected override Task<RoosterPreconditionResult> CheckPermissionsAsync(RoosterCommandContext context, ParameterInfo parameter, object value, IServiceProvider services) {
			bool ret = (dynamic) value >= Min && (dynamic) value <= Max;
			if (ret) {
				return Task.FromResult(RoosterPreconditionResult.FromSuccess());
			} else {
				return Task.FromResult(RoosterPreconditionResult.FromErrorBuiltin("#RangeAttribute_CheckFailed"));
			}
		}
	}
}
