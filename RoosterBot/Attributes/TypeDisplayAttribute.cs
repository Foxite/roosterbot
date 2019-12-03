using System;

namespace RoosterBot {
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class TypeDisplayAttribute : RoosterTextAttribute {
		public TypeDisplayAttribute(string typeDisplayName) : base(typeDisplayName) { }
	}
}
