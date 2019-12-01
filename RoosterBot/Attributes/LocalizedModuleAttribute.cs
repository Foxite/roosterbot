using System;
using System.Collections.Generic;

namespace RoosterBot {
	[Obsolete("Localize your component by overriding " + nameof(Component.SupportedCultures))]
	public sealed class LocalizedModuleAttribute : Attribute {
		public IReadOnlyList<string> Locales { get; }

		public LocalizedModuleAttribute(params string[] locales) {
			Locales = locales;
		}
	}
}
