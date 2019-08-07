using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RoosterBot;
using RoosterBot.Services;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GLUScheduleComponent {
	// Dear Wednesday-Me,
	// Good luck
	// Sincerely, Tuesday-Me
	public class GLUScheduleComponent : ComponentBase {
		public override string VersionString => "0.1.0";
		private List<(Type identifierType, string name, string path)> m_Schedules;

		public override Task AddServices(IServiceCollection services, string configPath) {
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);
			JObject scheduleContainer = jsonConfig["schedules"].ToObject<JObject>();

			m_Schedules = new List<(Type identifierType, string name, string path)> {
				(typeof(StudentSetInfo), nameof(ScheduleRecord.StudentSets),  Path.Combine(configPath, scheduleContainer["StudentSets"].ToObject<string>())),
				(typeof(TeacherInfo), nameof(ScheduleRecord.StaffMember),  Path.Combine(configPath, scheduleContainer["StaffMember"].ToObject<string>())),
				(typeof(RoomInfo), nameof(ScheduleRecord.Room), Path.Combine(configPath, scheduleContainer["Room"].ToObject<string>()))
			};

			return Task.CompletedTask;
		}

		public override async Task AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help) {
			List<(Type identifierType, Task<ScheduleService> scheduleTask)> tasks = new List<(Type identifierType, Task<ScheduleService> scheduleTask)>();
			TeacherNameService teachers = services.GetService<TeacherNameService>();

			foreach ((Type identifierType, string name, string path) in m_Schedules) {
				tasks.Add((identifierType, ScheduleService.CreateAsync(name, new GLUScheduleReader(path, teachers))));
			}

			await Task.WhenAll(tasks.Select(item => item.scheduleTask));

			ScheduleProvider provider = services.GetService<ScheduleProvider>();

			foreach ((Type identifierType, Task<ScheduleService> scheduleTask) in tasks) {
				provider.RegisterSchedule(identifierType, await scheduleTask);
			}
		}
	}
}
