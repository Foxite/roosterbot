using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule.MockUCS {
	public class MockUCSComponent : ComponentBase {
		public override Version ComponentVersion => new Version(1, 0, 0);
		public override IEnumerable<string> Tags => new[] { "UserClassesService" };

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			services.AddSingleton<IUserClassesService>(new MockUserClassesService());
			return Task.CompletedTask;
		}
	}
}
