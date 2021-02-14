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

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity(typeof(EFChannel)).HasKey(nameof(EFChannel.Platform), nameof(EFChannel.PlatformId));
			modelBuilder.Entity(typeof(EFUser)).HasKey(nameof(EFUser.Platform), nameof(EFUser.PlatformId));
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
		public PostgresProvider(
			[JsonProperty("Host")] string host,
			[JsonProperty("Port")] int port,
			[JsonProperty("Username")] string username,
			[JsonProperty("Password")] string password,
			[JsonProperty("Database")] string name)
			: base($"host={host};database={name};user id={username};password={password}") { }

		public override void ConfigureContext(DbContextOptionsBuilder optionsBuilder) {
			optionsBuilder.UseNpgsql(
				ConnectionString
			);
		}
	}
}
