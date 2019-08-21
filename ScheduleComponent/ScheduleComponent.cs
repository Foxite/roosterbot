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

			help.AddHelpSection("rooster", Resources.ScheduleComponent_HelpText_Rooster);

			help.AddHelpSection("klas", Resources.ScheduleComponent_HelpText_Class);
		}

		public override Task OnShutdown() {
			m_UserClasses.Dispose();
			return Task.CompletedTask;
		}
	}
}
