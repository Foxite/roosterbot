using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Discord;
using RoosterBot;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Services {
	public class TeacherNameService {
		private List<GuildTeacherList> m_Records = new List<GuildTeacherList>();
		
		/// <summary>
		/// Loads a CSV with teacher abbreviations into memory.
		/// </summary>
		public async Task ReadAbbrCSV(string path, ulong[] allowedGuilds) {
			Logger.Log(LogSeverity.Info, "TeacherNameService", $"Loading abbreviation CSV file {Path.GetFileName(path)}");

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
						m_Records.Add(new GuildTeacherList(allowedGuilds, currentRecords));
					}
				}
			}
			Logger.Log(LogSeverity.Info, "TeacherNameService", $"Successfully loaded abbreviation CSV file {Path.GetFileName(path)}");
		}

		public TeacherInfo GetRecordFromAbbr(ulong guild, string abbr) {
			return GetAllowedRecordsForGuild(guild).First(record => record.Abbreviation == abbr);
		}

		public TeacherInfo[] GetRecordsFromAbbrs(string[] abbrs) {
			List<TeacherInfo> records = new List<TeacherInfo>();
			for (int i = 0; i < abbrs.Length; i++) {
				TeacherInfo record = GetRecordFromAbbr(abbrs[i]);
				if (record != null) {
					records.Add(record);
				} else if (!string.IsNullOrWhiteSpace(abbrs[i])) {
					records.Add(new TeacherInfo() {
						IsUnknown = true,
						Abbreviation = abbrs[i],
						FullName = '"' + abbrs[i] + '"',
						NoLookup = true
					});
				}
			}
			return records.ToArray();
		}
		
		public TeacherInfo[] Lookup(ulong guild, string nameInput, bool skipNoLookup = true) {
			nameInput = nameInput.ToLower();

			List<TeacherInfo> records = new List<TeacherInfo>();

			foreach (TeacherInfo record in GetAllowedRecordsForGuild(guild)) {
				if (skipNoLookup && record.NoLookup)
					continue;

				if (record.MatchName(nameInput)) {
					records.Add(record);
				}
			}
			
			return records.ToArray();
		}
		
		public IEnumerable<TeacherInfo> GetAllRecords(ulong guild) {
			return GetAllowedRecordsForGuild(guild);
		}

		private IEnumerable<TeacherInfo> GetAllowedRecordsForGuild(ulong guild) {
			return m_Records.Where(gtl => gtl.IsGuildAllowed(guild)).SelectMany(gtl => gtl.Teachers);
		}

		private class GuildTeacherList {
			private ulong[] m_AllowedGuilds;
			private List<TeacherInfo> m_Teachers;

			public IReadOnlyList<TeacherInfo> Teachers => m_Teachers;

			public GuildTeacherList(ulong[] allowedGuilds, List<TeacherInfo> teachers) {
				m_AllowedGuilds = allowedGuilds;
				m_Teachers = teachers;
			}

			public bool IsGuildAllowed(ulong guildId) => m_AllowedGuilds.Contains(guildId);
			public bool IsGuildAllowed(IGuild guild) => IsGuildAllowed(guild.Id);
		}
	}
}
