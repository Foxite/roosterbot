using System;
using System.Threading.Tasks;

namespace RoosterBot {
	/// <summary>
	/// This precondition requires that the amount of item in an array is &gt;= and &lt;= a value. Both Min and Max are inclusive.
	/// Of course, it can only be used on parammeters that have an Array type; an exception will be thrown if the parameter is not an array.
	/// </summary>
	public sealed class CountAttribute : RoosterParameterCheckAttribute {
		/// <summary>
		/// The minimum amount of items in the array.
		/// </summary>
		public int Min { get; }

		/// <summary>
		/// The maximum amount of items in the array.
		/// </summary>
		public int Max { get; }

		/// <summary>
		/// This precondition requires that the amount of item in an array is &gt;= and &lt;= a value. Both Min and Max are inclusive.
		/// Of course, it can only be used on parammeters that have an Array type; an exception will be thrown if the parameter is not an array.
		/// </summary>
		/// <param name="min">The minimum amount of items in the array.</param>
		/// <param name="max">The maximum amount of items in the array.</param>
		public CountAttribute(int min, int max) {
			Min = min;
			Max = max;
		}

		/// 
		protected override ValueTask<RoosterCheckResult> CheckAsync(object value, RoosterCommandContext context) {
			if (value.GetType().IsArray) {
				int length = ((Array) value).Length;
				if (length >= Min && length <= Max) {
					return new ValueTask<RoosterCheckResult>(RoosterCheckResult.Successful);
				} else {
					return new ValueTask<RoosterCheckResult>(RoosterCheckResult.UnsuccessfulBuiltIn("Program_OnCommandExecuted_BadArgCount"));
				}
			} else {
				throw new InvalidOperationException("CountAttribute can only be used on array parameters.");
			}
		}
	}
}
