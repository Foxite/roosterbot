using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Discord;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Services {
	public class ScheduleService {
		private readonly ulong[] m_AllowedGuildIds;
		private List<ScheduleRecord> m_Schedule;
		private string m_Name;

		private ScheduleService(ulong[] allowedGuilds) {
			m_AllowedGuildIds = allowedGuilds;
		}

		public static async Task<ScheduleService> CreateAsync(string name, ScheduleReaderBase reader, ulong[] allowedGuildIds) {
			ScheduleService service = new ScheduleService(allowedGuildIds) {
				m_Name = name,
				m_Schedule = await reader.GetSchedule(name) // Unfortunately we can't have async constructors (for good reasons), so this'll do.
			};
			return service;
		}

		public bool IsGuildAllowed(ulong guildId) {
			return m_AllowedGuildIds.Contains(guildId);
		}

		public bool IsGuildAllowed(IGuild guild) => IsGuildAllowed(guild.Id);

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
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {m_Name}");
			} else {
				throw new ScheduleNotFoundException($"The class {identifier} does not exist in schedule {m_Name}.");
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
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {m_Name}");
			} else {
				throw new ScheduleNotFoundException($"The class {identifier} does not exist in schedule {m_Name}.");
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
				throw new RecordsOutdatedException($"Records outdated for class {identifier} in schedule {m_Name}");
			} else {
				throw new ScheduleNotFoundException($"The class {identifier} does not exist in schedule {m_Name}.");
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
					throw new ScheduleNotFoundException($"The class {identifier} does not exist in schedule {m_Name}.");
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
