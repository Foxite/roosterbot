using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RoosterBot;
using RoosterBot.Services;
using ScheduleComponent.Modules;
using ScheduleComponent.Services;

namespace ScheduleComponent {
	public class ScheduleComponent : ComponentBase {
		//public override void Initialize(ref IServiceCollection services, EditedCommandService commandService, string configPath) {
		public override void AddServices(ref IServiceCollection services, string configPath) {
			List<Task> concurrentLoading = new List<Task>();

			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);
			JObject scheduleContainer = jsonConfig["schedules"].ToObject<JObject>();
			Dictionary<string, string> schedules = new Dictionary<string, string>();
			foreach (KeyValuePair<string, JToken> token in scheduleContainer) {
				schedules.Add(token.Key, token.Value.ToObject<string>());
			}

			ScheduleService scheduleService = new ScheduleService();
			// Concurrently read schedules.
			foreach (KeyValuePair<string, string> schedule in schedules) {
				concurrentLoading.Add(scheduleService.ReadScheduleCSV(schedule.Key, Path.Combine(configPath, schedule.Value)));
			}

			TeacherNameService teachers = new TeacherNameService();
			concurrentLoading.Add(teachers.ReadAbbrCSV(Path.Combine(configPath, "leraren-afkortingen.csv")));

			Logger.Log(LogSeverity.Debug, "Main", "Started services");

			Task.WaitAll(concurrentLoading.ToArray());

			services
				.AddSingleton(teachers)
				.AddSingleton(scheduleService)
				.AddSingleton(new LastScheduleCommandService(scheduleService))
				.AddSingleton(new CommandMatchingService(teachers));
		}

		public override void AddModules(IServiceProvider services, EditedCommandService commandService) {
			//commandService.AddModulesAsync(GetType().Assembly);
			commandService.AddModuleAsync<GenericCommandsModule>(services);
			commandService.AddModuleAsync<ScheduleModuleBase>(services);
			commandService.AddModuleAsync<StudentScheduleModule>(services);
			commandService.AddModuleAsync<TeacherScheduleModule>(services);
			commandService.AddModuleAsync<RoomScheduleModule>(services);
			commandService.AddModuleAsync<TeacherListModule>(services);
		}
	}
}
