using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CsvHelper;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class GLUScheduleReader : ScheduleReader {
		private readonly string m_Path;
		private readonly TeacherNameService m_Teachers;
		private readonly ulong m_Guild;
		private readonly bool m_SkipPastRecords;

		public GLUScheduleReader(string path, TeacherNameService teachers, ulong guild, bool skipPastRecords) {
			m_Path = path;
			m_Teachers = teachers;
			m_Guild = guild;
			m_SkipPastRecords = skipPastRecords;
		}

		public async override Task<List<ScheduleRecord>> GetSchedule() {
			Logger.Info("GLUScheduleReader", $"Loading CSV file from {m_Path}");

			int line = 1;
			try {
				List<ScheduleRecord> schedule;
				using (StreamReader reader = File.OpenText(m_Path))
				using (var csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration() { Delimiter = "," })) {
					await csv.ReadAsync();
					csv.ReadHeader();

					DateTime lastMonday = DateTime.Today.AddDays(-(int) DateTime.Today.DayOfWeek + 1); // + 1 because C# weeks start on Sunday (which is 0, and Monday is 1, etc. Saturday is 6)

					schedule = new List<ScheduleRecord>();
					var culture = CultureInfo.GetCultureInfo("en-US");

					while (await csv.ReadAsync()) {
						line++;
						var date = DateTime.ParseExact(csv["StartDate"], @"yyyy\-MM\-dd", culture);

						if (m_SkipPastRecords && date < lastMonday) { // Only store past records for this week
							continue;
						}

						DateTime start = date + TimeSpan.ParseExact(csv["StartTime"], @"hh\:mm", culture);
						DateTime end = date + TimeSpan.ParseExact(csv["EndTime"], @"hh\:mm", culture); // Under the assumption that nobody works overnight

						string[]? studentsets = csv["StudentSets"].Split(new[] { ", " }, StringSplitOptions.None);
						// Rooms often have " (0)" behind them. unknown reason.
						// Just remove them for now. This is the simplest way. We can't trim from the end, because multiple rooms may be listed and they will all have this suffix.
						string[]? room = csv["Room"].Replace(" (0)", "").Split(new[] { ", " }, StringSplitOptions.None);

						if (studentsets.Length == 1 && studentsets[0].Length == 0) {
							studentsets = null;
						}
						if (room.Length == 1 && room[0].Length == 0) {
							room = null;
						}

						ScheduleRecord record = new GLUScheduleRecord(
							activity: new ActivityInfo(csv["Activity"], GLUActivities.GetActivityFromAbbr(csv["Activity"])),
							start: start,
							end: end,
							studentSets: studentsets != null ? studentsets.Select(code => new StudentSetInfo(code)).ToArray() : Array.Empty<StudentSetInfo>(),
							staffMember: m_Teachers.GetRecordsFromAbbrs(m_Guild, csv["StaffMember"].Split(new[] { ", " }, StringSplitOptions.None)),
							room: room != null ? room.Select(code => new RoomInfo(code)).ToArray() : Array.Empty<RoomInfo>()
						);

						bool shouldMerge(ScheduleRecord mergeThis, out ScheduleRecord? into) {
							into = null;
							for (int i = schedule.Count - 1; i >= 0; i--) {
								ScheduleRecord item = schedule[i];
								if (item.StaffMember.Intersect(mergeThis.StaffMember).Any() ||
									item.StudentSets.Intersect(mergeThis.StudentSets).Any() ||
									item.Room.Intersect(mergeThis.Room).Any()) {
									if (item.StaffMember.SequenceEqual(mergeThis.StaffMember) &&
										item.StudentSets.SequenceEqual(mergeThis.StudentSets) &&
										item.Room.SequenceEqual(mergeThis.Room)) {
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
