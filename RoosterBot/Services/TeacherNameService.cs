using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;

namespace RoosterBot.Services {
	public class TeacherNameService {
		private ConcurrentBag<TeacherAbbrRecord> m_Records = new ConcurrentBag<TeacherAbbrRecord>();

		/// <summary>
		/// Loads a CSV with teacher abbreviations into memory.
		/// </summary>
		/// <param name="name">Should be the same as the property you're going to search from.</param>
		public async Task ReadAbbrCSV(string path) {
			Logger.Log(Discord.LogSeverity.Info, "ScheduleService", $"Loading abbreviation CSV file {path}");

			using (StreamReader reader = File.OpenText(path)) {
				CsvReader csv = new CsvReader(reader);
				await csv.ReadAsync();
				csv.ReadHeader();
				while (await csv.ReadAsync()) {
					TeacherAbbrRecord record = new TeacherAbbrRecord() {
						Abbreviation = csv["Abbreviation"],
						FullName = csv["FullName"],
						NoLookup = bool.Parse(csv["NoLookup"])
					};
					string altSpellingsString = csv["AltSpellings"];

					if (altSpellingsString != "") {
						record.AltSpellings = altSpellingsString.Split(',');
					};

					m_Records.Add(record);

				}
			}
			Logger.Log(Discord.LogSeverity.Info, "ScheduleService", $"Successfully loaded abbreviation CSV file {path}");
		}

		/// <summary>
		/// Not case sensitive.
		/// </summary>
		public TeacherAbbrRecord GetRecordFromAbbr(string abbr) {
			abbr = abbr.ToLower();
			foreach (TeacherAbbrRecord record in m_Records) {
				if (record.Abbreviation.ToLower() == abbr) {
					return record;
				}
			}
			return null;
		}

		/// <summary>
		/// Case sensitive.
		/// </summary>
		public TeacherAbbrRecord GetRecordFromAbbrCS(string abbr) {
			foreach (TeacherAbbrRecord record in m_Records) {
				if (record.Abbreviation == abbr) {
					return record;
				}
			}
			return null;
		}

		public string[] GetAbbrFromNameInput(string nameInput) {
			nameInput = nameInput.ToLower();

			// Get a list of all teachers matching the given input.
			// We match them by first checking if their full name starts with the input (case insensitive),
			//  and then by checking if the *input* starts with one of the alternative spellings (also case insensitive).
			List<string> names = new List<string>();

			foreach (TeacherAbbrRecord record in m_Records) {
				// Check start of full name or exact match with abbreviation
				if (record.Abbreviation.ToLower() == nameInput ||
					record.FullName.ToLower().StartsWith(nameInput)) {
					names.Add(record.Abbreviation);
				} else if (record.AltSpellings != null) {
					// Check alternative spellings
					foreach (string altSpelling in record.AltSpellings) {
						if (nameInput.StartsWith(altSpelling)) {
							names.Add(record.Abbreviation);
						}
					}
				}
			}

			// If we end up with an empty list, return null, otherwise, return 
			if (names.Count == 0) {
				return null;
			} else {
				return names.ToArray();
			}
		}

		public string GetFullNameFromAbbr(string abbr) {
			return GetRecordFromAbbrCS(abbr)?.FullName;
		}
	}

	public class TeacherAbbrRecord : ICloneable {
		public string	Abbreviation;
		public string	FullName;
		public string[] AltSpellings;
		public bool		NoLookup;

		public object Clone() {
			return new TeacherAbbrRecord() {
				Abbreviation = Abbreviation,
				FullName = FullName,
				AltSpellings = (string[]) AltSpellings.Clone(),
				NoLookup = NoLookup
			};
		}
	}
}
