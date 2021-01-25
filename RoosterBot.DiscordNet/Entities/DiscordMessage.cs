using System;
using System.Threading.Tasks;

namespace RoosterBot.DiscordNet {
	public class DiscordMessage : IMessage {
		public Discord.IUserMessage DiscordEntity { get; }

		public PlatformComponent Platform => DiscordNetComponent.Instance;

		public object Id => DiscordEntity.Id;
		public IChannel Channel => new DiscordChannel(DiscordEntity.Channel);
		public IUser User => new DiscordUser(DiscordEntity.Author);
		public string Content => DiscordEntity.Content;
		public DateTimeOffset SentAt => DiscordEntity.EditedTimestamp ?? DiscordEntity.CreatedAt;

		internal DiscordMessage(Discord.IUserMessage discordMessage) {
			DiscordEntity = discordMessage;
		}

		public Task DeleteAsync() => DiscordEntity.DeleteAsync();
	}
}
