using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	/// <summary>
	/// This precondition requires that the amount of item in an array is >= and <= a value. Both Min and Max are inclusive.
	/// Of course, it can only be used on parammeters that have an Array type; an exception will be thrown if the parameter is not an array.
	/// </summary>
	public sealed class CountAttribute : RoosterParameterPreconditionAttribute {
		public int Min { get; }
		public int Max { get; }

		public CountAttribute(int min, int max) {
			Min = min;
			Max = max;
		}

		protected override Task<RoosterPreconditionResult> CheckPermissionsAsync(RoosterCommandContext context, ParameterInfo parameter, object value, IServiceProvider services) {
			if (parameter.Type.IsArray) {
				int length = ((Array) value).Length;
				if (length >= Min && length <= Max) {
					return Task.FromResult(RoosterPreconditionResult.FromSuccess());
				} else {
					return Task.FromResult(RoosterPreconditionResult.FromErrorBuiltin("Program_OnCommandExecuted_BadArgCount"));
				}
			} else {
				throw new InvalidOperationException("CountAttribute can only be used on array parameters.");
			}
		}
	}
}
