using System;

namespace RoosterBot {
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class TypeDisplayAttribute : Attribute {
		public string TypeDisplayName { get; }

		public TypeDisplayAttribute(string typeDisplayName) {
			TypeDisplayName = typeDisplayName;
		}
	}
}
