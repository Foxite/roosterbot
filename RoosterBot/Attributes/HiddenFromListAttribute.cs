using System;

namespace RoosterBot {
	/// <summary>
	/// Indicates that a module, command, or parameter should be hidden from listings.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
	public sealed class HiddenFromListAttribute : Attribute {}
}
