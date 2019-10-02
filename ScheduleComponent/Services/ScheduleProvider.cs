using System;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Services {
	public class ScheduleProvider {
		private ScheduleService m_Students;
		private ScheduleService m_Teachers;
		private ScheduleService m_Rooms;

		public ScheduleProvider(ScheduleService students, ScheduleService teachers, ScheduleService rooms) {
			m_Students = students;
			m_Teachers = teachers;
			m_Rooms = rooms;
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

		public ScheduleRecord[] GetWeekRecords(IdentifierInfo identifier, int weeksFromNow) {
			return GetScheduleType(identifier).GetWeekRecords(identifier, weeksFromNow);
		}

		private ScheduleService GetScheduleType(IdentifierInfo info) {
			if (info is StudentSetInfo) {
				return m_Students;
			} else if (info is TeacherInfo) {
				return m_Teachers;
			} else if (info is RoomInfo) {
				return m_Rooms;
			} else {
				throw new ArgumentException("Identifier type " + info.GetType().Name + " is not known to ScheduleProvider");
			}
		}
	}
}
