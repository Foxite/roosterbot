using System;
using System.Threading.Tasks;

namespace RoosterBot {
	/// <summary>
	/// This precondition requires that the amount of item in an array is >= and <= a value. Both Min and Max are inclusive.
	/// Of course, it can only be used on parammeters that have an Array type; an exception will be thrown if the parameter is not an array.
	/// </summary>
	public sealed class CountAttribute : RoosterParameterCheckAttribute {
		public int Min { get; }
		public int Max { get; }

		public CountAttribute(int min, int max) {
			Min = min;
			Max = max;
		}

		protected override ValueTask<RoosterCheckResult> CheckAsync(object value, RoosterCommandContext context) {
			if (value.GetType().IsArray) {
				int length = ((Array) value).Length;
				if (length >= Min && length <= Max) {
					return new ValueTask<RoosterCheckResult>(RoosterCheckResult.FromSuccess());
				} else {
					return new ValueTask<RoosterCheckResult>(RoosterCheckResult.UnsuccessfulBuiltIn("Program_OnCommandExecuted_BadArgCount"));
				}
			} else {
				throw new InvalidOperationException("CountAttribute can only be used on array parameters.");
			}
		}
	}
}
