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
		private readonly SqlCommand m_SetClassCommand;

		public SqlDatabaseUserClassesService(string configFile) {
			JObject jsonConfig = JObject.Parse(File.ReadAllText(configFile));

			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder {
				DataSource		= jsonConfig["server"]	.ToObject<string>(),
				UserID			= jsonConfig["userId"]	.ToObject<string>(),
				Password		= jsonConfig["password"].ToObject<string>(),
				InitialCatalog	= jsonConfig["database"].ToObject<string>()
			};

			m_SQL = new SqlConnection(builder.ConnectionString);

			m_GetClassCommand = new SqlCommand("SELECT UserClass FROM DiscordUsers WHERE UserId = @UserId LIMIT 1", m_SQL);
			m_SetClassCommand = new SqlCommand("UPDATE DiscordUsers SET UserClass = @UserClass WHERE UserId = @UserId LIMIT 1", m_SQL);
			
			m_GetClassCommand.Parameters.Add("@UserId",    SqlDbType.BigInt);
			m_SetClassCommand.Parameters.Add("@UserId",    SqlDbType.BigInt);
			m_SetClassCommand.Parameters.Add("@UserClass", SqlDbType.VarChar, 5);

			m_GetClassCommand.Prepare();
			m_SetClassCommand.Prepare();
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
			Task open = m_SQL.OpenAsync();
			
			m_GetClassCommand.Parameters["@UserId"]   .Value = user.Id;
			m_SetClassCommand.Parameters["@UserId"]   .Value = user.Id;
			m_SetClassCommand.Parameters["@UserClass"].Value = ssi.ClassName;

			await open;

			string old = (string) await m_GetClassCommand.ExecuteScalarAsync();
			await m_SetClassCommand.ExecuteNonQueryAsync();

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

		public void Dispose() {
			m_SetClassCommand.Dispose();
			m_GetClassCommand.Dispose();
			m_SQL.Dispose();
		}
	}
}
