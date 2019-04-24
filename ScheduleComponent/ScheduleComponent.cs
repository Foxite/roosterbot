using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RoosterBot;
using RoosterBot.Services;
using ScheduleComponent.Modules;
using ScheduleComponent.Services;

namespace ScheduleComponent {
	public class ScheduleComponent : ComponentBase {
		private DiscordSocketClient m_Client;
		private WatsonClient m_Watson;

		public override void AddServices(ref IServiceCollection services, string configPath) {
			List<Task> concurrentLoading = new List<Task>();

			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);
			JObject scheduleContainer = jsonConfig["schedules"].ToObject<JObject>();
			Dictionary<string, string> schedules = new Dictionary<string, string>();
			foreach (KeyValuePair<string, JToken> token in scheduleContainer) {
				schedules.Add(token.Key, token.Value.ToObject<string>());
			}
			
			TeacherNameService teachers = new TeacherNameService();
			teachers.ReadAbbrCSV(Path.Combine(configPath, "leraren-afkortingen.csv")).GetAwaiter().GetResult();

			ScheduleService<StudentSetInfo> schedStudents = new ScheduleService<StudentSetInfo>(teachers, "StudentSets");
			ScheduleService<TeacherInfo>	schedTeachers = new ScheduleService<TeacherInfo>   (teachers, "StaffMember");
			ScheduleService<RoomInfo>		schedRooms    = new ScheduleService<RoomInfo>	   (teachers, "Room");
			// Concurrently read schedules.
			concurrentLoading.Add(schedStudents.ReadScheduleCSV(Path.Combine(configPath, schedules["StudentSets"])));
			concurrentLoading.Add(schedTeachers.ReadScheduleCSV(Path.Combine(configPath, schedules["StaffMember"])));
			concurrentLoading.Add(schedRooms   .ReadScheduleCSV(Path.Combine(configPath, schedules["Room"])));

			Logger.Log(LogSeverity.Debug, "Main", "Started services");

			Task.WaitAll(concurrentLoading.ToArray());

			services
				.AddSingleton(teachers)
				.AddSingleton(new ScheduleProvider(schedStudents, schedTeachers, schedRooms))
				.AddSingleton(schedStudents)
				.AddSingleton(schedTeachers)
				.AddSingleton(schedRooms)
				.AddSingleton(new LastScheduleCommandService())
				.AddSingleton(new CommandMatchingService(teachers));
		}

