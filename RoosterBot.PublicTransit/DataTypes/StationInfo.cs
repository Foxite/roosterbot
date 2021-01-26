using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RoosterBot.PublicTransit {
	public class StationInfo : IEquatable<StationInfo> {
		public string Code { get; }
		public string Name { get; }
		public float Latitude { get; }
		public float Longitude { get; }
		public IReadOnlyList<string> Synonyms { get; }

		public StationInfo(XElement xmlStation) {
			Code = xmlStation.Element(XName.Get("Code"))!.Value;
			IEnumerable<XElement> names =
				from name in xmlStation.Element(XName.Get("Namen"))!.Elements()
				select name;

			string? getXmlName(string identifier) {
				return names.SingleOrDefault(element => element.Name == identifier)?.Value;
			}

			Name = (getXmlName("Lang") ?? getXmlName("Middel") ?? getXmlName("Kort"))!;

			XElement synos = xmlStation.Element(XName.Get("Synoniemen"))!;
			if (synos == null) {
				Synonyms = new List<string>().AsReadOnly();
			} else {
				Synonyms = (from syno in synos.Elements()
							select syno.Value).ToList().AsReadOnly();
			}
		}

		/// <summary>
		/// Lower return value means better score.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public int Match(string input) {
			return PublicTransitUtil.Levenshtein(input, Name.ToLower());
		}

		public override string ToString() => Name;

		public override bool Equals(object? obj) => Equals(obj as StationInfo);
		public bool Equals(StationInfo? other) {
			return other != null
				&& other.Code == Code;
		}

		public override int GetHashCode() => HashCode.Combine(Code);

		public static bool operator ==(StationInfo? left, StationInfo? right) {
			return (left is null && right is null) || (left is null != right is null && left!.Equals(right!));
		}

		public static bool operator !=(StationInfo? left, StationInfo? right) {
			return !(left == right);
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
