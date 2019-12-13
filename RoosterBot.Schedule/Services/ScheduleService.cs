using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public class ScheduleService {
		private readonly Dictionary<Type, List<ScheduleProvider>> m_Schedules;

		public ScheduleService() {
			m_Schedules = new Dictionary<Type, List<ScheduleProvider>>();
		}

		public Task<ScheduleRecord?> GetRecordAtDateTime(IdentifierInfo identifier, DateTime datetime, RoosterCommandContext context) {
			return GetSchedule(identifier, context).GetRecordAtDateTimeAsync(identifier, datetime);
		}

		public Task<ScheduleRecord> GetRecordAfterDateTime(IdentifierInfo identifier, DateTime datetime, RoosterCommandContext context) {
			return GetSchedule(identifier, context).GetRecordAfterDateTimeAsync(identifier, datetime);
		}

		public Task<ScheduleRecord[]> GetSchedulesForDate(IdentifierInfo identifier, DateTime date, RoosterCommandContext context) {
			return GetSchedule(identifier, context).GetSchedulesForDateAsync(identifier, date);
		}

		public Task<AvailabilityInfo[]> GetWeekAvailability(IdentifierInfo identifier, int weeksFromNow, RoosterCommandContext context) {
			return GetSchedule(identifier, context).GetWeekAvailabilityAsync(identifier, weeksFromNow);
		}
		
		public Task<ScheduleRecord[]> GetWeekRecordsAsync(IdentifierInfo identifier, int weeksFromNow, RoosterCommandContext context) {
			return GetSchedule(identifier, context).GetWeekRecordsAsync(identifier, weeksFromNow);
		}

		private ScheduleProvider GetSchedule(IdentifierInfo info, RoosterCommandContext context) {
			if (m_Schedules.TryGetValue(info.GetType(), out List<ScheduleProvider>? list)) {
				return list.FirstOrDefault(schedule => schedule.IsGuildAllowed(context.GuildConfig.GuildId)) ??
					throw new NoAllowedGuildsException($"No schedules are allowed for guild {(context.Guild == null ? ("from " + context.User.Username + "#" + context.User.Discriminator) : context.Guild.Name)}");
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

			if (m_Schedules.TryGetValue(infoType, out List<ScheduleProvider>? list)) {
				list.Add(schedule);
			} else {
				m_Schedules[infoType] = new List<ScheduleProvider>() {
					schedule
				};
			}
		}
	}
}
