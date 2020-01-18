using System;

namespace RoosterBot {
	/// <summary>
	/// A filter for a <see cref="Version"/>.
	/// </summary>
	public class VersionPredicate {
		/// <summary>
		/// The required <see cref="Version.Major"/>.
		/// </summary>
		public uint  Major { get; }

		/// <summary>
		/// The required <see cref="Version.Feature"/>.
		/// </summary>
		public uint? Feature { get; }

		/// <summary>
		/// The required <see cref="Version.Minor"/>.
		/// </summary>
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

		/// <summary>
		/// Test if a <see cref="Version"/> matches this predicate.
		/// </summary>
		public bool Matches(Version version) {
			return						 version.Major   == Major
				&& (!Feature.HasValue || version.Feature == Feature)
				&& (!Minor  .HasValue || version.Minor   == Minor);
		}
		
		/// <inheritdoc/>
		public override bool Equals(object? obj) {
			return obj is VersionPredicate predicate
				&& Major   == predicate.Major
				&& Feature == predicate.Feature
				&& Minor   == predicate.Minor;
		}

		/// <inheritdoc/>
		public override int GetHashCode() {
			var hashCode = 145219157;
			hashCode = hashCode * -1521134295 + Major.GetHashCode();
			hashCode = hashCode * -1521134295 + Feature.GetHashCode();
			hashCode = hashCode * -1521134295 + Minor.GetHashCode();
			return hashCode;
		}

		/// <inheritdoc/>
		public override string ToString() => $"{Major}.{Feature.ToString() ?? "x"}.{Minor.ToString() ?? "x"}";
	}
}
