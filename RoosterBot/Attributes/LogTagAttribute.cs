using System;

namespace RoosterBot.Attributes {
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class LogTagAttribute : Attribute {
		public string LogTag { get; }

		public LogTagAttribute(string logTag) {
			LogTag = logTag;
		}
	}
}
