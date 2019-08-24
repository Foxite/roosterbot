using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule.AWS {
	public class DynamoDBUserClassesService : IUserClassesService, IDisposable {
		private AmazonDynamoDBClient m_Client;
		private Regex m_StudentSetRegex = new Regex("^[1-4]G[AD][12]$"); // TODO components should be able to add their own student set patterns
		private Table m_Table;

		public DynamoDBUserClassesService(string keyId, string secretKey, RegionEndpoint endpoint, string tableName) {
			Logger.Info("UserClasses", "Connecting to database");
			m_Client = new AmazonDynamoDBClient(keyId, secretKey, endpoint);
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

		public async Task SetClassForDiscordUserAsync(ICommandContext context, IUser user, string clazz) {
			if (m_StudentSetRegex.IsMatch(clazz)) {
				Document document = await m_Table.GetItemAsync(user.Id);
				if (document is null) {
					document = new Document(new Dictionary<string, DynamoDBEntry>() {
						{ "id", DynamoDBEntryConversion.V2.ConvertToEntry(user.Id) },
						{ "class", DynamoDBEntryConversion.V2.ConvertToEntry(clazz.ToUpper()) }
					});
					await m_Table.PutItemAsync(document);
				} else {
					document["class"] = clazz;
					await m_Table.UpdateItemAsync(document);
				}
			} else {
				throw new ArgumentException(clazz + " is not a valid StudentSet.");
			}
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing) {
			if (!disposedValue) {
				if (disposing) {
					m_Client.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose() {
			Dispose(true);
		}
		#endregion
	}
}
