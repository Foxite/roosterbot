using System.Text.RegularExpressions;

namespace RoosterBot.Services {
	public class CommandMatchingService {
		private TeacherNameService m_Teachers;
		private Regex m_StudentRegex = new Regex("[1-4][Gg][ADad][12]");
		private Regex m_RoomRegex = new Regex("[aAbB][12][0-9]{2}");
		
		public CommandMatchingService(TeacherNameService teachers) {
			m_Teachers = teachers;
		}

		public CommandType MatchCommand(string parameters) {
			if (m_StudentRegex.IsMatch(parameters)) {
				return CommandType.Student;
			} else if (m_RoomRegex.IsMatch(parameters)) {
				return CommandType.Room;
			} else if (m_Teachers.GetAbbrsFromNameInput(parameters) != null) {
				return CommandType.Teacher;
			} else {
				return CommandType.Unknown;
			}
		}
	}

	public enum CommandType {
		Unknown, Student, Teacher, Room
	}
}
