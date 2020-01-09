using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
//using Discord;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class GLUScheduleRecord : ScheduleRecord {
		public override bool ShouldCallNextCommand => Activity.ScheduleCode == "pauze";

		public GLUScheduleRecord(ActivityInfo activity, DateTime start, DateTime end, IReadOnlyList<StudentSetInfo> studentSets, IReadOnlyList<TeacherInfo> staffMember, IReadOnlyList<RoomInfo> room)
			: base(activity, start, end, studentSets, staffMember, room) { }

		public override IEnumerable<AspectListItem> Present(ResourceService resources, CultureInfo culture) {
			string getString(string key, params object[] objects) {
				return string.Format(resources.GetString(culture, key), objects);
			}

			AspectListItem getAspect(IEmote emote, string nameKey, string value) {
				return new AspectListItem(emote, getString(nameKey), value);
			}

			yield return getAspect(new Emoji("🗒️"), "GLUScheduleRecord_Aspect_Activity", Activity.DisplayText);

			if (Activity.ScheduleCode != "stdag doc") {
				if (Activity.ScheduleCode != "pauze") {
					if (StaffMember.Count > 0) {
						if (StaffMember[0].IsUnknown && StaffMember.Count == 1) {
							yield return getAspect(new Emoji("👤"), "GLUScheduleRecord_Aspect_StaffMember", getString("GLUScheduleRecord_UnknownStaffMember", StaffMember[0].ScheduleCode));
						} else {
							string teachers = string.Join(", ", StaffMember.Select(teacher => teacher.DisplayText));
							IEmote teacherEmote;
							/* TODO Move into GLU.Discord or delete
							if (StaffMember.Count == 1 && StaffMember[0].ScheduleCode == "JWO") {
								teacherEmote = Emote.Parse("<:VRjoram:392762653367336960>");
							} else {*/
								teacherEmote = new Emoji("👤");
							//}
							yield return getAspect(teacherEmote, "GLUScheduleRecord_Aspect_StaffMember", teachers);
						}
					}

					if (StudentSets.Count > 0) {
						yield return getAspect(new Emoji("👥"), "GLUScheduleRecord_Aspect_StudentSets", StudentSetsString);
					}

					if (Room.Count > 0) {
						yield return getAspect(new Emoji("📍"), "GLUScheduleRecord_Aspect_Room", RoomString);
					}
				}

				if (Start.Date != DateTime.Today) {
					yield return getAspect(new Emoji("🗓️"), "GLUScheduleRecord_Aspect_Date", Start.ToString("D", culture));
				}

				string timeString = getString("GLUScheduleRecord_TimeStartEnd", Start.ToString("t", culture), End.ToString("t", culture));
				if (Start.Date == DateTime.Today && Start > DateTime.Now) {
					TimeSpan timeLeft = Start - DateTime.Now;
					timeString += getString("GLUScheduleRecord_TimeLeft", timeLeft.ToString("h':'mm':'ss", culture));
				}
				yield return getAspect(new Emoji("🕔"), "GLUScheduleRecord_Aspect_TimeStartEnd", timeString);


				timeString = Duration.ToString("h':'mm", culture);
				if (Start < DateTime.Now && End > DateTime.Now) {
					TimeSpan timeLeft = End - DateTime.Now;
					timeString += getString("GLUScheduleRecord_TimeLeft", timeLeft.ToString("h':'mm':'ss", culture));
				}
				yield return getAspect(new Emoji("⏱️"), "GLUScheduleRecord_Aspect_TimeLeft", timeString);

				if (Break != null) {
					yield return getAspect(new Emoji("☕"), "GLUScheduleRecord_Aspect_Break", getString("GLUScheduleRecord_TimeStartEnd", Break.Start.ToString("t", culture), Break.End.ToString("t", culture)));
				}
			}
		}

		public override IReadOnlyList<string> PresentRow(ResourceService resources, CultureInfo culture) {
			return new[] {
				Activity.DisplayText.ToString(),
				string.Format(resources.GetString(culture, "GLUScheduleRecord_TimeStartEnd"), Start.ToString("t", culture), End.ToString("t", culture)),
				StudentSetsString,
				StaffMemberString,
				RoomString
			};
		}
	}
}
