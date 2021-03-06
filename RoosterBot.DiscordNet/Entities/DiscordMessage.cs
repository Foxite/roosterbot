﻿using System;
using System.Threading.Tasks;

namespace RoosterBot.DiscordNet {
	public class DiscordMessage : IMessage {
		public Discord.IUserMessage DiscordEntity { get; }
		public bool UpsideDown { get; }

		public PlatformComponent Platform => DiscordNetComponent.Instance;

		public object Id => DiscordEntity.Id;
		public IChannel Channel => new DiscordChannel(DiscordEntity.Channel);
		public IUser User => new DiscordUser(DiscordEntity.Author);
		public string Content => UpsideDown ? DiscordEntity.Content.UpsideDown() : DiscordEntity.Content;
		public DateTimeOffset SentAt => DiscordEntity.EditedTimestamp ?? DiscordEntity.CreatedAt;

		public Task DeleteAsync() => DiscordEntity.DeleteAsync();

		// TODO (block) (fix) this currently breaks contract as there is no way to attach a file after it was sent.
		// We could create a new message and delete the old one but that would break contract as well.
		// I'm not sure what to do about this but something needs to be done.
		public Task ModifyAsync(string newContent, string? filePath) => DiscordEntity.ModifyAsync(props => {
			props.Content = newContent;
			props.Embed = null;
		});

		internal DiscordMessage(Discord.IUserMessage discordMessage, bool upsideDown = false) {
			DiscordEntity = discordMessage;
			UpsideDown = upsideDown;
		}
	}
}
