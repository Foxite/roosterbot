﻿using System;

namespace RoosterBot {
	public class Version : IEquatable<Version>, IComparable<Version> {
		/// <summary>
		/// Increments every time compatibility with old versions is broken (ie, something is removed)
		/// </summary>
		public uint Major { get; }

		/// <summary>
		/// Increments every time a new feature is added.
		/// </summary>
		public uint Feature { get; }

		/// <summary>
		/// Increments every time neither Major or Feature increment.
		/// </summary>
		public uint Minor { get; }

		public Version(uint major, uint feature, uint minor) {
			Major = major;
			Feature = feature;
			Minor = minor;
		}

		public override bool Equals(object obj) => Equals(obj as Version);

		public bool Equals(Version other) {
			return other != null
				&& Major   == other.Major
				&& Feature == other.Feature
				&& Minor   == other.Minor;
		}

		public override string ToString() => $"{Major}.{Feature}.{Minor}";

		public override int GetHashCode() {
			// Generated by Visual Studio 2019
			var hashCode = 145219157;
			hashCode = hashCode * -1521134295 + Major.GetHashCode();
			hashCode = hashCode * -1521134295 + Feature.GetHashCode();
			hashCode = hashCode * -1521134295 + Minor.GetHashCode();
			return hashCode;
		}
		
		/// <summary>
		/// Causes Version objects to be sorted oldest first.
		/// </summary>
		public int CompareTo(Version other) {
			if (this == other) {
				return 0;
			} else if (this > other) {
				return 1;
			} else {
				return -1;
			}
		}

		public static bool operator ==(Version left, Version right) {
			return left.Major   == right.Major
				&& left.Feature == right.Feature
				&& left.Minor   == right.Minor;
		}

		public static bool operator !=(Version left, Version right) {
			return !(left == right);
		}

		public static bool operator >(Version left, Version right) {
			if (left.Major > right.Major) {
				return true;
			} else if (left.Major < right.Major) {
				return false;
			}

			if (left.Feature > right.Feature) {
				return true;
			} else if (left.Feature < right.Feature) {
				return false;
			}

			if (left.Minor > right.Minor) {
				return true;
			} else if (left.Minor < right.Minor) {
				return false;
			}

			return false;
		}

		public static bool operator <(Version left, Version right) {
			if (left.Major < right.Major) {
				return true;
			} else if (left.Major > right.Major) {
				return false;
			}

			if (left.Feature < right.Feature) {
				return true;
			} else if (left.Feature > right.Feature) {
				return false;
			}

			if (left.Minor < right.Minor) {
				return true;
			} else if (left.Minor > right.Minor) {
				return false;
			}
			return false;
		}

		public static bool operator >=(Version left, Version right) {
			return !(left < right);
		}

		public static bool operator <=(Version left, Version right) {
			return !(left > right);
		}
	}
}
