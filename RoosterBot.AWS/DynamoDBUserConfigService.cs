using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoosterBot.AWS {
	public class DynamoDBUserConfigService : UserConfigService {
		private readonly Table m_Table;

		public DynamoDBUserConfigService(AmazonDynamoDBClient client, string tableName) {	
			Logger.Info("DynamoDBUser", "Loading user table");
			m_Table = Table.LoadTable(client, tableName);
			Logger.Info("DynamoDBUser", "Finished loading user table");
		}

		public async override Task<UserConfig> GetConfigAsync(SnowflakeReference user) {
			Document document = await m_Table.GetItemAsync(user.Platform.PlatformName + "/" + user.Id.ToString());
			if (document != null) {
				CultureInfo? culture = (document.TryGetValue("culture", out DynamoDBEntry cultureEntry) && cultureEntry.AsString() != " " ) ? CultureInfo.GetCultureInfo(cultureEntry.AsString()) : null;
				document.TryGetValue("customData", out DynamoDBEntry customDataEntry);
				var customData = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(customDataEntry.AsString());
				return new UserConfig(this, culture, user, customData);
			} else {
				return GetDefaultConfig(user);
			}
		}

		public async override Task UpdateUserAsync(UserConfig config) {
			string id = config.UserReference.Platform.PlatformName + "/" + config.UserReference.Id.ToString();
			Document document = await m_Table.GetItemAsync(id);
			if (document is null) {
				document = new Document(new Dictionary<string, DynamoDBEntry>() {
					{ "id", id },
					{ "customData", config.GetRawData().ToString(Formatting.None) }
				});
				
				if (config.Culture != null) {
					document["culture"] = config.Culture.Name;
				}

				await m_Table.PutItemAsync(document);
			} else {
				// We can assume the user ID won't change
				if (config.Culture != null) {
					document["culture"] = config.Culture.Name;
				} else if (config.Culture == null) {
					document.Remove("culture");
				}
				document["customData"] = config.GetRawData().ToString(Formatting.None);

				await m_Table.UpdateItemAsync(document);
			}
		}
	}
}
