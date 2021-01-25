using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoosterBot.AWS {
	public class DynamoDBChannelConfigService : ChannelConfigService {
		private readonly Table m_Table;

		public DynamoDBChannelConfigService(AmazonDynamoDBClient client, string tableName, string defaultCommandPrefix, CultureInfo defaultCulture)
			: base(defaultCommandPrefix, defaultCulture) {
			Logger.Info(AWSComponent.LogTag, "Loading channel table");
			m_Table = Table.LoadTable(client, tableName);
			Logger.Info(AWSComponent.LogTag, "Finished loading channel table");
		}

		public async override Task<ChannelConfig> GetConfigAsync(SnowflakeReference channel) {
			string id = channel.Platform.PlatformName + "/" + channel.Id.ToString();
			Document document = await m_Table.GetItemAsync(id);
			if (document != null) {
				if (!document.TryGetValue("culture", out DynamoDBEntry cultureEntry) ||
					!document.TryGetValue("commandPrefix", out DynamoDBEntry prefixEntry) ||
					!document.TryGetValue("customData", out DynamoDBEntry customDataEntry)) {
					return GetDefaultConfig(channel);
				} else {
					var culture = CultureInfo.GetCultureInfo(cultureEntry.AsString());
					string commandPrefix = prefixEntry.AsString();
					var customData = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(customDataEntry.AsString());

					IEnumerable<string> disabledModules;
					if (customData.TryGetValue("dynamodb.disabledModules", out JToken? value)) {
						disabledModules = value.ToObject<JArray>()!.Select(token => token.ToObject<string>()).WhereNotNull();
					} else {
						disabledModules = Array.Empty<string>();
					}

					return new ChannelConfig(this, commandPrefix, culture, channel, customData, disabledModules);
				}
			} else {
				return GetDefaultConfig(channel);
			}
		}

		public async override Task UpdateChannelAsync(ChannelConfig config) {
			config.SetData("dynamodb.disabledModules", config.DisabledModules);

			string id = config.ChannelReference.Platform.PlatformName + "/" + config.ChannelReference.Id.ToString();
			Document document = await m_Table.GetItemAsync(id);
			if (document is null) {
				await m_Table.PutItemAsync(new Document(new Dictionary<string, DynamoDBEntry>() {
					{ "id", id },
					{ "culture", config.Culture.Name },
					{ "commandPrefix", config.CommandPrefix },
					{ "customData", config.GetRawData().ToString(Formatting.None) }
				}));
			} else {
				// We can assume the channel ID won't change
				document["culture"] = config.Culture.Name;
				document["commandPrefix"] = config.CommandPrefix;
				document["customData"] = config.GetRawData().ToString(Formatting.None);

				await m_Table.UpdateItemAsync(document);
			}
		}
	}
}
