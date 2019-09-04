using System;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.Schedule.GLU {
	public class GLUScheduleRecord : ScheduleRecord {
		public override bool ShouldCallNextCommand => Activity.ScheduleCode == "pauze";

		public override Task<string> PresentAsync(IdentifierInfo info) {
			string ret = $":notepad_spiral: {Activity.DisplayText}\n";

			if (Activity.ScheduleCode != "stdag doc") {
				if (Activity.ScheduleCode != "pauze") {
					if (info.ScheduleField != "StaffMember") {
						if (StaffMember.Length == 1 && StaffMember[0].IsUnknown) {
							ret += $":bust_in_silhouette: Onbekende leraar met afkorting {StaffMember[0].Abbreviation}\n";
						}

						string teachers = string.Join(", ", StaffMember.Select(teacher => teacher.DisplayText));
						if (!string.IsNullOrWhiteSpace(teachers)) {
							if (StaffMember.Length == 1 && StaffMember[0].Abbreviation == "JWO") {
								ret += $"<:VRjoram:392762653367336960> {teachers}\n";
							} else {
								ret += $":bust_in_silhouette: {teachers}\n";
							}
						}
					}
					if (info.ScheduleField != "StudentSets" && !string.IsNullOrWhiteSpace(StudentSetsString)) {
						ret += $":busts_in_silhouette: {StudentSetsString}\n";
					}
					if (info.ScheduleField != "Room" && !string.IsNullOrWhiteSpace(RoomString)) {
						ret += $":round_pushpin: {RoomString}\n";
					}
				}

				if (Start.Date != DateTime.Today) {
					ret += $":calendar_spiral: {ScheduleUtil.GetStringFromDayOfWeek(Start.DayOfWeek)} {Start.ToString("dd-MM-yyyy")}\n";
				}

				ret += $":clock5: {Start.ToString("HH:mm")} - {End.ToString("HH:mm")}";
				if (Start.Date == DateTime.Today && Start > DateTime.Now) {
					TimeSpan timeTillStart = Start - DateTime.Now;
					ret += $" - nog {timeTillStart.Hours}:{timeTillStart.Minutes.ToString().PadLeft(2, '0')}";
				}

				ret += $"\n:stopwatch: {(int) Duration.TotalHours}:{Duration.Minutes.ToString().PadLeft(2, '0')}";
				if (Start < DateTime.Now && End > DateTime.Now) {
					TimeSpan timeLeft = End - DateTime.Now;
					ret += $" - nog {timeLeft.Hours}:{timeLeft.Minutes.ToString().PadLeft(2, '0')}";
				}

				if (BreakStart.HasValue) {
					ret += $"\n:coffee: {BreakStart.Value.ToString("HH:mm")} - {BreakEnd.Value.ToString("HH:mm")}\n";
				}
			}

			return Task.FromResult(ret);
		}
	}
}
