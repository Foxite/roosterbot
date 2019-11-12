using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Discord;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace RoosterBot.AWS {
	public class DynamoDBGuildConfigService : GuildConfigService {
		private readonly Table m_Table;

		public DynamoDBGuildConfigService(ConfigService configService, AmazonDynamoDBClient client, string tableName) : base(configService) {
			Logger.Info("DynamoDBGuild", "Loading user table");
			m_Table = Table.LoadTable(client, tableName);
			Logger.Info("DynamoDBGuild", "UserClassesService loaded");
		}

		public async override Task<GuildConfig> GetConfigAsync(IGuild guild) {
			Document document = await m_Table.GetItemAsync(guild.Id);
			if (document != null) {
				if (!document.TryGetValue("culture", out DynamoDBEntry cultureEntry) ||
					!document.TryGetValue("commandPrefix", out DynamoDBEntry prefixEntry) ||
					!document.TryGetValue("customData", out DynamoDBEntry customDataEntry)) {
					return GetDefaultConfig(guild.Id);
				} else {
					CultureInfo culture = CultureInfo.GetCultureInfo(cultureEntry.AsString());
					string commandPrefix = prefixEntry.AsString();
					JObject customData = JObject.Parse(customDataEntry.AsString());

					return new GuildConfig(this, commandPrefix, culture, guild.Id, customData);
				}
			} else {
				return GetDefaultConfig(guild.Id);
			}
		}

		public async override Task UpdateGuildAsync(GuildConfig config) {
			Document document = await m_Table.GetItemAsync(config.GuildId);
			if (document is null) {
				await m_Table.PutItemAsync(new Document(new Dictionary<string, DynamoDBEntry>() {
					{ "id", config.GuildId },
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
