using System;

namespace RoosterBot {
	/// <summary>
	/// Causes a module's commands to be localized, and the localized commands to be available from every language.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class GlobalLocalizationsAttribute : Attribute { }
}
