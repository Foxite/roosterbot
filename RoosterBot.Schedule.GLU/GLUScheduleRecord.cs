using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Schedule.GLU {
	public class GLUScheduleRecord : ScheduleRecord {
		public override bool ShouldCallNextCommand => Activity.ScheduleCode == "pauze";

		public GLUScheduleRecord(ActivityInfo activity, DateTime start, DateTime end, IReadOnlyList<StudentSetInfo> studentSets, IReadOnlyList<TeacherInfo> staffMember, IReadOnlyList<RoomInfo> room, TimeZoneInfo timezone)
			: base(activity, start, end, studentSets, staffMember, room, timezone) { }

		public override Task<string> PresentAsync(IdentifierInfo info) {
			string ret = $":notepad_spiral: {Activity.DisplayText}\n";

			if (Activity.ScheduleCode != "stdag doc") {
				if (Activity.ScheduleCode != "pauze") {
					if (!(info is TeacherInfo)) {
						if (StaffMember.Count == 1 && StaffMember[0].IsUnknown) {
							ret += $":bust_in_silhouette: Onbekende leraar met afkorting {StaffMember[0].ScheduleCode}\n";
						}

						string teachers = string.Join(", ", StaffMember.Select(teacher => teacher.DisplayText));
						if (!string.IsNullOrWhiteSpace(teachers)) {
							if (StaffMember.Count == 1 && StaffMember[0].ScheduleCode == "JWO") {
								ret += $"<:VRjoram:392762653367336960> {teachers}\n";
							} else {
								ret += $":bust_in_silhouette: {teachers}\n";
							}
						}
					}
					if (!(info is StudentSetInfo) && !string.IsNullOrWhiteSpace(StudentSetsString)) {
						ret += $":busts_in_silhouette: {StudentSetsString}\n";
					}
					if (!(info is RoomInfo) && !string.IsNullOrWhiteSpace(RoomString)) {
						ret += $":round_pushpin: {RoomString}\n";
					}
				}

				DateTime now = DateTime.UtcNow + Timezone.GetUtcOffset(DateTimeOffset.UtcNow);

				if (Start.Date != now.Date) {
					ret += $":calendar_spiral: {Start.DayOfWeek.GetName(CultureInfo.GetCultureInfo("nl-NL"))} {Start.ToString("dd-MM-yyyy")}\n";
				}

				ret += $":clock5: {Start.ToString("HH:mm")} - {End.ToString("HH:mm")}";
				if (Start.Date == now.Date && Start > now) {
					TimeSpan timeTillStart = Start - now;
					ret += $" - nog {timeTillStart.Hours}:{timeTillStart.Minutes.ToString().PadLeft(2, '0')}";
				}

				ret += $"\n:stopwatch: {(int) Duration.TotalHours}:{Duration.Minutes.ToString().PadLeft(2, '0')}";
				if (Start < now && End > now) {
					TimeSpan timeLeft = End - now;
					ret += $" - nog {timeLeft.Hours}:{timeLeft.Minutes.ToString().PadLeft(2, '0')}";
				}

				if (Break != null) {
					ret += $"\n:coffee: {Break.Start.ToString("HH:mm")} - {Break.End.ToString("HH:mm")}\n";
				}
			}

			return Task.FromResult(ret);
		}
	}
}
