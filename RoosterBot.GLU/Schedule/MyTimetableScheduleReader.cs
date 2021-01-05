using System;
using System.Collections.Generic;
using System.Linq;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class MyTimetableScheduleReader : ScheduleReader {
		private readonly string m_IcsPath;
		private readonly StaffMemberService m_StaffMembers;
		private readonly SnowflakeReference m_StaffMemberChannel;

		public MyTimetableScheduleReader(string icsPath, StaffMemberService staffMembers, SnowflakeReference staffMemberChannel) {
			m_IcsPath = icsPath;
			m_StaffMembers = staffMembers;
			m_StaffMemberChannel = staffMemberChannel;

			//https://rooster.glu.nl/export?format=ICAL&locale=en_GB&group=true&startDate=2021-02-01&endDate=2021-02-08
		}

		public override IReadOnlyList<ScheduleRecord> GetSchedule() {
			var calParser = new EWSoftware.PDI.Parser.VCalendarParser();
			calParser.ParseFile(m_IcsPath);

			return calParser.VCalendar.Events.ListSelect(evt => {
				string[] descriptionLines = evt.Description.Value.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

				const string StaffMemberHeader = "Staff member(s): ";
				const string StudentSetsHeader = "Student set(s): ";

				string? staffMemberLine = descriptionLines.Where(line => line.StartsWith(StaffMemberHeader)).FirstOrDefault()?[StaffMemberHeader.Length..];
				string? studentSetsLine = descriptionLines.Where(line => line.StartsWith(StudentSetsHeader)).FirstOrDefault()?[StudentSetsHeader.Length..];

				IReadOnlyList<StaffMemberInfo> staffMember;
				IReadOnlyList<StudentSetInfo> studentSets;

				if (staffMemberLine is not null) {
					staffMember = staffMemberLine.Split(", ").ListSelect(item => m_StaffMembers.GetRecordFromAbbr(m_StaffMemberChannel, item) ?? new StaffMemberInfo(item, item, true, true, null, new List<string>()));
				} else {
					staffMember = new List<StaffMemberInfo>();
				}

				if (studentSetsLine is not null) {
					studentSets = studentSetsLine.Split(", ").ListSelect(item => new StudentSetInfo(item));
				} else {
					studentSets = new List<StudentSetInfo>();
				}

				return new GLUScheduleRecord(
					new ActivityInfo(evt.Summary.Value, GLUActivities.GetActivityFromAbbr(evt.Summary.Value)),
					evt.StartDateTime.UtcDateTime,
					evt.EndDateTime.UtcDateTime,
					studentSets,
					staffMember,
					evt.Location.Value.Split(", ").ListSelect(location => new RoomInfo(location))
				);
			});
		}
	}
}
