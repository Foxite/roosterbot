using System;

namespace RoosterBot {
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class LogTagAttribute : Attribute {
		public string LogTag { get; }

		public LogTagAttribute(string logTag) {
			LogTag = logTag;
		}
	}
}
