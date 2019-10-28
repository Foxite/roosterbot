using System;
using System.Collections.Generic;

namespace RoosterBot {
	public sealed class LocalizedModuleAttribute : Attribute {
		public IReadOnlyList<string> Locales { get; }

		public LocalizedModuleAttribute(params string[] locales) {
			Locales = locales;
		}
	}
}
