﻿using System.Collections.Generic;
using System.Linq;

namespace ScheduleComponent.DataTypes {
	public class TeacherInfo : IdentifierInfo {
		public bool     IsUnknown;
		public string   Abbreviation;
		public string   FullName;
		public string[] AltSpellings;
		public bool     NoLookup;
		public string   DiscordUser;

		public bool MatchName(string nameInput) {
			// We match the input by first checking if their full name starts with the input (case insensitive),
			//  and then by checking if the *input* starts with one of the alternative spellings (also case insensitive).

			// Check start of full name or exact match with abbreviation
			if (Abbreviation.ToLower() == nameInput ||
				FullName.ToLower().StartsWith(nameInput)) {
				return true;
			} else if (AltSpellings != null) {
				// Check alternative spellings
				foreach (string altSpelling in AltSpellings) {
					if (nameInput.StartsWith(altSpelling)) {
						return true;
					}
				}
			}
			return false;
		}

		public override string ScheduleField => "StaffMember";
		public override string ScheduleCode => Abbreviation;
		public override string DisplayText => FullName;

		public override bool Matches(ScheduleRecord record) {
			return record.StaffMember.Contains(this);
		}

		public override bool Equals(object obj) {
			// Auto-generated by Visual Studio 2017
			var info = obj as TeacherInfo;
			return info != null &&
				   //base.Equals(obj) && // produces compile error
				   this.Abbreviation == info.Abbreviation &&
				   this.FullName == info.FullName &&
				   EqualityComparer<string[]>.Default.Equals(this.AltSpellings, info.AltSpellings) &&
				   this.NoLookup == info.NoLookup &&
				   this.DiscordUser == info.DiscordUser;
		}

		public override int GetHashCode() {
			// Auto-generated by Visual Studio 2017
			var hashCode = 197285817;
			//hashCode = hashCode * -1521134295 + base.GetHashCode(); // produces compile error
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.Abbreviation);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.FullName);
			hashCode = hashCode * -1521134295 + EqualityComparer<string[]>.Default.GetHashCode(this.AltSpellings);
			hashCode = hashCode * -1521134295 + this.NoLookup.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(this.DiscordUser);
			return hashCode;
		}
	}
}
