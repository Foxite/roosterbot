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
		private Dictionary<DateTime, IEnumerable<ScheduleRecord>> m_Schedule;
		private HashSet<IdentifierInfo> m_PresentIdentifiers;
		private DateTime m_Begin;
		private readonly string m_Name;
		private readonly ScheduleReader m_Reader;

		/// <summary>
		/// The date of the last item accessible by this MemoryScheduleProvider. Trying to get any item after this point will result in a <see cref="RecordsOutdatedException"/>.
		/// </summary>
		public DateTime End { get; private set; }

		/// <param name="name">Used in logging. Does not affect anything else.</param>
		public MemoryScheduleProvider(string name, ScheduleReader reader, IReadOnlyCollection<SnowflakeReference> allowedChannels) : base(allowedChannels) {
			m_Name = name;
			m_Reader = reader;

			// The Reload function is called immediately after this, which sets these fields to something other than null
			m_Schedule = null!;
			m_PresentIdentifiers = null!;

			Reload();
		}

		private void Reload() {
			IReadOnlyList<ScheduleRecord> list = m_Reader.GetSchedule();
			m_Begin = list[0].Start.Date;
			End = list[list.Count - 1].End.Date;
			m_Schedule = list
				.GroupBy(record => record.Start.Date)
				.ToDictionary(
					grp => grp.Key,
					grp => (IEnumerable<ScheduleRecord>) grp
				);
			m_PresentIdentifiers = new HashSet<IdentifierInfo>(m_Schedule.Values.SelectMany(item => item).SelectMany(item => item.InvolvedIdentifiers).Distinct());
		}

		private void EnsureIdentifierPresent(IdentifierInfo identifier) {
			if (!m_PresentIdentifiers.Contains(identifier)) {
				throw new IdentifierNotFoundException($"The identifier {identifier} does not exist in schedule {m_Name}.");
			}
		}

		public override Task<ScheduleRecord?> GetRecordAtDateTimeAsync(IdentifierInfo identifier, DateTime target) => Task.Run(() => {
			EnsureIdentifierPresent(identifier);

			foreach (ScheduleRecord record in RecordsFrom(target)) {
				if (identifier.Matches(record)) {
					if (record.Start < target && record.End > target) {
						return record;
					} else if (record.Start > target) {
						return null;
					}
				}
			}
			throw new RecordsOutdatedException($"Records outdated for identifier {identifier} in schedule {m_Name}");
		});

		public override Task<ScheduleRecord> GetRecordAfterDateTimeAsync(IdentifierInfo identifier, DateTime target) => Task.Run(() => {
			EnsureIdentifierPresent(identifier);

			foreach (ScheduleRecord record in RecordsFrom(target)) {
				if (identifier.Matches(record) && record.Start > target) {
					return record;
				}
			}
			throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {m_Name}");
		});

		public override Task<ScheduleRecord[]> GetSchedulesForDateAsync(IdentifierInfo identifier, DateTime date) => Task.Run(() => {
			EnsureIdentifierPresent(identifier);

			if (m_Schedule.TryGetValue(date.Date, out var records)) {
				var ret = new List<ScheduleRecord>();
				foreach (ScheduleRecord record in records) {
					if (identifier.Matches(record)) {
						ret.Add(record);
					}
				}
				if (ret.Count == 0 && !RecordsFrom(date.Date.AddDays(1)).Where(record => identifier.Matches(record)).Any()) { // If nothing comes after this date
					throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {m_Name}");
				}
				return ret.ToArray();
			} else {
				return Array.Empty<ScheduleRecord>();
			}
		});

		public override Task<ScheduleRecord[]> GetWeekRecordsAsync(IdentifierInfo identifier, int weeksFromNow = 0) => Task.Run(() => {
			EnsureIdentifierPresent(identifier);

			DateTime targetFirstDate = DateTime.Today
				.AddDays(-(int) DateTime.Today.DayOfWeek + 1) // First date in this week; + 1 because DayOfWeek.Sunday == 0, and Monday == 1
				.AddDays(7 * weeksFromNow); // First date in the week n weeks from now
			DateTime targetLastDate = targetFirstDate.AddDays(4); // Friday

			var weekRecords = new List<ScheduleRecord>();

			foreach (ScheduleRecord record in RecordsFrom(targetFirstDate)) {
				if (identifier.Matches(record)) {
					if (record.Start.Date > targetLastDate) {
						break;
					}
					weekRecords.Add(record);
				}
			}
			return weekRecords.ToArray();
		});

		public override Task<ScheduleRecord> GetRecordBeforeDateTimeAsync(IdentifierInfo identifier, DateTime target) => Task.Run(() => {
			IEnumerable<ScheduleRecord> sequence() {
				for (DateTime date = target.Date; date > m_Begin; date = date.AddDays(-1)) {
					if (m_Schedule.TryGetValue(date, out var scheduleDate)) {
						foreach (ScheduleRecord yieldRecord in scheduleDate.Reverse()) {
							yield return yieldRecord;
						}
					}
				}
			}

			EnsureIdentifierPresent(identifier);

			foreach (ScheduleRecord record in sequence()) {
				if (identifier.Matches(record)) {
					if (record.End < target) {
						return record;
					}
				}
			}
			throw new RecordsOutdatedException("Cannot look as far back as " + target.ToString());
		});

		private IEnumerable<ScheduleRecord> RecordsFrom(DateTime target) {
			for (DateTime date = target.Date; date <= End; date = date.AddDays(1).Date) {
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
