using System;

namespace RoosterBot.Attributes {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
	public sealed class HiddenFromListAttribute : Attribute {}
}
