using System;
using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Services {
	public class ScheduleProvider {
		private Dictionary<Type, List<ScheduleService>> m_Schedules;

		public ScheduleProvider() {
			m_Schedules = new Dictionary<Type, List<ScheduleService>>()
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

		public ScheduleRecord[] GetSchedulesForDay(IdentifierInfo identifier, DayOfWeek day, bool includeToday, ICommandContext context) {
			return GetScheduleType(identifier, context).GetSchedulesForDay(identifier, day, includeToday);
		}

		public AvailabilityInfo[] GetWeekAvailability(IdentifierInfo identifier, int weeksFromNow, ICommandContext context) {
			return GetScheduleType(identifier, context).GetWeekAvailability(identifier, weeksFromNow);
		}

		private ScheduleService GetScheduleType(IdentifierInfo info, ICommandContext context) {
			if (m_Schedules.TryGetValue(info.GetType(), out List<ScheduleService> list)) {
				return list.First(schedule => schedule.IsGuildAllowed(context.Guild));
			} else {
				throw new ArgumentException("Identifier type " + info.GetType().Name + " is not known to ScheduleProvider");
			}
		}

		public void RegisterSchedule(Type infoType, ScheduleService schedule) {
			if (!typeof(IdentifierInfo).IsAssignableFrom(infoType)) {
				throw new ArgumentException($"The given type must be a type of IdentifierInfo.", nameof(infoType));
			}

			if (m_Schedules.ContainsKey(infoType)) {
				throw new ArgumentException($"A schedule was already registered for {infoType.Name}.");
			}

			if (m_Schedules.TryGetValue(infoType, out List<ScheduleService> list)) {
				list.Add(schedule);
			} else {
				m_Schedules[infoType] = new List<ScheduleService>() {
					schedule
				};
			}
		}
	}
}
