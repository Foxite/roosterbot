using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Newtonsoft.Json;
using Npgsql;

namespace RoosterBot.Meta {
	internal class EFContext : DbContext {
		private readonly DatabaseProvider m_DbProvider;

		public DbSet<EFChannel> Channels { get; set; } = null!;
		public DbSet<EFUser> Users { get; set; } = null!;

		public EFContext(DatabaseProvider dbProvider) {
			m_DbProvider = dbProvider;
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
			m_DbProvider.ConfigureContext(optionsBuilder);
		}
	}

	public abstract class DatabaseProvider {
		public string ConnectionString { get; }
		
		protected DatabaseProvider(string connectionString) {
			ConnectionString = connectionString;
		}

		public abstract void ConfigureContext(DbContextOptionsBuilder dcob);
	}

	public class PostgresProvider : DatabaseProvider {
		private readonly ProvidePasswordCallback m_PasswordCallback;

		public PostgresProvider(
			[JsonProperty("Host")] string host,
			[JsonProperty("Port")] int port,
			[JsonProperty("Username")] string username,
			[JsonProperty("Password")] string password,
			[JsonProperty("Database")] string name) : base($"Server={host}:{port}/{username};Database={name}") {
			m_PasswordCallback = (_, _, _, _) => password;
		}

		public override void ConfigureContext(DbContextOptionsBuilder optionsBuilder) {
			optionsBuilder.UseNpgsql(
				ConnectionString,
				postgres => postgres.ProvidePasswordCallback(m_PasswordCallback)
			);
		}
	}
}
