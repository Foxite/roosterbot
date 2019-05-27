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
using ScheduleComponent.Readers;
using ScheduleComponent.Services;

namespace ScheduleComponent {
	public class ScheduleComponent : ComponentBase {
		private DiscordSocketClient m_Client;
		private ConfigService m_Config;

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

			services
				.AddSingleton(teachers)
				.AddSingleton(new ScheduleProvider(schedStudents, schedTeachers, schedRooms))
				.AddSingleton(schedStudents)
				.AddSingleton(schedTeachers)
				.AddSingleton(schedRooms)
				.AddSingleton(new LastScheduleCommandService())
				.AddSingleton(new CommandMatchingService(teachers))
				.AddSingleton(new UserClassesService(jsonConfig["databaseKeyId"].ToObject<string>(), jsonConfig["databaseSecretKey"].ToObject<string>()));

			Task.WaitAll(concurrentLoading.ToArray());

			Logger.Log(LogSeverity.Debug, "ScheduleComponent", "Started services");
		}

		public override void AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help) {
			commandService.AddTypeReader<StudentSetInfo>(new StudentSetInfoReader());
			commandService.AddTypeReader<TeacherInfo[]>(new TeacherInfoReader());
			commandService.AddTypeReader<RoomInfo>(new RoomInfoReader());
			commandService.AddTypeReader<DayOfWeek>(new DayOfWeekReader());

			Task.WaitAll(
				//commandService.AddModuleAsync<GenericCommandsModule>(services),
				commandService.AddModuleAsync<ScheduleModuleBase<StudentSetInfo>>(services),
				commandService.AddModuleAsync<StudentScheduleModule>(services),
				commandService.AddModuleAsync<TeacherScheduleModule>(services),
				commandService.AddModuleAsync<RoomScheduleModule>(services),
				commandService.AddModuleAsync<TeacherListModule>(services),
				commandService.AddModuleAsync<UserClassModule>(services)
			);

			m_Config = services.GetService<ConfigService>();
			m_Client = services.GetService<DiscordSocketClient>();

			string helpText = "Je kan opvragen welke les een klas of een leraar nu heeft, of in een lokaal bezig is.\n";
			helpText += "Ik begrijp dan automatisch of je het over een klas, leraar of lokaal hebt.\n";
			helpText += "Ik ken de afkortingen, voornamen, en alternative spellingen van alle leraren.\n";
			helpText += "`!nu 1gd2`, `!nu laurence candel`, `!nu laurens`, `!nu lca`, `!nu a223`\n\n";

			helpText += "Je kan ook opvragen wat er hierna of op een bepaalde weekdag is.\n";
			helpText += "`!hierna 2gd1`, `!dag lance woensdag` (de volgorde maakt niet uit: `!dag wo lkr` doet hetzelfde), `!morgen b114`\n";
			helpText += "Let op: Als je `!vandaag` gebruikt, pak ik vandaag. Maar als je bijvoorbeeld op maandag dit doet: `!dag maandag 4ga1`, pak " +
				"ik volgende week maandag.\n\n";

			helpText += "Je kan ook zien wat de klas/leraar/lokaal heeft na wat ik je net heb verteld.\n";
			helpText += "`!hierna 3ga1` en dan `!daarna`. Je kan `!daarna` zo vaak gebruiken als je wilt.\n";
			helpText += "Als je pauze hebt, laat ik automatisch zien wat er daarna komt.\n\n";

			helpText += "Je kan een lijst van alle docenten opvragen, met hun afkortingen en discord namen: `!docenten` of `!leraren`\n";
			helpText += "Deze lijst kan ook gefilterd worden: `!docenten martijn`";
			help.AddHelpSection("rooster", helpText);


			helpText = "Ik kan onthouden in welke klas jij zit, zodat je dit niet elke keer er bij hoeft te zetten.\n";
			helpText += "Stel je klas in met: `!ik <klas>`, bijvoorbeeld `!ik 2gd1`.\n";
			helpText += "Daarna hoef je niet meer je klas in commands te zetten. Je kan dus alleen maar `!nu` typen en dan weet ik wat ik moet opzoeken.\n";
			helpText += "Je kan altijd kijken in welke klas ik denk dat je zit door alleen `!ik` te typen, om te checken of het nog klopt" +
				" (of als je aan geheugenverlies lijdt, maar dat denk ik niet.)\n\n";
			helpText += "Dit werkt ook met taalcommando's: `@RoosterBot wat heb ik nu?`";
			help.AddHelpSection("klas", helpText);
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

		public ScheduleRecord[] GetSchedulesForDay(IdentifierInfo identifier, DayOfWeek day, bool includeToday) {
			switch (GetScheduleType(identifier)) {
			case ScheduleType.StudentSets:
				return m_Students.GetSchedulesForDay((StudentSetInfo) identifier, day, includeToday);
			case ScheduleType.StaffMember:
				return m_Teachers.GetSchedulesForDay((TeacherInfo) identifier, day, includeToday);
			case ScheduleType.Room:
				return m_Rooms.GetSchedulesForDay((RoomInfo) identifier, day, includeToday);
			default:
				throw new ArgumentException("Identifier type " + identifier.GetType().Name + " is not known to ScheduleProvider (B - shouldn't have happened)");
			}
		}

		private ScheduleType GetScheduleType(IdentifierInfo info) {
			if (info is StudentSetInfo) {
				return ScheduleType.StudentSets;
			} else if (info is TeacherInfo) {
				return ScheduleType.StaffMember;
			} else if (info is RoomInfo) {
				return ScheduleType.Room;
			} else {
				throw new ArgumentException("Identifier type " + info.GetType().Name + " is not known to ScheduleProvider (A)");
			}
		}

		private enum ScheduleType {
			StudentSets, StaffMember, Room
		}
	}
}
