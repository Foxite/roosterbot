using System;

namespace RoosterBot {
	/// <summary>
	/// Specify a display name for your command's parameter type.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public sealed class TypeDisplayAttribute : RoosterTextAttribute {
		/// 
		public TypeDisplayAttribute(string typeDisplayName) : base(typeDisplayName) { }
	}
}
