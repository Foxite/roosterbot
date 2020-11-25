using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public record GLUScheduleRecord : ScheduleRecord {
		public IReadOnlyList<StudentSetInfo> StudentSets { get; }
		public IReadOnlyList<StaffMemberInfo> StaffMember { get; }
		public IReadOnlyList<RoomInfo> Room { get; }
		public BreakTime? Break { get; }

		public override IEnumerable<IdentifierInfo> InvolvedIdentifiers => ((IEnumerable<IdentifierInfo>) StudentSets).Concat(StaffMember).Concat(Room);

		public GLUScheduleRecord(ActivityInfo activity, DateTime start, DateTime end, IReadOnlyList<StudentSetInfo> studentSets, IReadOnlyList<StaffMemberInfo> staffMember, IReadOnlyList<RoomInfo> room)
			: base(activity, start, end) {
			StudentSets = studentSets;
			StaffMember = staffMember;
			Room = room;
		}

		[JsonIgnore] public TimeSpan Duration => End - Start;
		[JsonIgnore] public string StudentSetsString => string.Join(", ", StudentSets.Select(info => info.ScheduleCode));
		[JsonIgnore] public string StaffMemberString => string.Join(", ", StaffMember.Select(info => info.DisplayText));
		[JsonIgnore] public string RoomString => string.Join(", ", Room.Select(info => info.ScheduleCode));

		public override IEnumerable<AspectListItem> Present(RoosterCommandContext context) {
			CultureInfo culture = context.Culture;

			AspectListItem getAspect(IEmote emote, string nameKey, string value) {
				return new AspectListItem(emote, context.GetString(nameKey), value);
			}

			yield return getAspect(new Emoji("🗒️"), "GLUScheduleRecord_Aspect_Activity", Activity.DisplayText);

			if (Activity.ScheduleCode != "stdag doc") {
				if (Activity.ScheduleCode != "pauze") {
					if (StaffMember.Count > 0) {
						if (StaffMember[0].IsUnknown && StaffMember.Count == 1) {
							yield return getAspect(new Emoji("👤"), "GLUScheduleRecord_Aspect_StaffMember", context.GetString("GLUScheduleRecord_UnknownStaffMember", StaffMember[0].ScheduleCode));
						} else {
							string staffMembers;
							if (StaffMember.Count > 5) {
								staffMembers = string.Join(", ", StaffMember.Take(5).Select(staffMember => staffMember.DisplayText)) + "... (" + (StaffMember.Count - 5) + " extra)";
							} else {
								staffMembers = string.Join(", ", StaffMember.Select(staffMember => staffMember.DisplayText));
							}
							IEmote staffMemberEmote = new Emoji("👤");
							yield return getAspect(staffMemberEmote, "GLUScheduleRecord_Aspect_StaffMember", staffMembers);
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

				string timeString = context.GetString("GLUScheduleRecord_TimeStartEnd", Start.ToString("t", culture), End.ToString("t", culture));
				if (Start.Date == DateTime.Today && Start > DateTime.Now) {
					TimeSpan timeLeft = Start - DateTime.Now;
					timeString += context.GetString("GLUScheduleRecord_TimeLeft", timeLeft.ToString("h':'mm':'ss", culture));
				}
				yield return getAspect(new Emoji("🕔"), "GLUScheduleRecord_Aspect_TimeStartEnd", timeString);


				timeString = Duration.ToString("h':'mm", culture);
				if (Start < DateTime.Now && End > DateTime.Now) {
					TimeSpan timeLeft = End - DateTime.Now;
					timeString += context.GetString("GLUScheduleRecord_TimeLeft", timeLeft.ToString("h':'mm':'ss", culture));
				}
				yield return getAspect(new Emoji("⏱️"), "GLUScheduleRecord_Aspect_TimeLeft", timeString);

				if (Break != null) {
					yield return getAspect(new Emoji("☕"), "GLUScheduleRecord_Aspect_Break", context.GetString("GLUScheduleRecord_TimeStartEnd", Break.Start.ToString("t", culture), Break.End.ToString("t", culture)));
				}

				if (Activity.ScheduleCode != "pauze") {
					yield return getAspect(new Emoji("☣️"), "GLUScheduleRecord_QuarantineLabel", context.GetString("GLUScheduleRecord_Quarantine"));
				}
			}
		}

		public override IReadOnlyList<string> PresentRow(RoosterCommandContext context) {
			return new[] {
				Activity.DisplayText.ToString(),
				context.GetString("GLUScheduleRecord_TimeStartEnd", Start.ToString("t", context.Culture), End.ToString("t", context.Culture)),
				StudentSetsString,
				StaffMember.Count > 5
					? string.Join(", ", StaffMember.Take(5).Select(teacher => teacher.DisplayText)) + "... (" + (StaffMember.Count - 5) + " extra)"
					: string.Join(", ", StaffMember.Select(teacher => teacher.DisplayText)),
				RoomString
			};
		}

		public override IReadOnlyList<string> PresentRowHeadings(RoosterCommandContext context) => new string[] {
			context.GetString("ScheduleModule_RespondDay_ColumnActivity"),
			context.GetString("ScheduleModule_RespondDay_ColumnTime"),
			context.GetString("ScheduleModule_RespondDay_ColumnStudentSets"),
			context.GetString("ScheduleModule_RespondDay_ColumnStaffMember"),
			context.GetString("ScheduleModule_RespondDay_ColumnRoom")
		};
	}
}
