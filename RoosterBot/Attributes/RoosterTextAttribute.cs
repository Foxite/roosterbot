using System;

namespace RoosterBot {
	/// <summary>
	/// The abstract base class for all attributes within RoosterBot that consist of only a single string.
	/// </summary>
	public abstract class RoosterTextAttribute : Attribute {
		/// <summary>
		/// The text for this <see cref="RoosterTextAttribute"/>.
		/// </summary>
		public string Text { get; set; }

		///
		protected RoosterTextAttribute(string text) {
			Text = text;
		}
	}
}
