using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	public class GLUComponent : Component {
		private readonly List<ScheduleRegistryInfo> m_Schedules;
		private readonly Regex m_StudentSetRegex;
		private readonly Regex m_RoomRegex;
		private SnowflakeReference[] m_AllowedGuilds;
		private string m_TeacherPath;
		private bool m_SkipPastRecords;

		public override Version ComponentVersion => new Version(1, 1, 1);
		public override IEnumerable<string> Tags => new[] { "ScheduleProvider" };

		public GLUComponent() {
			m_Schedules = new List<ScheduleRegistryInfo>();
			m_AllowedGuilds = Array.Empty<SnowflakeReference>();
			m_TeacherPath = "";
			m_StudentSetRegex = new Regex("^[1-4]G[AD][12]$");
			m_RoomRegex = new Regex("[aAbBwW][012][0-9]{2}");
		}

		protected override DependencyResult CheckDependencies(IEnumerable<Component> components) {
			return DependencyResult.Build(components)
				.RequireMinimumVersion<ScheduleComponent>(new Version(2, 0, 0))
				.Check();
		}

		protected override void AddServices(IServiceCollection services, string configPath) {
			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				SkipPastRecords = false,
				TimezoneId = "",
				Schedules = new Dictionary<string, string>(),
				AllowedGuilds = new[] {
					new SnowflakeReference(null!, null!)
				}
			});

			m_SkipPastRecords = config.SkipPastRecords;
			m_AllowedGuilds = config.AllowedGuilds;

			void addSchedule<T>(string name) where T : IdentifierInfo {
				m_Schedules.Add(new ScheduleRegistryInfo(typeof(T), name, Path.Combine(configPath, config.Schedules[name])));
			}

			addSchedule<StudentSetInfo>("GLU-StudentSets");
			addSchedule<TeacherInfo>("GLU-Teachers");
			addSchedule<RoomInfo>("GLU-Rooms");

			m_TeacherPath = Path.Combine(configPath, "leraren-afkortingen.csv");
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commands, HelpService help) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.GLU.Resources");

			if (services.GetService<GlobalConfigService>().IgnoreUnknownPlatforms) {
				m_AllowedGuilds = m_AllowedGuilds.Where(sr => sr.Platform != null).ToArray();
			}

			// Teachers
			TeacherNameService teachers = services.GetService<TeacherNameService>();
			teachers.ReadAbbrCSV(m_TeacherPath, m_AllowedGuilds);

			ScheduleService provider = services.GetService<ScheduleService>();

			foreach (ScheduleRegistryInfo sri in m_Schedules) {
				provider.RegisterProvider(sri.IdentifierType, new MemoryScheduleProvider(sri.Name, new GLUScheduleReader(sri.Path, teachers, m_AllowedGuilds[0], m_SkipPastRecords), m_AllowedGuilds));
			}

			// Student sets and Rooms validator
			services.GetService<IdentifierValidationService>().RegisterValidator(ValidateIdentifier);
		}

		private Task<IdentifierInfo?> ValidateIdentifier(RoosterCommandContext context, string input) {
			if (m_AllowedGuilds.Contains(context.ChannelConfig.ChannelReference)) {
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
