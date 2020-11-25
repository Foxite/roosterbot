using System;
using System.Collections.Generic;
using System.Linq;

namespace RoosterBot.PublicTransit {
	public class StationInfo : IEquatable<StationInfo> {
		public string Code { get; }
		public string? ShortName { get; }
		public string? MidName { get; }
		public string? LongName { get; }
		public float Latitude { get; }
		public float Longitude { get; }
		public IReadOnlyList<string> Synonyms { get; }

		public string DisplayName => LongName ?? MidName ?? ShortName ?? Code ?? "If you see this, please contact the bot owner for a free mars bar.";

		private IEnumerable<string> Names => Synonyms.Concat(new[] { ShortName, MidName, LongName }.WhereNotNull());

		public StationInfo() => throw new NotImplementedException();

		/// <summary>
		/// Lower return value means better score.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public int Match(string input) {
			int score = PublicTransitUtil.Levenshtein(input, Names.First().ToLower());
			foreach (string name in Names.Skip(1).Select(name => name.ToLower())) {
				score = Math.Min(score, PublicTransitUtil.Levenshtein(input, name));
			}
			return score;
		}

		public override string ToString() => DisplayName;

		public bool Equals(StationInfo? other) {
			return other != null
				&& other.Code == Code;
		}
	}

	public class StationMatchInfo : IComparable<StationMatchInfo> {
		public StationInfo Station { get; }
		public float Score { get; }

		public StationMatchInfo(StationInfo station, float score) {
			Station = station;
			Score = score;
		}

		public int CompareTo(StationMatchInfo? other) {
			return other is null ? -1 : Math.Sign(Score - other.Score);
		}

		public override string ToString() => Station.ToString() + "@" + Score;

		public override bool Equals(object? obj) {
			return obj is StationMatchInfo info
				&& Station.Equals(info.Station)
				&& Score == info.Score;
		}

		public override int GetHashCode() => HashCode.Combine(Station, Score);

		public static bool operator ==(StationMatchInfo left, StationMatchInfo right) {
			if (left is null) {
				return right is null;
			}

			return left.Equals(right);
		}

		public static bool operator !=(StationMatchInfo left, StationMatchInfo right) {
			return !(left == right);
		}

		public static bool operator <(StationMatchInfo left, StationMatchInfo right) {
			return left.Score < right.Score;
		}

		public static bool operator <=(StationMatchInfo left, StationMatchInfo right) {
			return left.Score <= right.Score;
		}

		public static bool operator >(StationMatchInfo left, StationMatchInfo right) {
			return left.Score > right.Score;
		}

		public static bool operator >=(StationMatchInfo left, StationMatchInfo right) {
			return left.Score >= right.Score;
		}
	}
}
