namespace RoosterBot {
	public class Emoji : IEmote {
		private string m_Unicode;

		public Emoji(string unicode) {
			m_Unicode = unicode;
		}

		public override string ToString() => m_Unicode;
	}
}
