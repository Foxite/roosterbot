using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Schedule.GLU {
	public class GLUScheduleComponent : ComponentBase {
		private List<ScheduleRegistryInfo> m_Schedules;
		private ulong[] m_AllowedGuilds;
		private string m_TeacherPath;

		public override Version ComponentVersion => new Version(1, 0, 0);

		public override Task AddServices(IServiceCollection services, string configPath) {
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);
			JObject scheduleContainer = jsonConfig["schedules"].ToObject<JObject>();

			m_Schedules = new List<ScheduleRegistryInfo>();

			void addSchedule<T>(string name) where T : IdentifierInfo {
				m_Schedules.Add(new ScheduleRegistryInfo(typeof(T), name, Path.Combine(configPath, scheduleContainer[name].ToObject<string>())));
			}

			addSchedule<StudentSetInfo>("GLU-StudentSets");
			addSchedule<TeacherInfo>("GLU-Teachers");
			addSchedule<RoomInfo>("GLU-Rooms");

			m_AllowedGuilds = jsonConfig["allowedGuilds"].ToObject<JArray>().Select((token) => token.ToObject<ulong>()).ToArray();

			m_TeacherPath = Path.Combine(configPath, "leraren-afkortingen.csv");

			return Task.CompletedTask;
		}

		public override async Task AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			await services.GetService<TeacherNameService>().ReadAbbrCSV(m_TeacherPath, m_AllowedGuilds);

			List<(Type identifierType, Task<ScheduleService> scheduleTask)> tasks = new List<(Type identifierType, Task<ScheduleService> scheduleTask)>();
			TeacherNameService teachers = services.GetService<TeacherNameService>();

			foreach (ScheduleRegistryInfo sri in m_Schedules) {
				tasks.Add((sri.IdentifierType, ScheduleService.CreateAsync(sri.Name, new GLUScheduleReader(sri.Path, teachers, m_AllowedGuilds[0]), m_AllowedGuilds)));
			}

			await Task.WhenAll(tasks.Select(item => item.scheduleTask));

			ScheduleProvider provider = services.GetService<ScheduleProvider>();

			foreach ((Type identifierType, Task<ScheduleService> scheduleTask) in tasks) {
				provider.RegisterSchedule(identifierType, await scheduleTask);
			}

			ActivityNameService activityService = services.GetService<ActivityNameService>();
			GLUActivities gluActivities = new GLUActivities();
			activityService.RegisterLookup(m_AllowedGuilds, gluActivities.GetActivityFromAbbr);
		}

		private class ScheduleRegistryInfo {
			public Type IdentifierType { get; set; }
			public string Name { get; set; }
			public string Path { get; set; }

			public ScheduleRegistryInfo(Type identifierType, string name, string path) {
				IdentifierType = identifierType;
				Name = name;
				Path = path;
			}
		}
	}
}
