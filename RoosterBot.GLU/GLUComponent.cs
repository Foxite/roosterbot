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
		private SnowflakeReference[] m_AllowedChannels;
		private string m_StaffMemberPath;
		private bool m_SkipPastRecords;

		public override Version ComponentVersion => new Version(1, 1, 1);
		public override IEnumerable<string> Tags => new[] { "ScheduleProvider" };

		public GLUComponent() {
			m_Schedules = new List<ScheduleRegistryInfo>();
			m_AllowedChannels = Array.Empty<SnowflakeReference>();
			m_StaffMemberPath = "";
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
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commands) {
			services.GetRequiredService<ResourceService>().RegisterResources("RoosterBot.GLU.Resources");

			// Staff members
			StaffMemberService members = services.GetRequiredService<StaffMemberService>();
			members.AddStaff(new GLUStaffMemberReader(m_StaffMemberPath).ReadCSV(), m_AllowedChannels);

			ScheduleService provider = services.GetRequiredService<ScheduleService>();

			foreach (ScheduleRegistryInfo sri in m_Schedules) {
				provider.RegisterProvider(sri.IdentifierType, new MemoryScheduleProvider(sri.Name, new GLUScheduleReader(sri.Path, members, m_AllowedChannels[0], m_SkipPastRecords), m_AllowedChannels));
			}

			// Student sets and Rooms validator
			services.GetRequiredService<IdentifierValidationService>().RegisterValidator(ValidateIdentifier);
		}

		private Task<IdentifierInfo?> ValidateIdentifier(RoosterCommandContext context, string input) {
			if (m_AllowedChannels.Contains(context.ChannelConfig.ChannelReference)) {
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
