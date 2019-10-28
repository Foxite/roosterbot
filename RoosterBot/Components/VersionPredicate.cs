using System;

namespace RoosterBot {
	public class VersionPredicate {
		public uint  Major { get; }
		public uint? Feature { get; }
		public uint? Minor { get; }

		/// <summary>
		/// If you want to make feature null, you must also make minor null.
		/// It does not make sense to match against "1.x.4", only "1.4.x" or "1.x.x".
		/// </summary>
		public VersionPredicate(uint major, uint? feature, uint? minor) {
			if (feature == null && minor != null) {
				throw new ArgumentException("You may not have a null minor version and a non-null feature version.");
			}

			Major = major;
			Feature = feature;
			Minor = minor;
		}

		public bool Matches(Version version) {
			return						 version.Major   == Major
				&& (!Feature.HasValue || version.Feature == Feature)
				&& (!Minor  .HasValue || version.Minor   == Minor);
		}

		public override bool Equals(object obj) {
			return obj is VersionPredicate predicate
				&& Major   == predicate.Major
				&& Feature == predicate.Feature
				&& Minor   == predicate.Minor;
		}

		public override int GetHashCode() {
			var hashCode = 145219157;
			hashCode = hashCode * -1521134295 + Major.GetHashCode();
			hashCode = hashCode * -1521134295 + Feature.GetHashCode();
			hashCode = hashCode * -1521134295 + Minor.GetHashCode();
			return hashCode;
		}

		public override string ToString() => $"{Major}.{Feature.ToString() ?? "x"}.{Minor.ToString() ?? "x"}";
	}
}
