using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class GLUScheduleReader : ScheduleReader {
		private readonly string m_Path;
		private readonly TeacherNameService m_Teachers;
		private readonly SnowflakeReference m_TeacherChannel;
		private readonly bool m_SkipPastRecords;

		public GLUScheduleReader(string path, TeacherNameService teachers, SnowflakeReference teacherChannel, bool skipPastRecords) {
			m_Path = path;
			m_Teachers = teachers;
			m_TeacherChannel = teacherChannel;
			m_SkipPastRecords = skipPastRecords;
		}

		public override List<ScheduleRecord> GetSchedule() {
			Logger.Info("GLUScheduleReader", $"Loading CSV file from {m_Path}");

			int line = 1;
			try {
				List<ScheduleRecord> schedule;
				using (StreamReader reader = File.OpenText(m_Path))
				using (var csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration() { Delimiter = "," })) {
					csv.Read();
					csv.ReadHeader();

					DateTime lastMonday = DateTime.Today.AddDays(-(int) DateTime.Today.DayOfWeek + 1); // + 1 because C# weeks start on Sunday (which is 0, and Monday is 1, etc. Saturday is 6)

					schedule = new List<ScheduleRecord>();
					var culture = CultureInfo.GetCultureInfo("en-US");

					while (csv.Read()) {
						line++;
						var date = DateTime.ParseExact(csv["StartDate"], @"yyyy\-MM\-dd", culture);

						if (m_SkipPastRecords && date < lastMonday) { // Only store past records for this week
							continue;
						}

						DateTime start = date + TimeSpan.ParseExact(csv["StartTime"], @"hh\:mm", culture);
						DateTime end = date + TimeSpan.ParseExact(csv["EndTime"], @"hh\:mm", culture); // Under the assumption that nobody works overnight

						string[]? studentsets = csv["StudentSets"].Split(new[] { ", " }, StringSplitOptions.None);
						string[]? staffmember = csv["StaffMember"].Split(new[] { ", " }, StringSplitOptions.None);
						// Rooms often have " (0)" behind them. unknown reason.
						// Just remove them for now. This is the simplest way. We can't trim from the end, because multiple rooms may be listed and they will all have this suffix.
						string[]? room = csv["Room"].Replace(" (0)", "").Split(new[] { ", " }, StringSplitOptions.None);

						if (studentsets.Length == 1 && studentsets[0].Length == 0) {
							studentsets = null;
						}
						if (staffmember.Length == 1 && staffmember[0].Length == 0) {
							studentsets = null;
						}
						if (room.Length == 1 && room[0].Length == 0) {
							room = null;
						}

						ScheduleRecord record = new GLUScheduleRecord(
							activity: new ActivityInfo(csv["Activity"], GLUActivities.GetActivityFromAbbr(csv["Activity"])),
							start: start,
							end: end,
							studentSets: studentsets != null ? studentsets.Select(code => new StudentSetInfo(code)).ToList() : new List<StudentSetInfo>(),
							staffMember: staffmember != null ? staffmember.Select(abbr => m_Teachers.GetRecordFromAbbr(m_TeacherChannel, abbr)).ToList() : new List<TeacherInfo>(),
							room: room != null ? room.Select(code => new RoomInfo(code)).ToList() : new List<RoomInfo>()
						);

						bool shouldMerge(ScheduleRecord merge, out ScheduleRecord? into) {
							into = null;
							for (int i = schedule.Count - 1; i >= 0; i--) {
								ScheduleRecord item = schedule[i];
								if (item.StaffMember.Intersect(merge.StaffMember).Any() ||
									item.StudentSets.Intersect(merge.StudentSets).Any() ||
									item.Room.Intersect(merge.Room).Any()) {
									if (item.Activity == merge.Activity &&
										item.Start.Date == merge.Start.Date &&
										item.StaffMember.SequenceEqual(merge.StaffMember) &&
										item.StudentSets.SequenceEqual(merge.StudentSets) &&
										item.Room.SequenceEqual(merge.Room)) {
										into = item;
										return true;
									} else {
										return false;
									}
								}
							}

							return false;
						}

						if (shouldMerge(record, out ScheduleRecord? mergeInto)) {
							mergeInto!.Break = new BreakTime(mergeInto.End, record.Start);
							mergeInto.End = record.End;
						} else {
							schedule.Add(record);
						}
					}
				}
				return schedule;
			} catch (Exception e) {
				Logger.Critical("GLUScheduleReader", $"The following exception was thrown while loading the CSV at \"{m_Path}\" on line {line}", e);
				throw;
			}
		}
	}
}
