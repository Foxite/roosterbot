﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using System.Runtime.Serialization;
using RoosterBot;

namespace ScheduleComponent.Services {
	public class ScheduleService {
		private ConcurrentDictionary<string, List<ScheduleRecord>> m_Schedules;

		public ScheduleService() {
			m_Schedules = new ConcurrentDictionary<string, List<ScheduleRecord>>();
		}

		/// <summary>
		/// Clears all schedules.
		/// </summary>
		public void Reset() {
			m_Schedules.Clear();
		}

		/// <summary>
		/// Loads a schedule into memory from a CSV file so that it can be accessed using this service. It is safe to execute this function in parallel.
		/// </summary>
		/// <param name="name">Should be the same as the property you're going to search from.</param>
		public async Task ReadScheduleCSV(string name, string path) {
			Logger.Log(Discord.LogSeverity.Info, "ScheduleService", $"Loading schedule CSV file {path} into {name}");
			List<ScheduleRecord> records = new List<ScheduleRecord>();
			if (!m_Schedules.TryAdd(name, records)) {
				throw new ArgumentException($"A schedule CSV file has already been loaded by the name {name}.");
			}

			using (StreamReader reader = File.OpenText(path)) {
				CsvReader csv = new CsvReader(reader);
				await csv.ReadAsync();
				csv.ReadHeader();
				while (await csv.ReadAsync()) {
					ScheduleRecord record = new ScheduleRecord() {
						Activity = csv["Activity"],
						StaffMember = csv["StaffMember"],
						StudentSets = csv["StudentSets"],
						// Rooms often have " (0)" behind them. unknown reason.
						// Just remove them for now. This is the simplest way. We can't trim from the end, because multiple rooms may be listed and they will all have this suffix.
						Room = csv["Room"].Replace(" (0)", ""),
						Duration = csv["Duration"]
					};
					
					int[] startDate = Array.ConvertAll(csv["StartDate"].Split('-'), item => int.Parse(item));
					int[] startTime = Array.ConvertAll(csv["StartTime"].Split(':'), item => int.Parse(item));
					int[] endTime   = Array.ConvertAll(csv["EndTime"].Split(':'), item => int.Parse(item));
					record.Start = new DateTime(startDate[0], startDate[1], startDate[2], startTime[0], startTime[1], 0);
					record.End   = new DateTime(startDate[0], startDate[1], startDate[2],   endTime[0],   endTime[1], 0); // Under the assumption that nobody works overnight

					records.Add(record);
				}
			}
			Logger.Log(Discord.LogSeverity.Info, "ScheduleService", $"Successfully loaded schedule CSV file {path} into {name}");
		}
		
		/// <returns>null if the class has no activity currently ongoing.</returns>
		public ScheduleRecord GetCurrentRecord(string schedule, string identifier) {
			long ticksNow = DateTime.Now.Ticks;
			bool sawRecordForClass = false;

			foreach (ScheduleRecord record in m_Schedules[schedule]) {
				if (((string) record.GetType().GetProperty(schedule).GetValue(record)).Contains(identifier)) {
					sawRecordForClass = true;
					if (ticksNow > record.Start.Ticks && ticksNow < record.End.Ticks) {
						return record;
					} else if (ticksNow < record.Start.Ticks) {
						return null;
					}
				}
			}
			if (sawRecordForClass) {
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {schedule}");
			} else {
				throw new ScheduleNotFoundException($"The class {identifier} does not exist in schedule {schedule}.");
			}
		}

		public ScheduleRecord GetNextRecord(string schedule, string identifier) {
			long ticksNow = DateTime.Now.Ticks;
			bool sawRecordForClass = false;

			foreach (ScheduleRecord record in m_Schedules[schedule]) {
				if (((string) record.GetType().GetProperty(schedule).GetValue(record)).Contains(identifier)) {
					sawRecordForClass = true;
					if (ticksNow < record.Start.Ticks) {
						return record;
					}
				}
			}
			if (sawRecordForClass) {
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {schedule}");
			} else {
				throw new ScheduleNotFoundException($"The class {identifier} does not exist in schedule {schedule}.");
			}
		}

		public ScheduleRecord GetFirstRecordForDay(string schedule, string identifier, DayOfWeek day) {
			bool sawRecordForClass = false;
			// Get the next {day} after today
			// https://stackoverflow.com/a/6346190/3141917
			DateTime targetDate = DateTime.Today.AddDays(1 + ((int) day - (int) DateTime.Today.AddDays(1).DayOfWeek + 7) % 7);

			foreach (ScheduleRecord record in m_Schedules[schedule]) {
				if (((string) record.GetType().GetProperty(schedule).GetValue(record)).Contains(identifier)) {
					sawRecordForClass = true;
					if (record.Start.Date == targetDate) {
						return record;
					}
				} else if (record.Start.Date > targetDate) {
					if (sawRecordForClass) {
						return null;
					} else {
						throw new ScheduleNotFoundException($"The class {identifier} does not exist in schedule {schedule}.");
					}
				}
			}
			if (sawRecordForClass) {
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {schedule}");
			} else {
				throw new ScheduleNotFoundException($"The class {identifier} does not exist in schedule {schedule}.");
			}
		}

		public ScheduleRecord GetRecordAfter(string schedule, ScheduleRecord givenRecord) {
			long ticksNow = givenRecord.Start.Ticks; // This is probably not the best solution, but it should totally work. This allows us to simply
												//  reuse the code from GetNextRecord().
			bool sawRecordForClass = false;
			string identifier = (string) givenRecord.GetType().GetProperty(schedule).GetValue(givenRecord);

			foreach (ScheduleRecord record in m_Schedules[schedule]) {
				if (((string) record.GetType().GetProperty(schedule).GetValue(record)).Contains(identifier)) {
					sawRecordForClass = true;
					if (ticksNow < record.Start.Ticks) {
						return record;
					}
				}
			}
			if (sawRecordForClass) {
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {schedule}");
			} else {
				throw new ScheduleNotFoundException($"The class {identifier} does not exist in schedule {schedule}.");
			}
		}
	}

	public class ScheduleRecord : ICloneable {
		public string	Activity { get; set; }
		public string	Duration { get; set; }
		public DateTime	Start { get; set; }
		public DateTime End { get; set; }
		public string	StaffMember { get; set; }
		public string	Room { get; set; }
		public string	StudentSets { get; set; }
		
		public object Clone() {
			return new ScheduleRecord() {
				Activity = Activity,
				Duration = Duration,
				Start = Start,
				End = End,
				StaffMember = StaffMember,
				Room = Room,
				StudentSets = StudentSets
			};
		}
	}

	public class ScheduleNotFoundException : Exception {
		public ScheduleNotFoundException() { }
		public ScheduleNotFoundException(string message) : base(message) { }
		public ScheduleNotFoundException(string message, Exception innerException) : base(message, innerException) { }
		protected ScheduleNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	public class RecordsOutdatedException : Exception {
		public RecordsOutdatedException() { }
		public RecordsOutdatedException(string message) : base(message) { }
		public RecordsOutdatedException(string message, Exception innerException) : base(message, innerException) { }
		protected RecordsOutdatedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}