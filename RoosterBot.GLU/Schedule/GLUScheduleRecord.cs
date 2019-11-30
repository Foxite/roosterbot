using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Discord;
using RoosterBot.DateTimeUtils;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class GLUScheduleRecord : ScheduleRecord {
		private readonly ResourceService m_Resources;

		public override bool ShouldCallNextCommand => Activity.ScheduleCode == "pauze";

		public GLUScheduleRecord(ResourceService resources, ActivityInfo activity, DateTime start, DateTime end, IReadOnlyList<StudentSetInfo> studentSets, IReadOnlyList<TeacherInfo> staffMember, IReadOnlyList<RoomInfo> room)
			: base(activity, start, end, studentSets, staffMember, room) {
			m_Resources = resources;
		}

		public override IEnumerable<AspectListItem> Present(CultureInfo culture) {
			AspectListItem getAspect(IEmote emote, string nameKey, string valueKey, params string[] objects) {
				return new AspectListItem(emote, m_Resources.GetString(culture, nameKey), string.Format(m_Resources.GetString(culture, valueKey), objects));
			}

			yield return getAspect(new Emoji("🗒️"), "GLUScheduleRecord_Aspect_Activity", Activity.DisplayText);

			if (Activity.ScheduleCode != "stdag doc") {
				if (Activity.ScheduleCode != "pauze") {
					if (StaffMember.Count > 0) {
						if (StaffMember[0].IsUnknown && StaffMember.Count == 1) {
							yield return getAspect(new Emoji("👤"), "GLUScheduleRecord_Aspect_StaffMember", "GLUScheduleRecord_UnknownStaffMember", StaffMember[0].ScheduleCode);
						} else {
							string teachers = string.Join(", ", StaffMember.Select(teacher => teacher.DisplayText));
							IEmote teacherEmote;
							if (StaffMember.Count == 1 && StaffMember[0].ScheduleCode == "JWO") {
								teacherEmote = Emote.Parse("<:VRjoram:392762653367336960>");
							} else {
								teacherEmote = new Emoji("👤");
							}
							yield return getAspect(teacherEmote, "GLUScheduleRecord_Aspect_StaffMember", teachers);
						}
					}
					yield return getAspect(new Emoji("👥"), "GLUScheduleRecord_Aspect_StudentSets", StudentSetsString);
					yield return getAspect(new Emoji("📍"), "GLUScheduleRecord_Aspect_Room", RoomString);
				}

				if (Start.Date != DateTime.Today) {
					yield return getAspect(new Emoji("🗓️"), "GLUScheduleRecord_Aspect_Date", Start.ToLongDateString(culture));
				}

				string timeString = string.Format(m_Resources.GetString(culture, "GLUScheduleRecord_TimeStartEnd"), Start.ToString("HH:mm"), End.ToString("HH:mm"));
				if (Start.Date == DateTime.Today && Start > DateTime.Now) {
					TimeSpan timeTillStart = Start - DateTime.Now;
					timeString += string.Format(m_Resources.GetString(culture, "GLUScheduleRecord_TimeLeft"), timeTillStart.ToString("H:mm"));
				}
				yield return getAspect(new Emoji("🕔"), "GLUScheduleRecord_Aspect_TimeStartEnd", timeString);


				timeString = Duration.ToString("H:mm");
				if (Start < DateTime.Now && End > DateTime.Now) {
					TimeSpan timeLeft = End - DateTime.Now;
					timeString += string.Format(m_Resources.GetString(culture, "GLUScheduleRecord_TimeLeft"), timeLeft.ToString("H:mm"));
				}
				yield return getAspect(new Emoji("⏱️"), "GLUScheduleRecord_Aspect_TimeLeft", timeString);

				if (Break != null) {
					yield return getAspect(new Emoji("⏱️"), "GLUScheduleRecord_Aspect_Break", "GLUScheduleRecord_TimeStartEnd", Break.Start.ToString("HH:mm"), Break.End.ToString("HH:mm"));
				}
			}
		}

		public override IReadOnlyList<string> PresentRow(CultureInfo culture) {
			return new[] {
				Activity.DisplayText.ToString(),
				string.Format(m_Resources.GetString(culture, "GLUScheduleRecord_TimeStartEnd"), Start.ToShortTimeString(culture), End.ToShortTimeString(culture)),
				StudentSetsString,
				StaffMemberString,
				RoomString
			};
		}
	}
}
