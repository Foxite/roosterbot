using System.Collections.Generic;
using System.Linq;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class StaffMemberService {
		private readonly List<ChannelStaffList> m_Records = new List<ChannelStaffList>();
		
		public void AddStaff(IEnumerable<StaffMemberInfo> members, IEnumerable<SnowflakeReference> allowedChannels) {
			m_Records.Add(new ChannelStaffList(allowedChannels, members.ToList()));
		}

		public StaffMemberInfo? GetRecordFromAbbr(SnowflakeReference channel, string abbr) {
			return GetAllowedRecordsForChannel(channel).FirstOrDefault(record => record.ScheduleCode == abbr);
		}
		
		public IReadOnlyCollection<StaffMemberMatch> Lookup(SnowflakeReference channel, string nameInput, bool skipNoLookup = true) {
			// TODO (feature) Improve multiple match resolution
			// Consider a number of staff member, two of which:
			// "Mar Lastname", "MLA"
			// "Martin Othername", "MOT"
			// If given the input "mar", this should return MLA, and not [ MLA, MOT ]
			// If given the input "ma", this should return [ MLA, MOT ] (along with whatever other staff members match)
			// However if there's two "Martin"s then when given the input "martin" this should still return all matches.
			nameInput = nameInput.ToLower();

			var records = new List<StaffMemberMatch>();

			foreach (StaffMemberInfo record in GetAllowedRecordsForChannel(channel)) {
				if (skipNoLookup && record.NoLookup)
					continue;

				float score = record.MatchName(nameInput);
				if (score > 0) {
					records.Add(new StaffMemberMatch(record, score));
				}
			}
			
			return records.AsReadOnly();
		}

		public StaffMemberInfo? GetStaffMemberByDiscordUser(SnowflakeReference channel, IUser user) {
			string findDiscordUser = user.UserName;
			foreach (StaffMemberInfo staffMember in GetAllowedRecordsForChannel(channel)) {
				if (findDiscordUser == staffMember.DiscordUser) {
					return staffMember;
				}
			}
			return null;
		}
		
		public IEnumerable<StaffMemberInfo> GetAllRecords(SnowflakeReference channel) {
			return GetAllowedRecordsForChannel(channel);
		}

		private IEnumerable<StaffMemberInfo> GetAllowedRecordsForChannel(SnowflakeReference channel) {
			return m_Records.Where(gtl => gtl.IsChannelAllowed(channel)).SelectMany(gtl => gtl.Members);
		}

		private class ChannelStaffList : ChannelSpecificInfo {
			private readonly List<StaffMemberInfo> m_Members;

			public IReadOnlyList<StaffMemberInfo> Members => m_Members.AsReadOnly();

			public ChannelStaffList(IReadOnlyCollection<SnowflakeReference> allowedChannels, List<StaffMemberInfo> members) : base(allowedChannels) {
				m_Members = members;
			}
		}
	}

	public class StaffMemberMatch {
		public StaffMemberInfo StaffMember { get; }
		public float Score { get; }

		public StaffMemberMatch(StaffMemberInfo member, float score) {
			StaffMember = member;
			Score = score;
		}
	}
}
