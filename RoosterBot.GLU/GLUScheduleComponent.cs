using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Schedule.GLU {
	// This component has a lot of hardcoded snowflake IDs. Normally I'd get all that from a config file, but this component is specifically made for a particular guild,
	//  so generalizing the code does not make a lot of sense.
	public class GLUScheduleComponent : ComponentBase {
		public const long GLUGuildId = 278586698877894657;

		private readonly List<ScheduleRegistryInfo> m_Schedules;
		private readonly Regex m_StudentSetRegex;
		private readonly Regex m_RoomRegex;
		private ulong[] m_AllowedGuilds;
		private string m_TeacherPath;
		private bool m_SkipPastRecords;

		public override Version ComponentVersion => new Version(1, 0, 0);
		public override IEnumerable<string> Tags => new[] { "ScheduleProvider" };

		public GLUScheduleComponent() {
			m_Schedules = new List<ScheduleRegistryInfo>();
			m_AllowedGuilds = Array.Empty<ulong>();
			m_TeacherPath = "";
			m_StudentSetRegex = new Regex("^[1-4]G[AD][12]$");
			m_RoomRegex = new Regex("[aAbBwW][012][0-9]{2}");
		}

		public override DependencyResult CheckDependencies(IEnumerable<ComponentBase> components) {
			return DependencyResult.Build(components)
				.RequireMinimumVersion<ScheduleComponent>(new Version(2, 0, 0))
				.Check();
		}

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);
			m_SkipPastRecords = jsonConfig["skipPastRecords"].ToObject<bool>();

			JObject scheduleContainer = jsonConfig["schedules"].ToObject<JObject>();

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

		public override async Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			// Teachers
			await services.GetService<TeacherNameService>().ReadAbbrCSV(m_TeacherPath, m_AllowedGuilds);

			#region Read schedules
			List<(Type identifierType, Task<MemoryScheduleProvider> scheduleTask)> tasks = new List<(Type identifierType, Task<MemoryScheduleProvider> scheduleTask)>();
			TeacherNameService teachers = services.GetService<TeacherNameService>();

			foreach (ScheduleRegistryInfo sri in m_Schedules) {
				tasks.Add((sri.IdentifierType, MemoryScheduleProvider.CreateAsync(sri.Name, new GLUScheduleReader(sri.Path, teachers, m_AllowedGuilds[0], m_SkipPastRecords), m_AllowedGuilds)));
			}

			await Task.WhenAll(tasks.Select(item => item.scheduleTask));

			ScheduleService provider = services.GetService<ScheduleService>();

			foreach ((Type identifierType, Task<MemoryScheduleProvider> scheduleTask) in tasks) {
				provider.RegisterProvider(identifierType, await scheduleTask);
			}
			#endregion

			// Student sets and Rooms validator
			services.GetService<IdentifierValidationService>().RegisterValidator(ValidateIdentifier);
			DiscordSocketClient client = services.GetService<DiscordSocketClient>();

			new RoleAssignmentHandler(client, services.GetService<ConfigService>());
			new ManualRanksHintHandler(client);
			new NewUserHandler(client);
		}

		private Task<IdentifierInfo?> ValidateIdentifier(RoosterCommandContext context, string input) {
			if (m_AllowedGuilds.Contains(context.GuildConfig.GuildId)) {
				input = input.ToUpper();
				IdentifierInfo? result = null;
				if (m_StudentSetRegex.IsMatch(input)) {
					result = new StudentSetInfo(input);
				} else if (m_RoomRegex.IsMatch(input)) {
					result = new RoomInfo(input);
				}
				return Task.FromResult(result);
			}
			return Task.FromResult((IdentifierInfo?) null);
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
