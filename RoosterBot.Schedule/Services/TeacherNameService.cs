using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Discord;

namespace RoosterBot.Schedule {
	public class TeacherNameService {
		private readonly List<GuildTeacherList> m_Records = new List<GuildTeacherList>();
		
		/// <summary>
		/// Loads a CSV with teacher abbreviations into memory.
		/// </summary>
		public async Task ReadAbbrCSV(string path, ulong[] allowedGuilds) {
			Logger.Info("TeacherNameService", $"Loading abbreviation CSV file {Path.GetFileName(path)}");

			using (StreamReader reader = File.OpenText(path)) {
				using var csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration() { Delimiter = "," });
				await csv.ReadAsync();
				csv.ReadHeader();

				var currentRecords = new List<TeacherInfo>();

				while (await csv.ReadAsync()) {
					string altSpellingsString = csv["AltSpellings"];
					string[]? altSpellings = null;

					if (!string.IsNullOrEmpty(altSpellingsString)) {
						altSpellings = altSpellingsString.Split(',');
					};

					var record = new TeacherInfo(
						scheduleCode: csv["Abbreviation"],
						displayText: csv["FullName"],
						isUnknown: false,
						noLookup: bool.Parse(csv["NoLookup"]),
						discordUser: csv["DiscordUser"],
						altSpellings: altSpellings ?? Array.Empty<string>()
					);

					currentRecords.Add(record);
				}

				lock (m_Records) {
					m_Records.Add(new GuildTeacherList(allowedGuilds, currentRecords));
				}
			}
			Logger.Info("TeacherNameService", $"Successfully loaded abbreviation CSV file {Path.GetFileName(path)}");
		}

		public TeacherInfo GetRecordFromAbbr(ulong guild, string abbr) {
			return GetAllowedRecordsForGuild(guild).FirstOrDefault(record => record.ScheduleCode == abbr);
		}

		public TeacherInfo[] GetRecordsFromAbbrs(ulong guild, string[] abbrs) {
			var records = new List<TeacherInfo>();
			for (int i = 0; i < abbrs.Length; i++) {
				TeacherInfo record = GetRecordFromAbbr(guild, abbrs[i]);
				if (record != null) {
					records.Add(record);
				} else if (!string.IsNullOrWhiteSpace(abbrs[i])) {
					records.Add(new TeacherInfo(
						scheduleCode: abbrs[i],
						displayText: '"' + abbrs[i] + '"',
						isUnknown: true,
						noLookup: true,
						discordUser: null,
						altSpellings: Array.Empty<string>()
					));
				}
			}
			return records.ToArray();
		}
		
		public IReadOnlyCollection<TeacherMatch> Lookup(ulong guild, string nameInput, bool skipNoLookup = true) {
			// TODO (feature) Improve multiple match resolution
			// Consider a number of teachers, two of which:
			// "Mar Lastname", "MLA"
			// "Martin Othername", "MOT"
			// If given the input "mar", this should return MLA, and not [ MLA, MOT ]
			// If given the input "ma", this should return [ MLA, MOT ] (along with whatever other teachers match)
			// However if there's two "Martin"s then when given the input "martin" this should still return all matches.
			nameInput = nameInput.ToLower();

			var records = new List<TeacherMatch>();

			foreach (TeacherInfo record in GetAllowedRecordsForGuild(guild)) {
				if (skipNoLookup && record.NoLookup)
					continue;

				float score = record.MatchName(nameInput);
				if (score > 0) {
					records.Add(new TeacherMatch(record, score));
				}
			}
			
			return records.AsReadOnly();
		}

		public TeacherInfo? GetTeacherByDiscordUser(IGuild guild, IUser user) {
			string findDiscordUser = $"{user.Name}#{user.Discriminator}";
			foreach (TeacherInfo teacher in GetAllowedRecordsForGuild(guild.Id)) {
				if (findDiscordUser == teacher.DiscordUser) {
					return teacher;
				}
			}
			return null;
		}
		
		public IEnumerable<TeacherInfo> GetAllRecords(ulong guild) {
			return GetAllowedRecordsForGuild(guild);
		}

		private IEnumerable<TeacherInfo> GetAllowedRecordsForGuild(ulong guild) {
			return m_Records.Where(gtl => gtl.IsGuildAllowed(guild)).SelectMany(gtl => gtl.Teachers);
		}

		private class GuildTeacherList : GuildSpecificInfo {
			private readonly List<TeacherInfo> m_Teachers;

			public IReadOnlyList<TeacherInfo> Teachers => m_Teachers;

			public GuildTeacherList(ulong[] allowedGuilds, List<TeacherInfo> teachers) : base(allowedGuilds) {
				m_Teachers = teachers;
			}
		}
	}

	public class TeacherMatch {
		public TeacherInfo Teacher { get; }
		public float Score { get; }

		public TeacherMatch(TeacherInfo teacher, float score) {
			Teacher = teacher;
			Score = score;
		}
	}
}
