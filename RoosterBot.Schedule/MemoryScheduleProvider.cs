using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	/// <summary>
	/// A schedule provider that loads all records into memory from a schedule reader.
	/// </summary>
	public class MemoryScheduleProvider : ScheduleProvider {
		private readonly Dictionary<DateTime, IEnumerable<ScheduleRecord>> m_Schedule;
		private readonly DateTime m_ScheduleBegin;
		private readonly DateTime m_ScheduleEnd;
		private readonly string m_Name;

		/// <param name="name">Used in logging. Does not affect anything else.</param>
		public MemoryScheduleProvider(string name, ScheduleReader reader, IEnumerable<SnowflakeReference> allowedChannels) : base(allowedChannels) {
			m_Name = name;
			List<ScheduleRecord> list = reader.GetSchedule();
			m_ScheduleBegin = list[0].Start.Date;
			m_ScheduleEnd = list[list.Count - 1].End.Date;
			m_Schedule = list
				.GroupBy(record => record.Start.Date)
				.ToDictionary(
					grp => grp.Key,
					grp => (IEnumerable<ScheduleRecord>) grp
				);
		}

		public override Task<ScheduleRecord?> GetRecordAtDateTimeAsync(IdentifierInfo identifier, DateTime target) => Task.Run(() => {
			bool sawRecordForClass = false;

			foreach (ScheduleRecord record in RecordsFrom(target)) {
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
		});

		public override Task<ScheduleRecord> GetRecordAfterDateTimeAsync(IdentifierInfo identifier, DateTime target) => Task.Run(() => {
			bool sawRecordForClass = false;

			foreach (ScheduleRecord record in RecordsFrom(target)) {
				if (identifier.Matches(record)) {
					if (record.End > target) {
						return record;
					} else {
						sawRecordForClass = true;
					}
				}
			}
			if (sawRecordForClass) {
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {m_Name}");
			} else {
				throw new IdentifierNotFoundException($"The class {identifier} does not exist in schedule {m_Name}.");
			}
		});

		public override Task<ScheduleRecord[]> GetSchedulesForDateAsync(IdentifierInfo identifier, DateTime date) => Task.Run(() => {
			bool sawRecordForClass = false;
			bool sawRecordAfterTarget = false;

			if (m_Schedule.TryGetValue(date.Date, out var records)) {
				var ret = new List<ScheduleRecord>();
				foreach (ScheduleRecord record in records) {
					if (identifier.Matches(record)) {
						sawRecordForClass = true;
						if (record.Start.Date == date) {
							ret.Add(record);
						} else if (record.Start.Date > date) {
							sawRecordAfterTarget = true;
							break;
						}
					}
				}
				if (ret.Count == 0) {
					if (!sawRecordForClass) {
						throw new IdentifierNotFoundException($"The class {identifier} does not exist in schedule {m_Name}.");
					} else if (!sawRecordAfterTarget) {
						throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {m_Name}");
					}
				}
				return ret.ToArray();
			} else {
				return Array.Empty<ScheduleRecord>();
			}
		});

		public override Task<ScheduleRecord[]> GetWeekRecordsAsync(IdentifierInfo identifier, int weeksFromNow = 0) => Task.Run(() => {
			DateTime targetFirstDate =
				DateTime.Today
				.AddDays(-(int) DateTime.Today.DayOfWeek + 1) // First date in this week; + 1 because DayOfWeek.Sunday == 0, and Monday == 1
				.AddDays(7 * weeksFromNow); // First date in the week n weeks from now
			DateTime targetLastDate = targetFirstDate.AddDays(4); // Friday

			var weekRecords = new List<ScheduleRecord>();

			foreach (ScheduleRecord record in RecordsFrom(targetFirstDate)) {
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
		});

		public override Task<ScheduleRecord> GetRecordBeforeDateTimeAsync(IdentifierInfo identifier, DateTime target) => Task.Run(() => {
			ScheduleRecord? lastMatch = null;

			IEnumerable<ScheduleRecord> sequence() {
				for (DateTime date = target.Date; date > m_ScheduleBegin; date = date.AddDays(-1)) {
					if (m_Schedule.TryGetValue(date, out var scheduleDate)) {
						foreach (ScheduleRecord yieldRecord in scheduleDate.Reverse()) {
							yield return yieldRecord;
						}
					}
				}
			}

			bool sawMatch = false;
			bool sawAny = false;
			foreach (ScheduleRecord record in sequence()) {
				sawAny = true;
				if (identifier.Matches(record)) {
					sawMatch = true;

					if (record.End < target) {
						lastMatch = record;
					}
				}
			}
			return lastMatch ?? throw (sawMatch || !sawAny
				? (Exception) new IdentifierNotFoundException($"Identifier {identifier} not found in schedule {m_Name}")
				: new RecordsOutdatedException("Cannot look as far back as " + target.ToString())
			);
		});

		private IEnumerable<ScheduleRecord> RecordsFrom(DateTime target) {
			for (DateTime date = target.Date; date < m_ScheduleEnd; date = date.AddDays(1).Date) {
				if (m_Schedule.TryGetValue(date, out var dateSchedules)) {
					foreach (ScheduleRecord yieldRecord in dateSchedules) {
						yield return yieldRecord;
					}
				}
			}
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
