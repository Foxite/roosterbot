using System;
using System.Collections.Generic;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Services {
	public class ScheduleProvider {
		private Dictionary<Type, ScheduleService> m_Schedules;

		public ScheduleProvider() {
			m_Schedules = new Dictionary<Type, ScheduleService>();
		}

		public ScheduleRecord GetCurrentRecord(IdentifierInfo identifier) {
			return GetScheduleType(identifier).GetCurrentRecord(identifier);
		}

		public ScheduleRecord GetNextRecord(IdentifierInfo identifier) {
			return GetScheduleType(identifier).GetNextRecord(identifier);
		}

		public ScheduleRecord GetRecordAfter(IdentifierInfo identifier, ScheduleRecord givenRecord) {
			return GetScheduleType(identifier).GetRecordAfter(identifier, givenRecord);
		}

		public ScheduleRecord[] GetSchedulesForDay(IdentifierInfo identifier, DayOfWeek day, bool includeToday) {
			return GetScheduleType(identifier).GetSchedulesForDay(identifier, day, includeToday);
		}

		public AvailabilityInfo[] GetWeekAvailability(IdentifierInfo identifier, int weeksFromNow) {
			return GetScheduleType(identifier).GetWeekAvailability(identifier, weeksFromNow);
		}

		private ScheduleService GetScheduleType(IdentifierInfo info) {
			if (m_Schedules.TryGetValue(info.GetType(), out ScheduleService schedule)) {
				return schedule;
			} else {
				throw new ArgumentException("Identifier type " + info.GetType().Name + " is not known to ScheduleProvider");
			}
		}

		public void RegisterSchedule(Type infoType, ScheduleService schedule) {
			if (!infoType.IsAssignableFrom(typeof(IdentifierInfo))) {
				throw new ArgumentException($"The given type must be a type of IdentifierInfo.", nameof(infoType));
			}

			if (m_Schedules.ContainsKey(infoType)) {
				throw new ArgumentException($"A schedule was already registered for {infoType.Name}.");
			}

			m_Schedules[infoType] = schedule;
		}
	}
}
