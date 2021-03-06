﻿using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.DiscordNet {
	public class MessageParser<TMessage> : RoosterTypeParser<TMessage> where TMessage : class, Discord.IMessage {
		public override string TypeDisplayName => "#MessageParser_Name";

		public async override ValueTask<RoosterTypeParserResult<TMessage>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			if (context.Channel is DiscordChannel channel && (Discord.MentionUtils.TryParseChannel(input, out ulong messageId) || ulong.TryParse(input, out messageId))) {
				// By id
				Discord.IMessage message = await channel.DiscordEntity.GetMessageAsync(messageId);
				if (message == null) {
					return Unsuccessful(true, "#MessageParser_UnknownChannel");
				} else if (!(message is TMessage tMessage)) {
					return Unsuccessful(true, "#DiscordParser_InvalidType");
				} else {
					return Successful(tMessage);
				}
			} else if (Uri.TryCreate(input, UriKind.Absolute, out Uri? result)) {
				// By message link
				string host = result.GetComponents(UriComponents.Host, UriFormat.Unescaped);
				if (host == "discordapp.com" || host == "discord.com") {
					string[] pathComponents = result.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped).Substring(1).Split('/');
					if (pathComponents.Length == 4 && pathComponents[0] == "channels" &&
						ulong.TryParse(pathComponents[1], out _) &&
						ulong.TryParse(pathComponents[2], out ulong channelId) &&
						ulong.TryParse(pathComponents[3], out messageId)) {
						if (DiscordNetComponent.Instance.Client.GetChannel(channelId) is Discord.IMessageChannel messageChannel) {
							if (await messageChannel.GetMessageAsync(messageId) is TMessage message) {
								return Successful(message);
							} else {
								return Unsuccessful(true, "#DiscordParser_InvalidType");
							}
						} else {
							return Unsuccessful(true, "#MessageParser_UnknownChannel");
						}
					}
				}
				return Unsuccessful(true, "#MessageParser_InvalidUri");
			} else {
				return Unsuccessful(false, "#MessageParser_InvalidMention");
			}
		}
	}
}
