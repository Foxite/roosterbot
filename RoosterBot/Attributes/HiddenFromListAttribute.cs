using System;

namespace RoosterBot.Attributes {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class HiddenFromListAttribute : Attribute {}
}
