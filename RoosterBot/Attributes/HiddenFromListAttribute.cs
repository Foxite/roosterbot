using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RoosterBot {
	/// <summary>
	/// Indicates that a module, command, or parameter should be hidden from listings, optionally except for specified cultures.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
	public sealed class HiddenFromListAttribute : Attribute {
		/// <summary>
		/// The cultures that this module or command is not hidden in.
		/// </summary>
		public IReadOnlyList<CultureInfo> VisibleInCultures { get; }

		/// <param name="cultures">The cultures that this module or command is not hidden in.</param>
		public HiddenFromListAttribute(params string[] cultures) {
			VisibleInCultures = cultures.ListSelect(name => CultureInfo.GetCultureInfo(name));
		}
	}
}
