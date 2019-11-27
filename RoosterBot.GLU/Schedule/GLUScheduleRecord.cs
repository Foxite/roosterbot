using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Discord;
using RoosterBot.DateTimeUtils;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class GLUScheduleRecord : ScheduleRecord {
		public override bool ShouldCallNextCommand => Activity.ScheduleCode == "pauze";

		public GLUScheduleRecord(ActivityInfo activity, DateTime start, DateTime end, IReadOnlyList<StudentSetInfo> studentSets, IReadOnlyList<TeacherInfo> staffMember, IReadOnlyList<RoomInfo> room)
			: base(activity, start, end, studentSets, staffMember, room) { }

		public override IEnumerable<AspectListItem> Present(IdentifierInfo info) {
			yield return new AspectListItem(new Emoji("🗒️"), "Activiteit", Activity.DisplayText);

			if (Activity.ScheduleCode != "stdag doc") {
				if (Activity.ScheduleCode != "pauze") {
					if (!(info is TeacherInfo)) {
						if (StaffMember.Count == 1 && StaffMember[0].IsUnknown) {
							yield return new AspectListItem(new Emoji("👤"), "Leraar", StaffMember[0].ScheduleCode);
						}

						string teachers = string.Join(", ", StaffMember.Select(teacher => teacher.DisplayText));
						if (!string.IsNullOrWhiteSpace(teachers)) {
							if (StaffMember.Count == 1 && StaffMember[0].ScheduleCode == "JWO") {
								yield return new AspectListItem(new Emoji("<:VRjoram:392762653367336960>"), "Leraar", teachers);
							} else {
								yield return new AspectListItem(new Emoji("👤"), "Leraar", teachers);
							}
						}
					}
					if (!(info is StudentSetInfo) && !string.IsNullOrWhiteSpace(StudentSetsString)) {
						yield return new AspectListItem(new Emoji("👥"), "Klas", StudentSetsString);
					}
					if (!(info is RoomInfo) && !string.IsNullOrWhiteSpace(RoomString)) {
						yield return new AspectListItem(new Emoji("📍"), "Lokaal", RoomString);
					}
				}

				if (Start.Date != DateTime.Today) {
					yield return new AspectListItem(new Emoji("🗓️"), "Datum", $"{Start.DayOfWeek.GetName(CultureInfo.GetCultureInfo("nl-NL"))} {Start.ToString("dd-MM-yyyy")}");
				}

				string timeString = $"{Start.ToString("HH:mm")} - {End.ToString("HH:mm")}";
				if (Start.Date == DateTime.Today && Start > DateTime.Now) {
					TimeSpan timeTillStart = Start - DateTime.Now;
					 timeString += $" - nog {timeTillStart.Hours}:{timeTillStart.Minutes.ToString().PadLeft(2, '0')}";
				}
				yield return new AspectListItem(new Emoji("🕔"), "Tijd", timeString);


				timeString = $"{(int) Duration.TotalHours}:{Duration.Minutes.ToString().PadLeft(2, '0')}";
				if (Start < DateTime.Now && End > DateTime.Now) {
					TimeSpan timeLeft = End - DateTime.Now;
					timeString += $" - nog {timeLeft.Hours}:{timeLeft.Minutes.ToString().PadLeft(2, '0')}";
				}
				yield return new AspectListItem(new Emoji("⏱️"), "Tijd over", timeString);

				if (Break != null) {
					yield return new AspectListItem(new Emoji("☕"), "Pauze", $"{Break.Start.ToString("HH:mm")} - {Break.End.ToString("HH:mm")}");
				}
			}
		}
	}
}
