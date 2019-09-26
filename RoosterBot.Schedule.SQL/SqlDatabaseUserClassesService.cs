using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Data;

namespace RoosterBot.Schedule.SQL {
	public class SqlDatabaseUserClassesService : IUserClassesService, IDisposable {
		private readonly SqlConnection m_SQL;
		private readonly SqlCommand m_GetClassCommand;
		private readonly SqlCommand m_UpdateClassCommand;
		private readonly SqlCommand m_InsertClassCommand;

		public SqlDatabaseUserClassesService(string configFile) {
			JObject jsonConfig = JObject.Parse(File.ReadAllText(configFile));

			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder {
				DataSource		= jsonConfig["server"]	.ToObject<string>(),
				UserID			= jsonConfig["userId"]	.ToObject<string>(),
				Password		= jsonConfig["password"].ToObject<string>(),
				InitialCatalog	= jsonConfig["database"].ToObject<string>()
			};

			m_SQL = new SqlConnection(builder.ConnectionString);

			m_GetClassCommand = new SqlCommand("SELECT TOP (1) UserClass FROM DiscordUsers WHERE UserId = @UserId", m_SQL);
			m_UpdateClassCommand = new SqlCommand("UPDATE TOP (1) DiscordUsers SET UserClass = @UserClass WHERE UserId = @UserId", m_SQL);
			m_InsertClassCommand = new SqlCommand("INSERT INTO DiscordUsers (UserId, UserClass) VALUES (@UserId, @UserClass)", m_SQL);
			
			m_GetClassCommand.Parameters.Add("@UserId",    SqlDbType.BigInt);
			m_UpdateClassCommand.Parameters.Add("@UserId",    SqlDbType.BigInt);
			m_UpdateClassCommand.Parameters.Add("@UserClass", SqlDbType.VarChar, 5);
			m_InsertClassCommand.Parameters.Add("@UserId",    SqlDbType.BigInt);
			m_InsertClassCommand.Parameters.Add("@UserClass", SqlDbType.VarChar, 5);
		}

		public event Action<IUser, StudentSetInfo, StudentSetInfo> UserChangedClass;

		public async Task<StudentSetInfo> GetClassForDiscordUserAsync(ICommandContext context, IUser user) {
			Task open = m_SQL.OpenAsync();

			m_GetClassCommand.Parameters["@UserId"].Value = user.Id;

			await open;

			string ssi = (string) await m_GetClassCommand.ExecuteScalarAsync();

			m_SQL.Close();

			return GetSSIFromString(ssi);
		}

		public async Task<StudentSetInfo> SetClassForDiscordUserAsync(ICommandContext context, IUser user, StudentSetInfo ssi) {
			await m_SQL.OpenAsync();
			
			m_GetClassCommand.Parameters["@UserId"].Value = user.Id;

			string old = (string) await m_GetClassCommand.ExecuteScalarAsync();

			SqlCommand command;
			if (old == null) {
				command = m_InsertClassCommand;
			} else {
				command = m_UpdateClassCommand;
			}
			command.Parameters["@UserId"]   .Value = user.Id;
			command.Parameters["@UserClass"].Value = ssi.ClassName;
			await command.ExecuteNonQueryAsync();

			m_SQL.Close();

			StudentSetInfo oldSSI = GetSSIFromString(old);
			UserChangedClass?.Invoke(user, oldSSI, ssi);

			return oldSSI;
		}

		private StudentSetInfo GetSSIFromString(string str) {
			if (str == null) {
				return null;
			} else {
				return new StudentSetInfo() {
					ClassName = str
				};
			}
		}

		#region IDisposable Support
		private bool m_Disposed = false;

		protected virtual void Dispose(bool disposing) {
			if (!m_Disposed) {
				if (disposing) {
					m_GetClassCommand.Dispose();
					m_UpdateClassCommand.Dispose();
					m_InsertClassCommand.Dispose();
					m_SQL.Dispose();
				}

				m_Disposed = true;
			}
		}

		public void Dispose() {
			Dispose(true);
		}
		#endregion

	}
}