		public override void AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help) {
			commandService.AddModuleAsync<GenericCommandsModule>(services);
			commandService.AddModuleAsync<ScheduleModuleBase<StudentSetInfo>>(services);
			commandService.AddModuleAsync<StudentScheduleModule>(services);
			commandService.AddModuleAsync<TeacherScheduleModule>(services);
			commandService.AddModuleAsync<RoomScheduleModule>(services);
			commandService.AddModuleAsync<TeacherListModule>(services);

			m_Client = services.GetService<DiscordSocketClient>();
			m_Client.MessageReceived += ProcessNaturalLanguageCommands;

			string helpText = "Je kan opvragen welke les een klas of een leraar nu heeft, of in een lokaal bezig is.\n";
			helpText += "Ik begrijp dan automatisch of je het over een klas, leraar of lokaal hebt.\n";
			helpText += "Ik ken de afkortingen, voornamen, en alternative spellingen van alle leraren.\n";
			helpText += "`!nu 1gd2`, `!nu laurence candel`, `!nu laurens`, `!nu lca`, `!nu a223`\n\n";
			helpText += "Je kan ook opvragen wat er hierna, op een bepaalde weekdag, of morgen als eerste is.\n";
			helpText += "`!hierna 2gd1`, `!dag lance woensdag` (de volgorde maakt niet uit: `!dag wo lkr` doet hetzelfde), `!morgen b114`\n\n";
			helpText += "Je kan ook zien wat de klas/leraar/lokaal heeft na wat ik je net heb verteld. Dus als je pauze hebt, kun je zien wat je na de pauze hebt.\n";
			helpText += "`!hierna 3ga1` en dan `!daarna`. Je kan `!daarna` zo vaak gebruiken als je wilt.\n\n";
			helpText += "Als ik niet begrijp of je het over een klas, leraar, of lokaal hebt, kun je dit in de command zetten:\n";
			helpText += "`!klas nu 2ga1`, `leraar dag martijn dinsdag`, `!lokaal morgen a128`\n\n";
			helpText += "Je kan een lijst van alle docenten opvragen, met hun afkortingen en discord namen: `!docenten` of `!leraren`\n";
			helpText += "Deze lijst kan ook gefilterd worden: `!docenten martijn`";

			help.AddHelpSection("rooster", helpText);
		}

		private Task ProcessNaturalLanguageCommands(SocketMessage msg) {
			if (msg.Content.StartsWith(m_Client.CurrentUser.Mention)) {

			}
		}
	}

	public class ScheduleProvider {
		private ScheduleService<StudentSetInfo> m_Students;
		private ScheduleService<TeacherInfo> m_Teachers;
		private ScheduleService<RoomInfo> m_Rooms;

		public ScheduleProvider(ScheduleService<StudentSetInfo> students, ScheduleService<TeacherInfo> teachers, ScheduleService<RoomInfo> rooms) {
			m_Students = students;
			m_Teachers = teachers;
			m_Rooms = rooms;
		}

		public ScheduleRecord GetCurrentRecord(IdentifierInfo identifier) {
			switch (GetScheduleType(identifier)) {
			case ScheduleType.StudentSets:
				return m_Students.GetCurrentRecord((StudentSetInfo) identifier);
			case ScheduleType.StaffMember:
				return m_Teachers.GetCurrentRecord((TeacherInfo) identifier);
			case ScheduleType.Room:
				return m_Rooms.GetCurrentRecord((RoomInfo) identifier);
			default:
				throw new ArgumentException("Identifier type " + identifier.GetType().Name + " is not known to ScheduleProvider (B - shouldn't have happened)");
			}
		}

		public ScheduleRecord GetNextRecord(IdentifierInfo identifier) {
			switch (GetScheduleType(identifier)) {
			case ScheduleType.StudentSets:
				return m_Students.GetNextRecord((StudentSetInfo) identifier);
			case ScheduleType.StaffMember:
				return m_Teachers.GetNextRecord((TeacherInfo) identifier);
			case ScheduleType.Room:
				return m_Rooms.GetNextRecord((RoomInfo) identifier);
			default:
				throw new ArgumentException("Identifier type " + identifier.GetType().Name + " is not known to ScheduleProvider (B - shouldn't have happened)");
			}
		}

		public ScheduleRecord GetFirstRecordForDay(IdentifierInfo identifier, DayOfWeek day) {
			switch (GetScheduleType(identifier)) {
			case ScheduleType.StudentSets:
				return m_Students.GetFirstRecordForDay((StudentSetInfo) identifier, day);
			case ScheduleType.StaffMember:
				return m_Teachers.GetFirstRecordForDay((TeacherInfo) identifier, day);
			case ScheduleType.Room:
				return m_Rooms.GetFirstRecordForDay((RoomInfo) identifier, day);
			default:
				throw new ArgumentException("Identifier type " + identifier.GetType().Name + " is not known to ScheduleProvider (B - shouldn't have happened)");
			}
		}

		public ScheduleRecord GetRecordAfter(IdentifierInfo identifier, ScheduleRecord givenRecord) {
			switch (GetScheduleType(identifier)) {
			case ScheduleType.StudentSets:
				return m_Students.GetRecordAfter((StudentSetInfo) identifier, givenRecord);
			case ScheduleType.StaffMember:
				return m_Teachers.GetRecordAfter((TeacherInfo) identifier, givenRecord);
			case ScheduleType.Room:
				return m_Rooms.GetRecordAfter((RoomInfo) identifier, givenRecord);
			default:
				throw new ArgumentException("Identifier type " + identifier.GetType().Name + " is not known to ScheduleProvider (B - shouldn't have happened)");
			}
		}

		public ScheduleRecord[] GetSchedulesForDay(IdentifierInfo identifier, DayOfWeek day) {
			switch (GetScheduleType(identifier)) {
			case ScheduleType.StudentSets:
				return m_Students.GetSchedulesForDay((StudentSetInfo) identifier, day);
			case ScheduleType.StaffMember:
				return m_Teachers.GetSchedulesForDay((TeacherInfo) identifier, day);
			case ScheduleType.Room:
				return m_Rooms.GetSchedulesForDay((RoomInfo) identifier, day);
			default:
				throw new ArgumentException("Identifier type " + identifier.GetType().Name + " is not known to ScheduleProvider (B - shouldn't have happened)");
			}
		}

		private ScheduleType GetScheduleType(IdentifierInfo info) {
			if (info is StudentSetInfo)
				return ScheduleType.StudentSets;
			else if (info is TeacherInfo)
				return ScheduleType.StaffMember;
			else if (info is RoomInfo)
				return ScheduleType.Room;
			else
				throw new ArgumentException("Identifier type " + info.GetType().Name + " is not known to ScheduleProvider (A)");
		}

		private enum ScheduleType {
			StudentSets, StaffMember, Room
		}
	}
}
