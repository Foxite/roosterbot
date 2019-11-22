using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.Schedule.GLU {
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
				using (CsvReader csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration() { Delimiter = "," })) {
					await csv.ReadAsync();
					csv.ReadHeader();

					DateTime lastMonday = DateTime.Today.AddDays(-(int) DateTime.Today.DayOfWeek + 1); // + 1 because C# weeks start on Sunday (which is 0, and Monday is 1, etc. Saturday is 6)

					schedule = new List<ScheduleRecord>();
					CultureInfo culture = CultureInfo.GetCultureInfo("en-US");

					while (await csv.ReadAsync()) {
						line++;
						DateTime date = DateTime.ParseExact(csv["StartDate"], @"yyyy\-MM\-dd", culture);

						if (m_SkipPastRecords && date < lastMonday) { // Only store past records for this week
							continue;
						}

						DateTime start = date + TimeSpan.ParseExact(csv["StartTime"], @"hh\:mm", culture);
						DateTime end = date + TimeSpan.ParseExact(csv["EndTime"], @"hh\:mm", culture); // Under the assumption that nobody works overnight

						ScheduleRecord record = new GLUScheduleRecord() {
							Activity = new ActivityInfo(csv["Activity"], GLUActivities.GetActivityFromAbbr(csv["Activity"])),
							StaffMember = m_Teachers.GetRecordsFromAbbrs(m_Guild, csv["StaffMember"].Split(new[] { ", " }, StringSplitOptions.None)),
							StudentSets = csv["StudentSets"].Split(new[] { ", " }, StringSplitOptions.None).Select(code => new StudentSetInfo() { ClassName = code }).ToArray(),
							// Rooms often have " (0)" behind them. unknown reason.
							// Just remove them for now. This is the simplest way. We can't trim from the end, because multiple rooms may be listed and they will all have this suffix.
							Room = csv["Room"].Replace(" (0)", "").Split(new[] { ", " }, StringSplitOptions.None).Select(code => new RoomInfo() { Room = code }).ToArray(),
							Start = start,
							End = end
						};

						bool shouldMerge(ScheduleRecord mergeThis, out ScheduleRecord result) {
							// Disabled because it's broken until I figure out how to fix it.
							// The old code would merge records even if there's a record in between them. I only want to do this if there's a gap in the schedule.
							/*foreach (ScheduleRecord tryMerge in schedule) {
								if (tryMerge.Start.Date == mergeThis.Start.Date &&
									tryMerge.Activity == mergeThis.Activity &&
									tryMerge.StudentSetsString == mergeThis.StudentSetsString &&
									tryMerge.StaffMemberString == mergeThis.StaffMemberString &&
									tryMerge.RoomString == mergeThis.RoomString) {
									result = mergeThis;
									return true;
								}
							}*/
							result = null;
							return false;
						}

						if (shouldMerge(record, out ScheduleRecord mergeInto)) {
							mergeInto.BreakStart = mergeInto.End;
							mergeInto.BreakEnd = record.Start;
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
