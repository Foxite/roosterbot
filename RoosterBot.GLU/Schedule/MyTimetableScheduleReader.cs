using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class MyTimetableScheduleReader : ScheduleReader {
		private readonly string m_SessionToken;
		private readonly StaffMemberService m_StaffMembers;
		private readonly SnowflakeReference m_StaffMemberChannel;

		public MyTimetableScheduleReader(string mttSessionToken, StaffMemberService staffMembers, SnowflakeReference staffMemberChannel) {
			m_SessionToken = mttSessionToken;
			m_StaffMembers = staffMembers;
			m_StaffMemberChannel = staffMemberChannel;
		}

		public override IReadOnlyList<ScheduleRecord> GetSchedule() {
			var cookieContainer = new CookieContainer();
			cookieContainer.Add(new Uri("https://rooster.glu.nl"), new Cookie("JSESSIONID", m_SessionToken));
			cookieContainer.Add(new Uri("https://rooster.glu.nl"), new Cookie("zoneview", "false"));

			using var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
			using var client = new HttpClient(handler);
			client.DefaultRequestHeaders.Add("User-Agent", $"RoosterBot.GLU/{Program.Version}-{GLUComponent.Version}");

			DateTime startDate = DateTime.Today - TimeSpan.FromDays(14);
			DateTime endDate = DateTime.Today + TimeSpan.FromDays(60);
			string icsUrl = $"https://rooster.glu.nl/export?format=ICAL&locale=en_GB&group=true&startDate={startDate.Year}-{startDate.Month:00}-{startDate.Day:00}&endDate={endDate.Year}-{endDate.Month:00}-{endDate.Day:00}";
			using Stream downloadStream = client.GetStreamAsync(icsUrl).GetAwaiter().GetResult();
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
					evt.Location.Value.Split(", ").ListSelect(location => new RoomInfo(location))
				);
			});
		}
	}
}
