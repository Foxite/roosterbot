using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoosterBot.Schedule {
	[JsonObject(MemberSerialization.OptOut)]
	public class TeacherInfo : IdentifierInfo {
		public override string ScheduleCode { get; }
		public override string DisplayText { get; }
		public bool			   IsUnknown { get; }
		public bool			   NoLookup { get; }
		public string?		   DiscordUser { get; }
		public IReadOnlyList<string> AltSpellings { get; }

		public TeacherInfo(string scheduleCode, string displayText, bool isUnknown, bool noLookup, string? discordUser, IReadOnlyList<string> altSpellings) {
			IsUnknown = isUnknown;
			ScheduleCode = scheduleCode;
			DisplayText = displayText;
			NoLookup = noLookup;
			DiscordUser = discordUser;
			AltSpellings = altSpellings;
		}

		public float MatchName(string nameInput) {
			// We match the input by first checking if their full name starts with the input (case insensitive),
			//  and then by checking if the *input* starts with one of the alternative spellings (also case insensitive).

			// Check start of full name or exact match with abbreviation
			if (ScheduleCode.ToLower() == nameInput ||
				DisplayText.ToLower().StartsWith(nameInput)) {
				return (float) nameInput.Length / DisplayText.Length;
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
