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

					Dictionary<ActivityInfo, ScheduleRecord> lastRecords = new Dictionary<ActivityInfo, ScheduleRecord>();

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

						ScheduleRecord record = new GLUScheduleRecord(
							activity:  new ActivityInfo(csv["Activity"], GLUActivities.GetActivityFromAbbr(csv["Activity"])),
							staffMember: m_Teachers.GetRecordsFromAbbrs(m_Guild, csv["StaffMember"].Split(new[] { ", " }, StringSplitOptions.None)),
							studentSets: csv["StudentSets"].Split(new[] { ", " }, StringSplitOptions.None).Select(code => new StudentSetInfo(code)).ToArray(),
							// Rooms often have " (0)" behind them. unknown reason.
							// Just remove them for now. This is the simplest way. We can't trim from the end, because multiple rooms may be listed and they will all have this suffix.
							room: csv["Room"].Replace(" (0)", "").Split(new[] { ", " }, StringSplitOptions.None).Select(code => new RoomInfo(code)).ToArray(),
							start: start,
							end: end
						);

						if (lastRecords.TryGetValue(record.Activity, out ScheduleRecord? lastRecord) &&
							record.Start.Date == lastRecord.Start.Date &&
							record.StudentSets.SequenceEqual(lastRecord.StudentSets) &&
							record.StaffMember.SequenceEqual(lastRecord.StaffMember) &&
							record.Room.SequenceEqual(lastRecord.Room)) {
							// Note: This does not support records with multiple breaks. If that happens, it will result in only the last break being displayed.
							lastRecord.Break = new BreakTime(lastRecord.End, record.Start);
							lastRecord.End = record.End;
						} else {
							lastRecords[record.Activity] = record;
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
