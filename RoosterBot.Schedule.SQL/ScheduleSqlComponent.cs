using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RoosterBot.Schedule.SQL {
	public class ScheduleSqlComponent : ComponentBase {
#nullable disable
		private SqlDatabaseUserClassesService m_UCS;
#nullable restore

		public override Version ComponentVersion => new Version(0, 1, 0);
		public override IEnumerable<string> Tags => new[] { "UserClassesService" };

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			m_UCS = new SqlDatabaseUserClassesService(Path.Combine(configPath, "Config.json"));
			services.AddSingleton<IUserClassesService>(m_UCS);
			return Task.CompletedTask;
		}

		protected override void Dispose(bool disposing) {
			m_UCS.Dispose();
		}
	}
}
