using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RoosterBot;
using RoosterBot.Services;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Modules;
using ScheduleComponent.Readers;
using ScheduleComponent.Services;

namespace ScheduleComponent {
	public class ScheduleComponent : ComponentBase {
		private UserClassesService m_UserClasses;

		public override string VersionString => "2.0.0";

		public override Task AddServices(IServiceCollection services, string configPath) {
			TeacherNameService teachers = new TeacherNameService();

			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);
			
			m_UserClasses = new UserClassesService(jsonConfig["databaseKeyId"].ToObject<string>(), jsonConfig["databaseSecretKey"].ToObject<string>());

			services
				.AddSingleton(teachers)
				.AddSingleton(new ScheduleProvider())
				.AddSingleton(new LastScheduleCommandService())
				.AddSingleton(new ActivityNameService())
				.AddSingleton(m_UserClasses);

			Logger.Debug("ScheduleComponent", "Started services");

			return Task.CompletedTask;
		}

		public async override Task AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help) {
			commandService.AddTypeReader<StudentSetInfo>(new StudentSetInfoReader());
			commandService.AddTypeReader<TeacherInfo[]>(new TeacherInfoReader());
			commandService.AddTypeReader<RoomInfo>(new RoomInfoReader());
			commandService.AddTypeReader<DayOfWeek>(new DayOfWeekReader());

			await Task.WhenAll(
				commandService.AddModuleAsync<DefaultScheduleModule>(services),
				commandService.AddModuleAsync<AfterScheduleModule>(services),
				commandService.AddModuleAsync<StudentScheduleModule>(services),
				commandService.AddModuleAsync<TeacherScheduleModule>(services),
				commandService.AddModuleAsync<RoomScheduleModule>(services),
				commandService.AddModuleAsync<TeacherListModule>(services),
				commandService.AddModuleAsync<UserClassModule>(services)
			);

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

		public override Task OnShutdown() {
			m_UserClasses.Dispose();
			return Task.CompletedTask;
		}
	}
}
