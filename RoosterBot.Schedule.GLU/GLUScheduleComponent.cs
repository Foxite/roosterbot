using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Schedule.GLU {
	public class GLUScheduleComponent : ComponentBase {
		private List<ScheduleRegistryInfo> m_Schedules;
		private ulong[] m_AllowedGuilds;
		private string m_TeacherPath;
		private Regex m_StudentSetRegex;
		private Regex m_RoomRegex;

		public override Version ComponentVersion => new Version(1, 0, 0);

		public GLUScheduleComponent() {
			m_StudentSetRegex = new Regex("^[1-4]G[AD][12]$");
			m_RoomRegex = new Regex("[aAbBwW][012][0-9]{2}");
		}

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
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

		public override async Task AddModulesAsync(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			// Teachers
			await services.GetService<TeacherNameService>().ReadAbbrCSV(m_TeacherPath, m_AllowedGuilds);

			#region Read schedules
			List<(Type identifierType, Task<ScheduleProvider> scheduleTask)> tasks = new List<(Type identifierType, Task<ScheduleProvider> scheduleTask)>();
			TeacherNameService teachers = services.GetService<TeacherNameService>();

			foreach (ScheduleRegistryInfo sri in m_Schedules) {
				tasks.Add((sri.IdentifierType, ScheduleProvider.CreateAsync(sri.Name, new GLUScheduleReader(sri.Path, teachers, m_AllowedGuilds[0]), m_AllowedGuilds)));
			}

			await Task.WhenAll(tasks.Select(item => item.scheduleTask));

			ScheduleService provider = services.GetService<ScheduleService>();

			foreach ((Type identifierType, Task<ScheduleProvider> scheduleTask) in tasks) {
				provider.RegisterProvider(identifierType, await scheduleTask);
			}
			#endregion

			// Activities
			ActivityNameService activityService = services.GetService<ActivityNameService>();
			GLUActivities gluActivities = new GLUActivities();
			activityService.RegisterLookup(m_AllowedGuilds, gluActivities.GetActivityFromAbbr);

			// Student sets
			services.GetService<IdentifierValidationService>().RegisterValidator((context, input) => {
				input = input.ToUpper();
				if (m_AllowedGuilds.Contains(context.Guild.Id) && m_StudentSetRegex.IsMatch(input)) {
					return new StudentSetInfo() {
						ClassName = input
					};
				} else {
					return null;
				}
			});

			// Rooms
			services.GetService<IdentifierValidationService>().RegisterValidator((context, input) => {
				input = input.ToUpper();
				if (m_AllowedGuilds.Contains(context.Guild.Id) && m_RoomRegex.IsMatch(input)) {
					return new RoomInfo() {
						Room = input
					};
				} else {
					return null;
				}
			});
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
