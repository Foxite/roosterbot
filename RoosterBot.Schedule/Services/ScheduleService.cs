using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	public class ScheduleService {
		private Dictionary<Type, List<ScheduleProvider>> m_Schedules;

		public ScheduleService() {
			m_Schedules = new Dictionary<Type, List<ScheduleProvider>>();
		}

		public Task<ScheduleRecord> GetCurrentRecord(IdentifierInfo identifier, RoosterCommandContext context) {
			return GetSchedule(identifier, context).GetCurrentRecordAsync(identifier);
		}

		public Task<ScheduleRecord> GetNextRecord(IdentifierInfo identifier, RoosterCommandContext context) {
			return GetSchedule(identifier, context).GetNextRecordAsync(identifier);
		}

		public Task<ScheduleRecord> GetRecordAfter(IdentifierInfo identifier, ScheduleRecord givenRecord, RoosterCommandContext context) {
			return GetSchedule(identifier, context).GetRecordAfterOtherAsync(identifier, givenRecord);
		}

		public Task<ScheduleRecord[]> GetSchedulesForDate(IdentifierInfo identifier, DateTime date, RoosterCommandContext context) {
			return GetSchedule(identifier, context).GetSchedulesForDateAsync(identifier, date);
		}

		public Task<AvailabilityInfo[]> GetWeekAvailability(IdentifierInfo identifier, int weeksFromNow, RoosterCommandContext context) {
			return GetSchedule(identifier, context).GetWeekAvailabilityAsync(identifier, weeksFromNow);
		}

		public Task<ScheduleRecord> GetRecordAfterTimeSpan(IdentifierInfo identifier, TimeSpan timespan, RoosterCommandContext context) {
			return GetSchedule(identifier, context).GetRecordAfterTimeSpanAsync(identifier, timespan);
		}

		private ScheduleProvider GetSchedule(IdentifierInfo info, RoosterCommandContext context) {
			if (m_Schedules.TryGetValue(info.GetType(), out List<ScheduleProvider> list)) {
				return list.FirstOrDefault(schedule => schedule.IsGuildAllowed(context.Guild)) ?? throw new NoAllowedGuildsException($"No schedules are allowed for guild {context.Guild.Name}");
			} else {
				throw new ArgumentException("Identifier type " + info.GetType().Name + " is not known to ScheduleProvider");
			}
		}

		public void RegisterProvider(Type infoType, ScheduleProvider schedule) {
			if (!typeof(IdentifierInfo).IsAssignableFrom(infoType)) {
				throw new ArgumentException($"The given type must be a type of IdentifierInfo.", nameof(infoType));
			}

			if (m_Schedules.ContainsKey(infoType)) {
				throw new ArgumentException($"A schedule was already registered for {infoType.Name}.");
			}

			if (m_Schedules.TryGetValue(infoType, out List<ScheduleProvider> list)) {
				list.Add(schedule);
			} else {
				m_Schedules[infoType] = new List<ScheduleProvider>() {
					schedule
				};
			}
		}
	}
}
