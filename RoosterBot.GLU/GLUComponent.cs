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

		public override Version ComponentVersion => new Version(1, 1, 1);
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
				TimezoneId = "",
				Schedules = new Dictionary<string, string>(),
				AllowedChannels = new[] {
					new {
						Platform = "",
						Id = ""
					}
				}
			});

			m_SkipPastRecords = config.SkipPastRecords;
			m_AllowedChannels = (
				from uncheckedSR in config.AllowedChannels
				let platform = Program.Instance.Components.GetPlatform(uncheckedSR.Platform)
				where !(platform is null)
				select new SnowflakeReference(platform, platform.GetSnowflakeIdFromString(uncheckedSR.Id))
			).ToArray();

			void addSchedule<T>(string name) where T : IdentifierInfo {
				m_Schedules.Add(new ScheduleRegistryInfo(typeof(T), name, Path.Combine(configPath, config.Schedules[name])));
			}

			addSchedule<StudentSetInfo>("GLU-StudentSets");
			addSchedule<StaffMemberInfo>("GLU-StaffMembers");
			addSchedule<RoomInfo>("GLU-Rooms");

			m_StaffMemberPath = Path.Combine(configPath, "leraren-afkortingen.csv");
			var staffMemberService = new StaffMemberService();
			staffMemberService.AddStaff(new GLUStaffMemberReader(m_StaffMemberPath).ReadCSV().ToArray(), m_AllowedChannels);
			services.AddSingleton(staffMemberService);
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commands) {
			services.GetRequiredService<ResourceService>().RegisterResources("RoosterBot.GLU.Resources");

			ScheduleService provider = services.GetRequiredService<ScheduleService>();
			StaffMemberService staffMembers = services.GetService<StaffMemberService>();

			foreach (ScheduleRegistryInfo sri in m_Schedules) {
				provider.RegisterProvider(sri.IdentifierType, new MemoryScheduleProvider(sri.Name, new GLUScheduleReader(sri.Path, staffMembers, m_AllowedChannels[0], m_SkipPastRecords), m_AllowedChannels));
			}

			// Student sets and Rooms validator
			services.GetRequiredService<IdentifierValidationService>().RegisterValidator(new GLUIdentifierValidator(m_AllowedChannels));

			var identifierReaders = commands.GetSpecificTypeParser<IdentifierInfo, MultiParser<IdentifierInfo>>();
			if (identifierReaders == null) {
				throw new InvalidOperationException("GLU must be installed after Schedule");
			}

			identifierReaders.AddParser(new StudentSetInfoParser());
			identifierReaders.AddParser(new StaffMemberInfoParser());
			identifierReaders.AddParser(new RoomInfoParser());

			commands.AddModule<StaffMemberModule>();
		}

		private class GLUIdentifierValidator : IdentifierValidator {
			private static readonly Regex StudentSetRegex = new Regex("^[1-4]G[AD][12]$");
			private static readonly Regex RoomRegex = new Regex("[aAbBwW][0-4][0-9]{2}");

			public GLUIdentifierValidator(IReadOnlyCollection<SnowflakeReference> allowedChannels) : base(allowedChannels) { }

			public override Task<IdentifierInfo?> ValidateAsync(RoosterCommandContext context, string input) {
				input = input.ToUpper();
				IdentifierInfo? result = null;
				if (StudentSetRegex.IsMatch(input)) {
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
