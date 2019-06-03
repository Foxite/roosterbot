using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Discord;
using RoosterBot;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Services {
	public class UserClassesService : IDisposable {
		private AmazonDynamoDBClient m_Client;
		private Regex m_StudentSetRegex = new Regex("^[1-4]G[AD][12]$");
		private Table m_Table;

		public UserClassesService(string keyId, string secretKey) {
			m_Client = new AmazonDynamoDBClient(keyId, secretKey, Amazon.RegionEndpoint.EUWest1);
			m_Table = Table.LoadTable(m_Client, "roosterbot-userclasses");
		}

		public async Task<StudentSetInfo> GetClassForDiscordUser(ulong userId) {
			Document document = await m_Table.GetItemAsync(userId);
			if (document != null) {
				return new StudentSetInfo() { ClassName = document["class"].AsString() };
			} else {
				return null;
			}
		}

		public Task<StudentSetInfo> GetClassForDiscordUser(IUser user) {
			return GetClassForDiscordUser(user.Id);
		}

		public async Task SetClassForDiscordUser(ulong userId, string clazz) {
			if (m_StudentSetRegex.IsMatch(clazz)) {
				Document document = await m_Table.GetItemAsync(userId);
				if (document is null) {
					document = new Document(new Dictionary<string, DynamoDBEntry>() {
						{ "id", DynamoDBEntryConversion.V2.ConvertToEntry(userId) },
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

		public Task SetClassForDiscordUser(IUser user, string clazz) {
			return SetClassForDiscordUser(user.Id, clazz);
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
