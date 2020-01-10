using System;
using System.Linq;

namespace RoosterBot {
	public static class Util {
		public static readonly Random RNG = new Random();

		public static bool CompareNumeric(object left, object right) {
			Type[] numerics = new[] {
				typeof(byte),
				typeof(short),
				typeof(int),
				typeof(long),
				typeof(sbyte),
				typeof(ushort),
				typeof(uint),
				typeof(ulong),
				typeof(float),
				typeof(double),
				typeof(decimal),
			};
			if (numerics.Contains(left.GetType()) && numerics.Contains(right.GetType())) {
				return ((IConvertible) left).ToDecimal(null).Equals(((IConvertible) left).ToDecimal(null));
			} else {
				return left.Equals(right);
			}
		}
	}
}
