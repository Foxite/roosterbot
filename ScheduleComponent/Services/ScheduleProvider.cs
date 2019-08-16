using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Discord.Commands;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Services {
	public class ScheduleProvider {
		private Dictionary<Type, List<ScheduleService>> m_Schedules;

		public ScheduleProvider() {
			m_Schedules = new Dictionary<Type, List<ScheduleService>>();
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

		public ScheduleRecord GetRecordAfterTimeSpan(IdentifierInfo identifier, TimeSpan timespan, ICommandContext context) {
			return GetScheduleType(identifier, context).GetRecordAfterTimeSpan(identifier, timespan);
		}

		private ScheduleService GetScheduleType(IdentifierInfo info, ICommandContext context) {
			if (m_Schedules.TryGetValue(info.GetType(), out List<ScheduleService> list)) {
				return list.FirstOrDefault(schedule => schedule.IsGuildAllowed(context.Guild)) ?? throw new NoSchedulesAvailableException($"No schedules are allowed for guild {context.Guild}");
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

	[Serializable]
	public class NoSchedulesAvailableException : Exception {
		public NoSchedulesAvailableException() { }
		public NoSchedulesAvailableException(string message) : base(message) { }
		public NoSchedulesAvailableException(string message, Exception inner) : base(message, inner) { }
		protected NoSchedulesAvailableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
