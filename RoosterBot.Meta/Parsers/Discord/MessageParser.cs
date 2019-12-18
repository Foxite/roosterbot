﻿using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	public class MessageParser<TMessage> : RoosterTypeParser<TMessage> where TMessage : class, IMessage {
		public override string TypeDisplayName => "#MessageParser_Name";

		protected override ValueTask<RoosterTypeParserResult<TMessage>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			/* // TODO Discord
			if (MentionUtils.TryParseChannel(input, out ulong messageId) || ulong.TryParse(input, out messageId)) {
				// By id
				var channel = (TMessage?) await context.Channel.GetMessageAsync(messageId);
				if (channel == null) {
					return Unsuccessful(true, context, "#MessageParser_UnknownChannel");
				} else if (!(channel is TMessage)) {
					return Unsuccessful(true, context, "#DiscordParser_InvalidType");
				} else {
					return Successful(channel);
				}
			} else if (Uri.TryCreate(input, UriKind.Absolute, out Uri? result)) {
				// By message link
				if (result.GetComponents(UriComponents.Host, UriFormat.Unescaped) == "discordapp.com") {
					string[] pathComponents = result.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped).Substring(1).Split('/');
					if (pathComponents.Length == 4 && pathComponents[0] == "channels" &&
						ulong.TryParse(pathComponents[1], out _) &&
						ulong.TryParse(pathComponents[2], out ulong channelId) &&
						ulong.TryParse(pathComponents[3], out messageId)) {
						if (await context.Client.GetChannelAsync(channelId) is IMessageChannel channel) {
							if (await channel.GetMessageAsync(messageId) is TMessage message) {
								return Successful(message);
							} else {
								return Unsuccessful(true, context, "#DiscordParser_InvalidType");
							}
						} else {
							return Unsuccessful(true, context, "#MessageParser_UnknownChannel");
						}
					}
				}
				return Unsuccessful(true, context, "#MessageParser_InvalidUri");
			} else {
				return Unsuccessful(false, context, "#MessageParser_InvalidMention");
			}
			*/
			throw new NotImplementedException();
		}
	}
}
