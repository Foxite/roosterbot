using System;
using System.Collections.Generic;
using System.Linq;

namespace RoosterBot.Schedule {
	public class TeacherNameService {
		private readonly List<ChannelTeacherList> m_Records = new List<ChannelTeacherList>();
		
		public void AddTeachers(IEnumerable<TeacherInfo> teachers, IEnumerable<SnowflakeReference> allowedChannels) {
			m_Records.Add(new ChannelTeacherList(allowedChannels, teachers.ToList()));
		}

		public TeacherInfo GetRecordFromAbbr(SnowflakeReference channel, string abbr) {
			return GetAllowedRecordsForChannel(channel).FirstOrDefault(record => record.ScheduleCode == abbr);
		}

		public TeacherInfo[] GetRecordsFromAbbrs(SnowflakeReference channel, string[] abbrs) {
			var records = new List<TeacherInfo>();
			for (int i = 0; i < abbrs.Length; i++) {
				TeacherInfo record = GetRecordFromAbbr(channel, abbrs[i]);
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
		
		public IReadOnlyCollection<TeacherMatch> Lookup(SnowflakeReference channel, string nameInput, bool skipNoLookup = true) {
			// TODO (feature) Improve multiple match resolution
			// Consider a number of teachers, two of which:
			// "Mar Lastname", "MLA"
			// "Martin Othername", "MOT"
			// If given the input "mar", this should return MLA, and not [ MLA, MOT ]
			// If given the input "ma", this should return [ MLA, MOT ] (along with whatever other teachers match)
			// However if there's two "Martin"s then when given the input "martin" this should still return all matches.
			nameInput = nameInput.ToLower();

			var records = new List<TeacherMatch>();

			foreach (TeacherInfo record in GetAllowedRecordsForChannel(channel)) {
				if (skipNoLookup && record.NoLookup)
					continue;

				float score = record.MatchName(nameInput);
				if (score > 0) {
					records.Add(new TeacherMatch(record, score));
				}
			}
			
			return records.AsReadOnly();
		}

		public TeacherInfo? GetTeacherByDiscordUser(SnowflakeReference channel, IUser user) {
			string findDiscordUser = user.UserName;
			foreach (TeacherInfo teacher in GetAllowedRecordsForChannel(channel)) {
				if (findDiscordUser == teacher.DiscordUser) {
					return teacher;
				}
			}
			return null;
		}
		
		public IEnumerable<TeacherInfo> GetAllRecords(SnowflakeReference channel) {
			return GetAllowedRecordsForChannel(channel);
		}

		private IEnumerable<TeacherInfo> GetAllowedRecordsForChannel(SnowflakeReference channel) {
			return m_Records.Where(gtl => gtl.IsChannelAllowed(channel)).SelectMany(gtl => gtl.Teachers);
		}

		private class ChannelTeacherList : ChannelSpecificInfo {
			private readonly List<TeacherInfo> m_Teachers;

			public IReadOnlyList<TeacherInfo> Teachers => m_Teachers;

			public ChannelTeacherList(IEnumerable<SnowflakeReference> allowedChannels, List<TeacherInfo> teachers) : base(allowedChannels) {
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
