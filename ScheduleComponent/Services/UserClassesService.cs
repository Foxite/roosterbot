using Discord;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace ScheduleComponent.Services {
	public class UserClassesService {
		private AmazonDynamoDBClient m_Client;
		private Regex m_StudentSetRegex = new Regex("^[1-4]G[AD][12]$");
		private Table m_Table;

		public UserClassesService(string keyId, string secretKey) {
			m_Client = new AmazonDynamoDBClient(keyId, secretKey, Amazon.RegionEndpoint.EUWest1);
			m_Table = Table.LoadTable(m_Client, "roosterbot-userclasses");
		}

		public async Task<StudentSetInfo> GetClassForDiscordUser(IUser user) {
			Document document = await m_Table.GetItemAsync(user.Id);
			return new StudentSetInfo() { ClassName = document["class"].AsString() };
		}

		public async Task SetClassForDiscordUser(IUser user, string clazz) {
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
	}
}
