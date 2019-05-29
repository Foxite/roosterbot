using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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

			ScheduleService schedStudents = new ScheduleService(teachers, "StudentSets");
			ScheduleService schedTeachers = new ScheduleService(teachers, "StaffMember");
			ScheduleService schedRooms    = new ScheduleService(teachers, "Room");
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
				.AddSingleton(new UserClassesService(jsonConfig["databaseKeyId"].ToObject<string>(), jsonConfig["databaseSecretKey"].ToObject<string>()));

			Task.WaitAll(concurrentLoading.ToArray());

			Logger.Debug("ScheduleComponent", "Started services");
		}

		public override void AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help) {
			commandService.AddTypeReader<StudentSetInfo>(new StudentSetInfoReader());
			commandService.AddTypeReader<TeacherInfo[]>(new TeacherInfoReader());
			commandService.AddTypeReader<RoomInfo>(new RoomInfoReader());
			commandService.AddTypeReader<DayOfWeek>(new DayOfWeekReader());

			Task.WaitAll(
				commandService.AddModuleAsync<DefaultScheduleModule>(services),
				commandService.AddModuleAsync<AfterScheduleModule>(services),
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
		private ScheduleService m_Students;
		private ScheduleService m_Teachers;
		private ScheduleService m_Rooms;

		public ScheduleProvider(ScheduleService students, ScheduleService teachers, ScheduleService rooms) {
			m_Students = students;
			m_Teachers = teachers;
			m_Rooms = rooms;
		}

		public ScheduleRecord GetCurrentRecord(IdentifierInfo identifier) {
			return GetScheduleType(identifier).GetCurrentRecord((StudentSetInfo) identifier);
		}

		public ScheduleRecord GetNextRecord(IdentifierInfo identifier) {
			return GetScheduleType(identifier).GetNextRecord((StudentSetInfo) identifier);
		}

		public ScheduleRecord GetRecordAfter(IdentifierInfo identifier, ScheduleRecord givenRecord) {
			return GetScheduleType(identifier).GetRecordAfter((StudentSetInfo) identifier, givenRecord);
		}

		public ScheduleRecord[] GetSchedulesForDay(IdentifierInfo identifier, DayOfWeek day, bool includeToday) {
			return GetScheduleType(identifier).GetSchedulesForDay((StudentSetInfo) identifier, day, includeToday);
		}

		public AvailabilityInfo[] GetWeekAvailability(IdentifierInfo identifier, int weeksFromNow) {
			return GetScheduleType(identifier).GetWeekAvailability(identifier, weeksFromNow);
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

		private enum ScheduleType {
			StudentSets, StaffMember, Room
		}
	}
}
