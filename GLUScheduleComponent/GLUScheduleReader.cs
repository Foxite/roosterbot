using CsvHelper;
using RoosterBot;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GLUScheduleComponent {
	public class GLUScheduleReader : ScheduleReaderBase {
		private readonly string m_Path;
		private TeacherNameService m_Teachers;

		public GLUScheduleReader(string path, TeacherNameService teachers) {
			m_Path = path;
			m_Teachers = teachers;
		}

		/// <summary>
		/// Loads a schedule into memory from a CSV file so that it can be accessed using this service.
		/// </summary>
		/// <param name="name">Should be the same as the property you're going to search from.</param>
		public async override Task<List<ScheduleRecord>> GetSchedule(string name) {
			Logger.Log(Discord.LogSeverity.Info, "GLUScheduleReader", $"Loading CSV file from {m_Path}");

			int line = 1;
			try {
				List<ScheduleRecord> schedule;
				using (StreamReader reader = File.OpenText(m_Path))
				using (CsvReader csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration() { Delimiter = "," })) {
					await csv.ReadAsync();
					csv.ReadHeader();

					Dictionary<IdentifierInfo[], ScheduleRecord> lastRecords = new Dictionary<IdentifierInfo[], ScheduleRecord>();
					PropertyInfo identifier = typeof(ScheduleRecord).GetProperty(name);

					DateTime lastMonday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1); // + 1 because C# weeks start on Sunday (which is 0, and Monday is 1, etc. Saturday is 6)

					schedule = new List<ScheduleRecord>();
					CultureInfo culture = new CultureInfo("en-US");

					while (await csv.ReadAsync()) {
						DateTime date = DateTime.ParseExact(csv["StartDate"], @"yyyy\-MM\-dd", culture);

						if (date < lastMonday) { // Only store past records for this week
							continue;
						}

						DateTime start = date + TimeSpan.ParseExact(csv["StartTime"], @"hh\:mm", culture);
						DateTime end = date + TimeSpan.ParseExact(csv["EndTime"], @"hh\:mm", culture); // Under the assumption that nobody works overnight

						ScheduleRecord record = new ScheduleRecord() {
							Activity = csv["Activity"],
							StaffMember = m_Teachers.GetRecordsFromAbbrs(csv["StaffMember"].Split(new[] { ", " }, StringSplitOptions.None)),
							StudentSets = csv["StudentSets"].Split(new[] { ", " }, StringSplitOptions.None).Select(code => new StudentSetInfo() { ClassName = code }).ToArray(),
							// Rooms often have " (0)" behind them. unknown reason.
							// Just remove them for now. This is the simplest way. We can't trim from the end, because multiple rooms may be listed and they will all have this suffix.
							Room = csv["Room"].Replace(" (0)", "").Split(new[] { ", " }, StringSplitOptions.None).Select(code => new RoomInfo() { Room = code }).ToArray(),
							Start = start,
							End = end
						};

						IdentifierInfo[] key = (IdentifierInfo[])identifier.GetValue(record);
						if (lastRecords.TryGetValue(key, out ScheduleRecord lastRecord) &&
							record.Activity == lastRecord.Activity &&
							record.Start.Date == lastRecord.Start.Date &&
							record.StudentSetsString == lastRecord.StudentSetsString &&
							record.StaffMember == lastRecord.StaffMember &&
							record.RoomString == lastRecord.RoomString) {
							lastRecord.BreakStart = lastRecord.End;
							lastRecord.BreakEnd = record.Start;
							lastRecord.End = record.End;
						} else {
							lastRecords[key] = record;
							schedule.Add(record);
						}
					}
				}
				return schedule;
			} catch (Exception e) {
				Logger.Log(Discord.LogSeverity.Critical, "GLUScheduleReader", $"The following exception was thrown while loading the CSV at \"{m_Path}\" on line {line}", e);
				throw;
			}
		}
	}
}
