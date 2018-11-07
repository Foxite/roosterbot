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
			Logger.Log(Discord.LogSeverity.Info, "TeacherNameService", $"Loading abbreviation CSV file {Path.GetFileName(path)}");

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
			Logger.Log(Discord.LogSeverity.Info, "TeacherNameService", $"Successfully loaded abbreviation CSV file {Path.GetFileName(path)}");
		}

		/// <summary>
		/// Case sensitive.
		/// </summary>
		public TeacherRecord GetRecordFromAbbr(string abbr) {
			return m_Records.Find(record => record.Abbreviation == abbr);
		}

		public TeacherRecord[] GetRecordsFromAbbrs(string[] abbrs) {
			List<TeacherRecord> records = new List<TeacherRecord>();
			for (int i = 0; i < abbrs.Length; i++) {
				TeacherRecord record = GetRecordFromAbbr(abbrs[i]);
				if (record != null) {
					records.Add(record);
				}
			}
			return records.ToArray();
		}
		
		public TeacherRecord[] GetRecordsFromNameInput(string nameInput, bool skipNoLookup = true) {
			nameInput = nameInput.ToLower();

			List<TeacherRecord> records = new List<TeacherRecord>();

			foreach (TeacherRecord record in m_Records) {
				if (skipNoLookup && record.NoLookup)
					continue;

				if (record.MatchName(nameInput)) {
					records.Add(record);
				}
			}

			// If we end up with an empty list, return null, otherwise, return  the list
			if (records.Count == 0) {
				return null;
			} else {
				return records.ToArray();
			}
		}
		
		public string[] GetAbbrsFromNameInput(string nameInput, bool skipNoLookup = true) {
			return GetRecordsFromNameInput(nameInput, skipNoLookup)?.Select(record => record.Abbreviation).ToArray();
		}

		public string GetFullNameFromAbbr(string abbr) {
			return GetRecordFromAbbr(abbr)?.FullName;
		}

		public IReadOnlyList<TeacherRecord> GetAllRecords() {
			return m_Records as IReadOnlyList<TeacherRecord>;
		}
	}

	public class TeacherRecord {
		public string	Abbreviation;
		public string	FullName;
		public string[] AltSpellings;
		public bool		NoLookup;
		public string	DiscordUser;

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
	}
}
