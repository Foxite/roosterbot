﻿using CsvHelper;
using RoosterBot;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GLUScheduleComponent {
	public class GLUScheduleReader : ScheduleReaderBase {
		private readonly string m_Path;
		private readonly TeacherNameService m_Teachers;
		private readonly ulong m_Guild;

		public GLUScheduleReader(string path, TeacherNameService teachers, ulong guild) {
			m_Path = path;
			m_Teachers = teachers;
			m_Guild = guild;
		}

		public async override Task<List<ScheduleRecord>> GetSchedule() {
			Logger.Log(Discord.LogSeverity.Info, "GLUScheduleReader", $"Loading CSV file from {m_Path}");

			int line = 1;
			try {
				List<ScheduleRecord> schedule;
				using (StreamReader reader = File.OpenText(m_Path))
				using (CsvReader csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration() { Delimiter = "," })) {
					await csv.ReadAsync();
					csv.ReadHeader();

					Dictionary<string, ScheduleRecord> lastRecords = new Dictionary<string, ScheduleRecord>();

					DateTime lastMonday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1); // + 1 because C# weeks start on Sunday (which is 0, and Monday is 1, etc. Saturday is 6)

					schedule = new List<ScheduleRecord>();
					CultureInfo culture = new CultureInfo("en-US");

					while (await csv.ReadAsync()) {
						line++;
						DateTime date = DateTime.ParseExact(csv["StartDate"], @"yyyy\-MM\-dd", culture);

						if (date < lastMonday) { // Only store past records for this week
							continue;
						}

						DateTime start = date + TimeSpan.ParseExact(csv["StartTime"], @"hh\:mm", culture);
						DateTime end = date + TimeSpan.ParseExact(csv["EndTime"], @"hh\:mm", culture); // Under the assumption that nobody works overnight

						ScheduleRecord record = new ScheduleRecord() {
							Activity = csv["Activity"],
							StaffMember = m_Teachers.GetRecordsFromAbbrs(m_Guild, csv["StaffMember"].Split(new[] { ", " }, StringSplitOptions.None)),
							StudentSets = csv["StudentSets"].Split(new[] { ", " }, StringSplitOptions.None).Select(code => new StudentSetInfo() { ClassName = code }).ToArray(),
							// Rooms often have " (0)" behind them. unknown reason.
							// Just remove them for now. This is the simplest way. We can't trim from the end, because multiple rooms may be listed and they will all have this suffix.
							Room = csv["Room"].Replace(" (0)", "").Split(new[] { ", " }, StringSplitOptions.None).Select(code => new RoomInfo() { Room = code }).ToArray(),
							Start = start,
							End = end
						};

						if (lastRecords.TryGetValue(record.Activity, out ScheduleRecord lastRecord) &&
							record.Start.Date == lastRecord.Start.Date &&
							record.StudentSetsString == lastRecord.StudentSetsString &&
							record.StaffMemberString == lastRecord.StaffMemberString &&
							record.RoomString == lastRecord.RoomString) {
							// Note: This does not support records with multiple breaks. If that happens, it will result in only the last break being displayed.
							lastRecord.BreakStart = lastRecord.End;
							lastRecord.BreakEnd = record.Start;
							lastRecord.End = record.End;
						} else {
							lastRecords[record.Activity] = record;
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
