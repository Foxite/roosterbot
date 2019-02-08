using System.Text.RegularExpressions;

namespace ScheduleComponent.Services {
	public class CommandMatchingService {
		private TeacherNameService m_Teachers;
		private Regex m_StudentRegex = new Regex("[1-4][Gg][ADad][12]");
		private Regex m_RoomRegex = new Regex("[aAbBwW][012][0-9]{2}");
		
		public CommandMatchingService(TeacherNameService teachers) {
			m_Teachers = teachers;
		}

		public CommandType MatchCommand(string parameters) {
			// Try to recognize what the parameters are for. For student sets and rooms we have a regex, if these don't match we go through the list of teachers and see if we can find one.
			if (m_StudentRegex.IsMatch(parameters)) {
				return CommandType.Student;
			} else if (m_RoomRegex.IsMatch(parameters)) {
				return CommandType.Room;
			} else {
				int teacherResults = m_Teachers.GetAbbrsFromNameInput(parameters).Length;
				if (teacherResults > 0 && teacherResults < 3) {
					return CommandType.Teacher;
				} else {
					return CommandType.Unknown;
				}
			}
		}
	}

	public enum CommandType {
		Unknown, Student, Teacher, Room
	}
}
