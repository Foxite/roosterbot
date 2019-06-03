using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using RoosterBot;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Services {
	public class TeacherNameService {
		private List<TeacherInfo> m_Records = new List<TeacherInfo>();
		
		/// <summary>
		/// Loads a CSV with teacher abbreviations into memory.
		/// </summary>
		public async Task ReadAbbrCSV(string path) {
			Logger.Log(Discord.LogSeverity.Info, "TeacherNameService", $"Loading abbreviation CSV file {Path.GetFileName(path)}");

			using (StreamReader reader = File.OpenText(path)) {
				using (CsvReader csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration() { Delimiter = "," })) {
					await csv.ReadAsync();
					csv.ReadHeader();

					List<TeacherInfo> currentRecords = new List<TeacherInfo>();

					while (await csv.ReadAsync()) {
						TeacherInfo record = new TeacherInfo() {
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
			}
			Logger.Log(Discord.LogSeverity.Info, "TeacherNameService", $"Successfully loaded abbreviation CSV file {Path.GetFileName(path)}");
		}

		public TeacherInfo GetRecordFromAbbr(string abbr) {
			return m_Records.Find(record => record.Abbreviation == abbr);
		}

		public TeacherInfo[] GetRecordsFromAbbrs(string[] abbrs) {
			List<TeacherInfo> records = new List<TeacherInfo>();
			for (int i = 0; i < abbrs.Length; i++) {
				TeacherInfo record = GetRecordFromAbbr(abbrs[i]);
				if (record != null) {
					records.Add(record);
				}
			}
			return records.ToArray();
		}
		
		public TeacherInfo[] Lookup(string nameInput, bool skipNoLookup = true) {
			nameInput = nameInput.ToLower();

			List<TeacherInfo> records = new List<TeacherInfo>();

			foreach (TeacherInfo record in m_Records) {
				if (skipNoLookup && record.NoLookup)
					continue;

				if (record.MatchName(nameInput)) {
					records.Add(record);
				}
			}
			
			return records.ToArray();
		}
		
		public IReadOnlyList<TeacherInfo> GetAllRecords() {
			return m_Records;
		}
	}
}
