using System;

namespace RoosterBot {
	/// <summary>
	/// Indicates that a parameter is only allowed to have one value, and that it is there to make a command grammatically structured.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class GrammarParameterAttribute : Attribute { }
}
