using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using RoosterBot;

namespace ScheduleComponent.Services {
	public class TeacherNameService {
		private List<TeacherRecord> m_Records = new List<TeacherRecord>();

		/// <summary>
		/// Clears all records.
		/// </summary>
		public void Reset() {
			m_Records.Clear();
		}

		/// <summary>
		/// Loads a CSV with teacher abbreviations into memory.
		/// </summary>
		/// <param name="name">Should be the same as the property you're going to search from.</param>
		public async Task ReadAbbrCSV(string path) {
			Logger.Log(Discord.LogSeverity.Info, "TeacherNameService", $"Loading abbreviation CSV file {path}");

			using (StreamReader reader = File.OpenText(path)) {
				CsvReader csv = new CsvReader(reader);
				await csv.ReadAsync();
				csv.ReadHeader();

				List<TeacherRecord> currentRecords = new List<TeacherRecord>();

				while (await csv.ReadAsync()) {
					TeacherRecord record = new TeacherRecord() {
						Abbreviation = csv["Abbreviation"],
						FullName = csv["FullName"],
						NoLookup = bool.Parse(csv["NoLookup"]),
						DiscordUser = csv["DiscordUser"]
					};
					string altSpellingsString = csv["AltSpellings"];

					if (altSpellingsString != "") {
						record.AltSpellings = altSpellingsString.Split(',');
					};

					currentRecords.Add(record);
				}

				lock (m_Records) {
					m_Records.AddRange(currentRecords);
				}
			}
			Logger.Log(Discord.LogSeverity.Info, "TeacherNameService", $"Successfully loaded abbreviation CSV file {path}");
		}

		/// <summary>
		/// Case sensitive.
		/// </summary>
		public TeacherRecord GetRecordFromAbbr(string abbr) {
			foreach (TeacherRecord record in m_Records) {
				if (record.Abbreviation == abbr) {
					return record;
				}
			}
			return null;
		}

		/// <summary>
		/// Case sensitive.
		/// </summary>
		public TeacherRecord GetRecordFromFullName(string fullname) {
			foreach (TeacherRecord record in m_Records) {
				if (record.Abbreviation == fullname) {
					return record;
				}
			}
			return null;
		}

		public TeacherRecord[] GetRecordsFromAbbrs(string[] abbrs) {
			TeacherRecord[] records = new TeacherRecord[abbrs.Length];
			for (int i = 0; i < abbrs.Length; i++) {
				records[i] = GetRecordFromAbbr(abbrs[i]);
			}
			return records;
		}
		
		public TeacherRecord[] GetRecordsFromNameInput(string nameInput, bool skipNoLookup = true) {
			nameInput = nameInput.ToLower();

			// Get a list of all teachers matching the given input.
			// We match them by first checking if their full name starts with the input (case insensitive),
			//  and then by checking if the *input* starts with one of the alternative spellings (also case insensitive).
			List<TeacherRecord> records = new List<TeacherRecord>();

			foreach (TeacherRecord record in m_Records) {
				if (skipNoLookup && record.NoLookup)
					continue;

				// Check start of full name or exact match with abbreviation
				if (record.Abbreviation.ToLower() == nameInput ||
					record.FullName.ToLower().StartsWith(nameInput)) {
					records.Add(record);
				} else if (record.AltSpellings != null) {
					// Check alternative spellings
					foreach (string altSpelling in record.AltSpellings) {
						if (nameInput.StartsWith(altSpelling)) {
							records.Add(record);
						}
					}
				}
			}

			// If we end up with an empty list, return null, otherwise, return 
			if (records.Count == 0) {
				return null;
			} else {
				return records.ToArray();
			}
		}

		public string GetFullNameFromAbbr(string abbr) {
			return GetRecordFromAbbr(abbr)?.FullName;
		}

		public string[] GetFullNamesFromNameInput(string nameInput, bool skipNoLookup = true) {
			return GetRecordsFromNameInput(nameInput, skipNoLookup)?.Select(record => record.FullName).ToArray();
		}

		public string GetAbbrFromFullName(string fullname) {
			return GetRecordFromFullName(fullname)?.Abbreviation;
		}

		public string[] GetAbbrsFromNameInput(string nameInput, bool skipNoLookup = true) {
			return GetRecordsFromNameInput(nameInput, skipNoLookup)?.Select(record => record.Abbreviation).ToArray();
		}

		public IReadOnlyList<TeacherRecord> GetAllRecords() {
			return m_Records as IReadOnlyList<TeacherRecord>;
		}
	}

	public class TeacherRecord : ICloneable {
		public string	Abbreviation;
		public string	FullName;
		public string[] AltSpellings;
		public bool		NoLookup;
		public string	DiscordUser;

		public object Clone() {
			return new TeacherRecord() {
				Abbreviation = Abbreviation,
				FullName = FullName,
				AltSpellings = (string[]) AltSpellings.Clone(),
				NoLookup = NoLookup,
				DiscordUser = DiscordUser
			};
		}
	}
}
