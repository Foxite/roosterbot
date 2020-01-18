namespace RoosterBot {
	/// <summary>
	/// A string, usually an emoji, that can be used as an <see cref="IEmote"/>.
	/// </summary>
	public class Emoji : IEmote {
		private readonly string m_Unicode;

		/// <summary>
		/// Construct a new Emoji.
		/// </summary>
		/// <param name="unicode"></param>
		public Emoji(string unicode) {
			m_Unicode = unicode;
		}

		/// <inheritdoc/>
		public override string ToString() => m_Unicode;
	}
}
