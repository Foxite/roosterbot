using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Discord;
using Discord.Commands;
using RoosterBot.AWS;

namespace RoosterBot.Schedule.AWS {
	public class DynamoDBUserClassesService : IUserClassesService, IDisposable {
		private AmazonDynamoDBClient m_Client;
		private Table m_Table;

		public event Action<IGuildUser, StudentSetInfo, StudentSetInfo> UserChangedClass;

		public DynamoDBUserClassesService() { }

		public void Initialize(AWSConfigService config, string tableName) {
			Logger.Info("UserClasses", "Connecting to database");
			m_Client = new AmazonDynamoDBClient(config.Credentials, new AmazonDynamoDBConfig() {
				RegionEndpoint = config.Region
			});
			Logger.Info("UserClasses", "Loading user table");
			m_Table = Table.LoadTable(m_Client, tableName);
			Logger.Info("UserClasses", "UserClassesService loaded");
		}

		public async Task<StudentSetInfo> GetClassForDiscordUserAsync(ICommandContext context, IUser user) {
			Document document = await m_Table.GetItemAsync(user.Id);
			if (document != null) {
				return new StudentSetInfo() { ClassName = document["class"].AsString() };
			} else {
				return null;
			}
		}

		public async Task<StudentSetInfo> SetClassForDiscordUserAsync(ICommandContext context, IGuildUser user, StudentSetInfo ssi) {
			Document document = await m_Table.GetItemAsync(user.Id);
			if (document is null) {
				document = new Document(new Dictionary<string, DynamoDBEntry>() {
					{ "id", DynamoDBEntryConversion.V2.ConvertToEntry(user.Id) },
					{ "class", DynamoDBEntryConversion.V2.ConvertToEntry(ssi.ScheduleCode) }
				});
				await m_Table.PutItemAsync(document);
				UserChangedClass?.Invoke(user, null, ssi);
				return null;
			} else {
				StudentSetInfo old = new StudentSetInfo() { ClassName = document["class"] };
				document["class"] = ssi.ScheduleCode;
				await m_Table.UpdateItemAsync(document);
				UserChangedClass?.Invoke(user, null, ssi);
				return old;
			}
		}

		#region IDisposable Support
		private bool m_DisposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing) {
			if (!m_DisposedValue) {
				if (disposing) {
					m_Client.Dispose();
				}

				m_DisposedValue = true;
			}
		}

		public void Dispose() {
			Dispose(true);
		}
		#endregion
	}
}
