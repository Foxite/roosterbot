using System;

namespace RoosterBot {
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class TypeDisplayAttribute : RoosterTextAttribute {
		public string TypeDisplayName => Text;

		public TypeDisplayAttribute(string typeDisplayName) : base(typeDisplayName) { }
	}
}
