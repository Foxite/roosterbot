using System.Collections.Generic;
using System.Linq;
using Foxite.Common;

namespace RoosterBot.Weather {
	public class CityInfo {
		public int CityId { get; }

		public string Name { get; }
		public IReadOnlyList<string> Aliases { get; }
		public RegionInfo Region { get; }

		private readonly string m_NormalName;

		public CityInfo(int cityId, string name, RegionInfo region, params string[] aliases) {
			CityId = cityId;
			Name = name;
			Aliases = aliases;
			Region = region;

			m_NormalName = name.RemoveDiacritics().ToLower();
		}

		public bool Match(string input) {
			string[] split = input.Split(',');
			if (split.Length == 1) {
				return MatchName(input);
			} else {
				return
					Region.Match(split[^1].Trim()) &&
					MatchName(string.Join(" ", split.Take(split.Length - 1).Select(str => str.Trim())));
			}
		}

		private bool MatchName(string input) {
			// We could change this to a fuzzy matching algorithm, but is it necessary?
			// It certainly is useful for towns with stupidly long names (Westerhaar-Vriezenveensewijk)
			// We could do something like m_NormalName.StartsWith(input) but what if there's a town with a shorter name that also matches this predicate? That one should get chosen.
			// If we do this I recommend returning the score as (input.Length / m_NormalName.Length) if m_NormalName.StartsWith(input), otherwise it will always be 0.
			return m_NormalName == input || Aliases.Any(alias => alias == input);
		}

		public override string ToString() => Name;
	}
}