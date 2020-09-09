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
		private SnowflakeReference[] m_AllowedChannels;
		private string m_StaffMemberPath;
		private bool m_SkipPastRecords;
		private int m_RepeatRecords;
		private bool m_ExpandActivites;

		public override Version ComponentVersion => new Version(1, 2, 0);
		public override IEnumerable<string> Tags => new[] { "ScheduleProvider" };

		public GLUComponent() {
			m_Schedules = new List<ScheduleRegistryInfo>();
			m_AllowedChannels = Array.Empty<SnowflakeReference>();
			m_StaffMemberPath = "";
		}

		protected override DependencyResult CheckDependencies(IEnumerable<Component> components) {
			return DependencyResult.Build(components)
				.RequireMinimumVersion<ScheduleComponent>(new Version(2, 0, 0))
				.Check();
		}

		protected override void AddServices(IServiceCollection services, string configPath) {
			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				SkipPastRecords = false,
				RepeatRecords = 0,
				TimezoneId = "",
				Schedules = new Dictionary<string, string>(),
				AllowedChannels = new[] {
					new {
						Platform = "",
						Id = ""
					}
				},
				ExpandActivites = true
			});

			m_SkipPastRecords = config.SkipPastRecords;
			m_RepeatRecords = config.RepeatRecords;
			m_ExpandActivites = config.ExpandActivites;
			m_AllowedChannels = (
				from uncheckedSR in config.AllowedChannels
				let platform = Program.Instance.Components.GetPlatform(uncheckedSR.Platform)
				where !(platform is null)
				select new SnowflakeReference(platform, platform.GetSnowflakeIdFromString(uncheckedSR.Id))
			).ToArray();

			void addSchedule<T>(string name) where T : IdentifierInfo {
				m_Schedules.Add(new ScheduleRegistryInfo(typeof(T), name, Path.Combine(configPath, config.Schedules[name])));
			}

			if (config.Schedules.ContainsKey("GLU-StudentSets")) {
				addSchedule<StudentSetInfo>("GLU-StudentSets");
			}

			if (config.Schedules.ContainsKey("GLU-StaffMembers")) {
				addSchedule<StaffMemberInfo>("GLU-StaffMembers");
			}

			if (config.Schedules.ContainsKey("GLU-Rooms")) {
				addSchedule<RoomInfo>("GLU-Rooms");
			}

			m_StaffMemberPath = Path.Combine(configPath, "leraren-afkortingen.csv");
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commands) {
			services.GetRequiredService<ResourceService>().RegisterResources("RoosterBot.GLU.Resources");

			// Staff members
			StaffMemberService members = services.GetRequiredService<StaffMemberService>();
			members.AddStaff(new GLUStaffMemberReader(m_StaffMemberPath).ReadCSV(), m_AllowedChannels);

			ScheduleService provider = services.GetRequiredService<ScheduleService>();

			foreach (ScheduleRegistryInfo sri in m_Schedules) {
				provider.RegisterProvider(sri.IdentifierType,
					new MemoryScheduleProvider(sri.Name,
						new GLUScheduleReader(sri.Path, members, m_AllowedChannels[0], m_SkipPastRecords, m_RepeatRecords, m_ExpandActivites),
						m_AllowedChannels));
			}

			// Student sets and Rooms validator
			services.GetRequiredService<IdentifierValidationService>().RegisterValidator(new GLUIdentifierValidator(m_AllowedChannels));
		}

		private class GLUIdentifierValidator : IdentifierValidator {
			private static readonly Regex StudentSetRegex = new Regex("^[1-4]g[ad][12](-?[abcd])?$", RegexOptions.IgnoreCase);
			private static readonly Regex RoomRegex = new Regex("^[abw][0-4][0-9]{2}$", RegexOptions.IgnoreCase);

			public GLUIdentifierValidator(IEnumerable<SnowflakeReference> allowedChannels) : base(allowedChannels) { }

			public override Task<IdentifierInfo?> ValidateAsync(RoosterCommandContext context, string input) {
				input = input.ToUpper();
				IdentifierInfo? result = null;
				if (StudentSetRegex.IsMatch(input)) {
					if (input.Length == 5) {
						input = input.Insert(4, "-"); // Reinsert - if it was not added by user
					}
					result = new StudentSetInfo(input);
				} else if (RoomRegex.IsMatch(input)) {
					result = new RoomInfo(input);
				}
				return Task.FromResult(result);
			}
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
