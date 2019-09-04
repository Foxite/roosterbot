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

		public async Task<ScheduleRecord> GetCurrentRecord(IdentifierInfo identifier, RoosterCommandContext context) {
			return (await GetScheduleTypeAsync(identifier, context)).GetCurrentRecord(identifier);
		}

		public async Task<ScheduleRecord> GetNextRecord(IdentifierInfo identifier, RoosterCommandContext context) {
			return (await GetScheduleTypeAsync(identifier, context)).GetNextRecord(identifier);
		}

		public async Task<ScheduleRecord> GetRecordAfter(IdentifierInfo identifier, ScheduleRecord givenRecord, RoosterCommandContext context) {
			return (await GetScheduleTypeAsync(identifier, context)).GetRecordAfter(identifier, givenRecord);
		}

		public async Task<ScheduleRecord[]> GetSchedulesForDate(IdentifierInfo identifier, DateTime date, RoosterCommandContext context) {
			return (await GetScheduleTypeAsync(identifier, context)).GetSchedulesForDate(identifier, date);
		}

		public async Task<AvailabilityInfo[]> GetWeekAvailability(IdentifierInfo identifier, int weeksFromNow, RoosterCommandContext context) {
			return (await GetScheduleTypeAsync(identifier, context)).GetWeekAvailability(identifier, weeksFromNow);
		}

		public async Task<ScheduleRecord> GetRecordAfterTimeSpan(IdentifierInfo identifier, TimeSpan timespan, RoosterCommandContext context) {
			return (await GetScheduleTypeAsync(identifier, context)).GetRecordAfterTimeSpan(identifier, timespan);
		}

		private async Task<ScheduleProvider> GetScheduleTypeAsync(IdentifierInfo info, RoosterCommandContext context) {
			if (m_Schedules.TryGetValue(info.GetType(), out List<ScheduleProvider> list)) {
				IGuild guild = context.Guild ?? await context.GetDMGuildAsync();
				return list.FirstOrDefault(schedule => schedule.IsGuildAllowed(guild)) ?? throw new NoAllowedGuildsException($"No schedules are allowed for guild {guild.Name}");
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
