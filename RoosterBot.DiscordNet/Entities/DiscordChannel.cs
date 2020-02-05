using System;
using System.Threading.Tasks;

namespace RoosterBot.DiscordNet {
	public class DiscordChannel : IChannel {
		public Discord.IMessageChannel DiscordEntity { get; }
		public PlatformComponent Platform => DiscordNetComponent.Instance;

		public object Id => DiscordEntity.Name;
		public string Name => (DiscordEntity is Discord.IGuildChannel igc ? (igc.Guild + "/") : "DM with ") + DiscordEntity.Name;

		public bool IsPrivate => DiscordEntity is Discord.IPrivateChannel;

		public async Task<IMessage> GetMessageAsync(object id) {
			if (id is ulong ulongId) {
				if (await DiscordEntity.GetMessageAsync(ulongId) is Discord.IUserMessage message) {
					return new DiscordMessage(message);
				} else {
					throw new SnowflakeNotFoundException("No message with ID " + ulongId + " exists in this channel.");
				}
			} else {
				throw new ArgumentException("ID must be ulong for Discord.NET entities.", nameof(id));
			}
		}

		public async Task<IMessage> SendMessageAsync(string content, string? filePath = null) {
			if (filePath == null) {
				return new DiscordMessage(await DiscordEntity.SendMessageAsync(content));
			} else {
				return new DiscordMessage(await DiscordEntity.SendFileAsync(filePath, content));
			}
		}

		internal DiscordChannel(Discord.IMessageChannel discordChannel) {
			DiscordEntity = discordChannel;
		}
	}
}
