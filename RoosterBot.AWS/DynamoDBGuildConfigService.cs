using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoosterBot.AWS {
	// TODO (refactor) Improve database structure
	// This currently indexes SnowflakeReferences by sr.Platform.PlatformName + "/" + sr.Id.ToString().
	// That's not great and to change it requires changing the database structure.
	public class DynamoDBGuildConfigService : ChannelConfigService {
		private readonly Table m_Table;

		public DynamoDBGuildConfigService(GlobalConfigService configService, AmazonDynamoDBClient client, string tableName) : base(configService) {
			Logger.Info("DynamoDBGuild", "Loading guild table");
			m_Table = Table.LoadTable(client, tableName);
			Logger.Info("DynamoDBGuild", "Finished loading guild table");
		}

		public async override Task<ChannelConfig> GetConfigAsync(SnowflakeReference channel) {
			string id = channel.Platform.PlatformName + "/" + channel.Id.ToString();
			Document document = await m_Table.GetItemAsync(id);
			if (document != null) {
				if (!document.TryGetValue("culture", out DynamoDBEntry cultureEntry) ||
					!document.TryGetValue("commandPrefix", out DynamoDBEntry prefixEntry) ||
					!document.TryGetValue("timeZoneId", out DynamoDBEntry timeZoneEntry) ||
					!document.TryGetValue("customData", out DynamoDBEntry customDataEntry)) {
					return GetDefaultConfig(channel);
				} else {
					TimeZoneInfo timezone;
					try {
						timezone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneEntry.AsString());
					} catch (TimeZoneNotFoundException) {
						return GetDefaultConfig(channel);
					}
					var culture = CultureInfo.GetCultureInfo(cultureEntry.AsString());
					string commandPrefix = prefixEntry.AsString();
					var customData = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(customDataEntry.AsString());

					return new ChannelConfig(this, commandPrefix, culture, channel, customData);
				}
			} else {
				return GetDefaultConfig(channel);
			}
		}

		// In the future, guild staff will modify their settings on a website, and there will be no way to update this through commands.
		public async override Task UpdateGuildAsync(ChannelConfig config) {
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
				// We can assume the guild ID won't change
				document["culture"] = config.Culture.Name;
				document["commandPrefix"] = config.CommandPrefix;
				document["customData"] = config.GetRawData().ToString(Formatting.None);

				await m_Table.UpdateItemAsync(document);
			}
		}
	}
}
