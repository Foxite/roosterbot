using Foxite.Common;

namespace RoosterBot.Weather {
	public class RegionInfo {
		public int Id { get; }
		public string Name { get; }

		private readonly string m_NormalName;

		public RegionInfo(int id, string name) {
			Id = id;
			Name = name;

			m_NormalName = name.RemoveDiacritics().ToLower();
		}

		public bool Match(string input) {
			return m_NormalName.StartsWith(input);
		}

		public override string ToString() => Name;
	}
}