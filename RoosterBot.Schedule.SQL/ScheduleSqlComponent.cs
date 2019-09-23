using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;

namespace RoosterBot.Schedule.SQL {
	public class ScheduleSqlComponent : ComponentBase {
		private SqlDatabaseUserClassesService m_UCS;

		public override Version ComponentVersion => new Version(0, 1, 0);

		public override string[] Tags => new[] { "IUserClassesService" };

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			m_UCS = new SqlDatabaseUserClassesService(Path.Combine(configPath, "Config.json"));
			services.AddSingleton<IUserClassesService>(m_UCS);
			return Task.CompletedTask;
		}

		public override Task ShutdownAsync() {
			m_UCS.Dispose();
			return Task.CompletedTask;
		}
	}
}
