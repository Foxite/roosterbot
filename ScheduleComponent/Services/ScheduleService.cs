﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using CsvHelper;
using RoosterBot;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Services {
	public class ScheduleService {
		private List<ScheduleRecord> m_Schedule;
		private TeacherNameService m_Teachers;
		private PropertyInfo m_SearchProperty;
		private string Name => m_SearchProperty.Name;

		public ScheduleService(TeacherNameService teachers, string searchProperty) {
			m_Schedule = new List<ScheduleRecord>();
			m_Teachers = teachers;
			m_SearchProperty = typeof(ScheduleRecord).GetProperty(searchProperty);
		}
		
		/// <summary>
		/// Loads a schedule into memory from a CSV file so that it can be accessed using this service.
		/// </summary>
		/// <param name="name">Should be the same as the property you're going to search from.</param>
		public async Task ReadScheduleCSV(string path) {
			Logger.Log(Discord.LogSeverity.Info, "ScheduleService", $"Schedule for {Name}: Loading CSV file from {path}");

			int line = 1;
			try {
				using (StreamReader reader = File.OpenText(path)) {
					using (CsvReader csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration() { Delimiter = "," })) {
						await csv.ReadAsync();
						csv.ReadHeader();

						Dictionary<string, ScheduleRecord> lastRecords = new Dictionary<string, ScheduleRecord>();
						PropertyInfo identifier = typeof(ScheduleRecord).GetProperty(m_SearchProperty.Name + "String");

						DateTime lastMonday = DateTime.Today.AddDays(-(int) DateTime.Today.DayOfWeek + 1); // + 1 because C# weeks start on Sunday (which is 0, and Monday is 1, etc. Saturday is 6)

						while (await csv.ReadAsync()) {
							int[] startDate = Array.ConvertAll(csv["StartDate"].Split('-'), item => int.Parse(item));
							int[] startTime = Array.ConvertAll(csv["StartTime"].Split(':'), item => int.Parse(item));
							int[] endTime = Array.ConvertAll(csv["EndTime"].Split(':'), item => int.Parse(item));
							DateTime start = new DateTime(startDate[0], startDate[1], startDate[2], startTime[0], startTime[1], 0);
							if (start.Date < lastMonday) {
								continue;
							}

							DateTime end = new DateTime(startDate[0], startDate[1], startDate[2], endTime[0], endTime[1], 0); // Under the assumption that nobody works overnight

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

							string key = (string) identifier.GetValue(record);
							if (lastRecords.TryGetValue(key, out ScheduleRecord lastRecord) &&
								record.Activity == lastRecord.Activity &&
								record.Start.Date == lastRecord.Start.Date &&
								record.StudentSetsString == lastRecord.StudentSetsString &&
								record.StaffMemberString == lastRecord.StaffMemberString &&
								record.RoomString == lastRecord.RoomString) {
								lastRecord.BreakStart = lastRecord.End;
								lastRecord.BreakEnd = record.Start;
								lastRecord.End = record.End;
							} else {
								lastRecords[key] = record;
								m_Schedule.Add(record);
							}
						}
					}
				}
			} catch (Exception e) {
				Logger.Log(Discord.LogSeverity.Critical, "ScheduleService", "The following exception was thrown while loading the CSV at \"" + path + "\" on line " + line, e);
				throw;
			}
			Logger.Log(Discord.LogSeverity.Info, "ScheduleService", $"Successfully loaded CSV file {Path.GetFileName(path)} into schedule for {Name}");
		}
		
		/// <returns>null if the class has no activity currently ongoing.</returns>
		public ScheduleRecord GetCurrentRecord(IdentifierInfo identifier) {
			long ticksNow = DateTime.Now.Ticks;
			bool sawRecordForClass = false;

			foreach (ScheduleRecord record in m_Schedule) {
				if (identifier.Matches(record)) {
					sawRecordForClass = true;
					if (ticksNow > record.Start.Ticks && ticksNow < record.End.Ticks) {
						return record;
					} else if (ticksNow < record.Start.Ticks) {
						return null;
					}
				}
			}
			if (sawRecordForClass) {
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule for {Name}");
			} else {
				throw new ScheduleNotFoundException($"The class {identifier} does not exist in schedule for {Name}.");
			}
		}

		public ScheduleRecord GetNextRecord(IdentifierInfo identifier) {
			long ticksNow = DateTime.Now.Ticks;
			bool sawRecordForClass = false;

			foreach (ScheduleRecord record in m_Schedule) {
				if (identifier.Matches(record)) {
					sawRecordForClass = true;
					if (ticksNow < record.Start.Ticks) {
						return record;
					}
				}
			}
			if (sawRecordForClass) {
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule for {Name}");
			} else {
				throw new ScheduleNotFoundException($"The class {identifier} does not exist in schedule for {Name}.");
			}
		}

		public ScheduleRecord GetRecordAfter(IdentifierInfo identifier, ScheduleRecord givenRecord) {
			long ticksNow = givenRecord.Start.Ticks; // This is probably not the best solution, but it should totally work. This allows us to simply
													 //  reuse the code from GetNextRecord().
			bool sawRecordForClass = false;
			
			foreach (ScheduleRecord record in m_Schedule) {
				if (identifier.Matches(record)) {
					sawRecordForClass = true;
					if (ticksNow < record.Start.Ticks) {
						return record;
					}
				}
			}
			if (sawRecordForClass) {
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule for {Name}");
			} else {
				throw new ScheduleNotFoundException($"The class {identifier} does not exist in schedule for {Name}.");
			}
		}

		public ScheduleRecord[] GetSchedulesForDay(IdentifierInfo identifier, DayOfWeek day, bool includeToday) {
			List<ScheduleRecord> records = new List<ScheduleRecord>();
			bool sawRecordForClass = false;
			bool sawRecordAfterTarget = false;
			DateTime targetDate;

			// https://stackoverflow.com/a/6346190/3141917
			if (includeToday) {
				// Get the next {day} including today
				targetDate = DateTime.Today.AddDays(((int) day - (int) DateTime.Today.DayOfWeek + 7) % 7);
			} else {
				// Get the next {day} after today
				targetDate = DateTime.Today.AddDays(1 + ((int) day - (int) DateTime.Today.AddDays(1).DayOfWeek + 7) % 7);
			}

			foreach (ScheduleRecord record in m_Schedule) {
				if (identifier.Matches(record)) {
					sawRecordForClass = true;
					if (record.Start.Date == targetDate) {
						records.Add(record);
					} else if (record.Start.Date > targetDate) {
						sawRecordAfterTarget = true;
						break;
					}
				}
			}

			if (records.Count == 0) {
				if (!sawRecordForClass) {
					throw new ScheduleNotFoundException($"The class {identifier} does not exist in schedule for {Name}.");
				} else if (!sawRecordAfterTarget) {
					throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule for {Name}");
				}
			}
			return records.ToArray();
		}

		/// <summary>
		/// Gets all the days in a week that have at least 1 record on those days.
		/// </summary>
		public AvailabilityInfo[] GetWeekAvailability(IdentifierInfo identifier, int weeksFromNow = 0) {
			ScheduleRecord[] weekRecords = GetWeekRecords(identifier, weeksFromNow);
			IEnumerable<IGrouping<DayOfWeek, ScheduleRecord>> recordsByDay = weekRecords.GroupBy(record => record.Start.DayOfWeek);
			var ret = new AvailabilityInfo[recordsByDay.Count()];
			int i = 0;
			foreach (IGrouping<DayOfWeek, ScheduleRecord> day in recordsByDay) {
				ret[i] = new AvailabilityInfo(day.First().Start, day.Last().End);
				i++;
			}
			
			return ret;
		}

		public ScheduleRecord[] GetWeekRecords(IdentifierInfo identifier, int weeksFromNow = 0) {
			DateTime targetFirstDate =
				DateTime.Today
				.AddDays(-(int) DateTime.Today.DayOfWeek + 1) // First date in this week; + 1 because DayOfWeek.Sunday == 0, and Monday == 1
				.AddDays(7 * weeksFromNow); // First date in the week n weeks from now
			DateTime targetLastDate = targetFirstDate.AddDays(4); // Friday

			List<ScheduleRecord> weekRecords = new List<ScheduleRecord>();

			foreach (ScheduleRecord record in m_Schedule) {
				if (identifier.Matches(record)) {
					if (record.Start.Date >= targetFirstDate) {
						if (record.Start.Date > targetLastDate) {
							break;
						}
						weekRecords.Add(record);
					}
				}
			}
			return weekRecords.ToArray();
		}
	}

	[Serializable]
	public class ScheduleNotFoundException : Exception {
		public ScheduleNotFoundException() { }
		public ScheduleNotFoundException(string message) : base(message) { }
		public ScheduleNotFoundException(string message, Exception innerException) : base(message, innerException) { }
		protected ScheduleNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class RecordsOutdatedException : Exception {
		public RecordsOutdatedException() { }
		public RecordsOutdatedException(string message) : base(message) { }
		public RecordsOutdatedException(string message, Exception innerException) : base(message, innerException) { }
		protected RecordsOutdatedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
