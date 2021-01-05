using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class MyTimetableScheduleReader : ScheduleReader {
		private readonly string m_ICalLink;
		private readonly StaffMemberService m_StaffMembers;
		private readonly SnowflakeReference m_StaffMemberChannel;

		public MyTimetableScheduleReader(string icalLink, StaffMemberService staffMembers, SnowflakeReference staffMemberChannel) {
			m_ICalLink = icalLink;
			m_StaffMembers = staffMembers;
			m_StaffMemberChannel = staffMemberChannel;
		}

		public override IReadOnlyList<ScheduleRecord> GetSchedule() {
			using var client = new HttpClient();
			client.DefaultRequestHeaders.Add("User-Agent", $"RoosterBot.GLU/{Program.Version}-{GLUComponent.Version}");

			DateTime startDate = DateTime.Today - TimeSpan.FromDays(14);
			DateTime endDate = DateTime.Today + TimeSpan.FromDays(60);

			using Stream downloadStream = client.GetStreamAsync(m_ICalLink).GetAwaiter().GetResult();
			using var sr = new StreamReader(downloadStream);

			var calParser = new EWSoftware.PDI.Parser.VCalendarParser();
			calParser.ParseReader(sr);

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
					evt.Location.Value.Split(", ", StringSplitOptions.RemoveEmptyEntries).ListSelect(location => new RoomInfo(location))
				);
			});
		}
	}
}
