using System;
using System.Collections.Generic;
using System.Linq;

namespace RoosterBot.Schedule {
	public class TeacherInfo : IdentifierInfo {
		public bool     IsUnknown { get; }
		public string   Abbreviation { get; }
		public string   FullName { get; }
		public bool     NoLookup { get; }
		public string?  DiscordUser { get; }
		public IReadOnlyList<string> AltSpellings { get; }

		public TeacherInfo(bool isUnknown, string abbreviation, string fullName, bool noLookup, string? discordUser, IReadOnlyList<string> altSpellings) {
			IsUnknown = isUnknown;
			Abbreviation = abbreviation;
			FullName = fullName;
			NoLookup = noLookup;
			DiscordUser = discordUser;
			AltSpellings = altSpellings;
		}

		public float MatchName(string nameInput) {
			// We match the input by first checking if their full name starts with the input (case insensitive),
			//  and then by checking if the *input* starts with one of the alternative spellings (also case insensitive).

			// Check start of full name or exact match with abbreviation
			if (Abbreviation.ToLower() == nameInput ||
				FullName.ToLower().StartsWith(nameInput)) {
				return (float) nameInput.Length / FullName.Length;
			} else if (AltSpellings != null) {
				// Check alternative spellings
				foreach (string altSpelling in AltSpellings) {
					if (nameInput.StartsWith(altSpelling)) {
						return (float) nameInput.Length / altSpelling.Length;
					}
				}
			}
			return 0;
		}

		public override string ScheduleCode => Abbreviation;
		public override string DisplayText => FullName;

		public override bool Matches(ScheduleRecord record) {
			return record.StaffMember.Contains(this);
		}

		public override bool Equals(object? obj) {
			return obj is TeacherInfo info &&
				base.Equals(obj) &&
				IsUnknown == info.IsUnknown &&
				NoLookup == info.NoLookup &&
				EqualityComparer<IReadOnlyList<string>>.Default.Equals(AltSpellings, info.AltSpellings);
		}

		public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), IsUnknown, NoLookup, AltSpellings);
	}
}
