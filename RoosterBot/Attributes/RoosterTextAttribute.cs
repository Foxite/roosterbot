using System;

namespace RoosterBot {
	public abstract class RoosterTextAttribute : Attribute {
		public string Text { get; set; }

		protected RoosterTextAttribute(string text) {
			Text = text;
		}
	}
}
