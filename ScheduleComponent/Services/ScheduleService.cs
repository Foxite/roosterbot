﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Services {
	public class ScheduleService : GuildSpecificInfo {
		private List<ScheduleRecord> m_Schedule;
		private string m_Name;

		private ScheduleService(ulong[] allowedGuilds) : base(allowedGuilds) { }

		/// <param name="name">Used in logging. Does not affect anything else.</param>
		public static async Task<ScheduleService> CreateAsync(string name, ScheduleReaderBase reader, ulong[] allowedGuildIds) {
			ScheduleService service = new ScheduleService(allowedGuildIds) {
				m_Name = name,
				m_Schedule = await reader.GetSchedule() // Unfortunately we can't have async constructors (for good reasons), so this'll do.
			};
			return service;
		}

		/// <returns>null if the class has no activity currently ongoing.</returns>
		public ScheduleRecord GetCurrentRecord(IdentifierInfo identifier) {
			DateTime now = DateTime.Now;
			bool sawRecordForClass = false;

			foreach (ScheduleRecord record in m_Schedule) {
				if (identifier.Matches(record)) {
					sawRecordForClass = true;
					if (now > record.Start && now < record.End) {
						return record;
					} else if (now < record.Start) {
						return null;
					}
				}
			}
			if (sawRecordForClass) {
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {m_Name}");
			} else {
				throw new IdentifierNotFoundException($"The class {identifier} does not exist in schedule {m_Name}.");
			}
		}

		public ScheduleRecord GetRecordAfterTimeSpan(IdentifierInfo identifier, TimeSpan timespan) {
			DateTime target = DateTime.Now + timespan;
			bool sawRecordForClass = false;

			foreach (ScheduleRecord record in m_Schedule) {
				if (identifier.Matches(record)) {
					sawRecordForClass = true;
					if (record.Start < target && record.End > target) {
						return record;
					} else if (record.Start > target) {
						return null;
					}
				}
			}
			if (sawRecordForClass) {
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {m_Name}");
			} else {
				throw new IdentifierNotFoundException($"The class {identifier} does not exist in schedule {m_Name}.");
			}
		}

		public ScheduleRecord GetNextRecord(IdentifierInfo identifier) {
			DateTime now = DateTime.Now;
			bool sawRecordForClass = false;

			foreach (ScheduleRecord record in m_Schedule) {
				if (identifier.Matches(record)) {
					sawRecordForClass = true;
					if (now < record.Start) {
						return record;
					}
				}
			}
			if (sawRecordForClass) {
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {m_Name}");
			} else {
				throw new IdentifierNotFoundException($"The class {identifier} does not exist in schedule {m_Name}.");
			}
		}

		public ScheduleRecord GetRecordAfter(IdentifierInfo identifier, ScheduleRecord givenRecord) {
			bool sawRecordForClass = false;
			bool sawGivenRecord = false;
			
			foreach (ScheduleRecord record in m_Schedule) {
				if (sawGivenRecord) {
					if (identifier.Matches(record)) {
						return record;
					}
				} else {
					if (identifier.Matches(record)) {
						sawRecordForClass = true;
					}
					if (givenRecord == record) {
						sawGivenRecord = true;
					}
				}
			}
			if (sawRecordForClass) {
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {m_Name}");
			} else {
				throw new IdentifierNotFoundException($"The class {identifier} does not exist in schedule {m_Name}.");
			}
		}

		public ScheduleRecord[] GetSchedulesForDate(IdentifierInfo identifier, DateTime date) {
			List<ScheduleRecord> records = new List<ScheduleRecord>();
			bool sawRecordForClass = false;
			bool sawRecordAfterTarget = false;

			foreach (ScheduleRecord record in m_Schedule) {
				if (identifier.Matches(record)) {
					sawRecordForClass = true;
					if (record.Start.Date == date) {
						records.Add(record);
					} else if (record.Start.Date > date) {
						sawRecordAfterTarget = true;
						break;
					}
				}
			}

			if (records.Count == 0) {
				if (!sawRecordForClass) {
					throw new IdentifierNotFoundException($"The class {identifier} does not exist in schedule {m_Name}.");
				} else if (!sawRecordAfterTarget) {
					throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {m_Name}");
				}
			}
			return records.ToArray();
		}

		/// <summary>
		/// Gets all the days in a week that have at least 1 record on those days.
		/// </summary>
		public AvailabilityInfo[] GetWeekAvailability(IdentifierInfo identifier, int weeksFromNow = 0) {
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

			IEnumerable<IGrouping<DayOfWeek, ScheduleRecord>> recordsByDay = weekRecords.GroupBy(record => record.Start.DayOfWeek);
			var ret = new AvailabilityInfo[recordsByDay.Count()];
			int i = 0;
			foreach (IGrouping<DayOfWeek, ScheduleRecord> day in recordsByDay) {
				ret[i] = new AvailabilityInfo(day.First().Start, day.Last().End);
				i++;
			}
			
			return ret;
		}
	}

	[Serializable]
	public class IdentifierNotFoundException : Exception {
		public IdentifierNotFoundException() { }
		public IdentifierNotFoundException(string message) : base(message) { }
		public IdentifierNotFoundException(string message, Exception innerException) : base(message, innerException) { }
		protected IdentifierNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	[Serializable]
	public class RecordsOutdatedException : Exception {
		public RecordsOutdatedException() { }
		public RecordsOutdatedException(string message) : base(message) { }
		public RecordsOutdatedException(string message, Exception innerException) : base(message, innerException) { }
		protected RecordsOutdatedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
