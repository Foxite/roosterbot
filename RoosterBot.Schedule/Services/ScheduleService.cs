using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;

namespace RoosterBot.Schedule {
	public class ScheduleService {
		private Dictionary<Type, List<ScheduleProvider>> m_Schedules;

		public ScheduleService() {
			m_Schedules = new Dictionary<Type, List<ScheduleProvider>>();
		}

		public ScheduleRecord GetCurrentRecord(IdentifierInfo identifier, ICommandContext context) {
			return GetScheduleType(identifier, context).GetCurrentRecord(identifier);
		}

		public ScheduleRecord GetNextRecord(IdentifierInfo identifier, ICommandContext context) {
			return GetScheduleType(identifier, context).GetNextRecord(identifier);
		}

		public ScheduleRecord GetRecordAfter(IdentifierInfo identifier, ScheduleRecord givenRecord, ICommandContext context) {
			return GetScheduleType(identifier, context).GetRecordAfter(identifier, givenRecord);
		}

		public ScheduleRecord[] GetSchedulesForDate(IdentifierInfo identifier, DateTime date, ICommandContext context) {
			return GetScheduleType(identifier, context).GetSchedulesForDate(identifier, date);
		}

		public AvailabilityInfo[] GetWeekAvailability(IdentifierInfo identifier, int weeksFromNow, ICommandContext context) {
			return GetScheduleType(identifier, context).GetWeekAvailability(identifier, weeksFromNow);
		}

		public ScheduleRecord GetRecordAfterTimeSpan(IdentifierInfo identifier, TimeSpan timespan, ICommandContext context) {
			return GetScheduleType(identifier, context).GetRecordAfterTimeSpan(identifier, timespan);
		}

		private ScheduleProvider GetScheduleType(IdentifierInfo info, ICommandContext context) {
			if (m_Schedules.TryGetValue(info.GetType(), out List<ScheduleProvider> list)) {
				return list.FirstOrDefault(schedule => schedule.IsGuildAllowed(context.Guild)) ?? throw new NoAllowedGuildsException($"No schedules are allowed for guild {context.Guild.Name}");
			} else {
				throw new ArgumentException("Identifier type " + info.GetType().Name + " is not known to ScheduleProvider");
			}
		}

		public void RegisterSchedule(Type infoType, ScheduleProvider schedule) {
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
