using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	public class RangeAttribute : ParameterPreconditionAttribute {
		public int Min { get; }
		public int Max { get; }

		public RangeAttribute(int min, int max) {
			Min = min;
			Max = max;
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services) {
			bool ret = (dynamic) value >= Min && (dynamic) value <= Max;
			if (ret) {
				return Task.FromResult(PreconditionResult.FromSuccess());
			} else {
				return Task.FromResult(PreconditionResult.FromError(Resources.RangeAttribute_CheckFailed));
			}
		}
	}
}
