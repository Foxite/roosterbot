using System;

namespace RoosterBot {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
	public sealed class HiddenFromListAttribute : Attribute {}
}
