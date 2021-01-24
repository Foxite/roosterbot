using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot.DiscordNet {
	public abstract class DiscordResultAdapter<TResult> : ResultAdapter where TResult : RoosterCommandResult {
		public override bool CanHandleResult(RoosterCommandContext context, RoosterCommandResult result) => context is DiscordCommandContext && result is TResult;

		public sealed async override Task<IMessage> HandleResult(RoosterCommandContext context, RoosterCommandResult result) => new DiscordMessage(await HandleResult((DiscordCommandContext) context, (TResult) result));

		protected abstract Task<IUserMessage> HandleResult(DiscordCommandContext context, TResult result);

		/// <param name="context">The context.</param>
		/// <param name="text">Optional text in the message.</param>
		/// <param name="embed">Optional EmbedBuilder to be built and included in the message.</param>
		/// <param name="attachmentStream">Optional open stream with data to be attached. You must specify <paramref name="filename"/> and you may not specify <paramref name="attachmentPath"/>.</param>
		/// <param name="attachmentPath">Optional path to the file to be attached. You must specify <paramref name="filename"/> and you may not specify <paramref name="attachmentStream"/>.</param>
		/// <param name="filename">Optional name of the file to be attached. You may not specify this unless you specified either <paramref name="attachmentStream"/> or <paramref name="attachmentPath"/>.</param>
		protected async Task<IUserMessage> SendMessage(DiscordCommandContext context, string? text = null, EmbedBuilder? embed = null, Stream? attachmentStream = null, string? attachmentPath = null, string? filename = null) {
			if ((attachmentStream is not null && attachmentPath is not null) || // Cannot specify both a stream and a path
				((attachmentStream is not null || attachmentPath is not null) && filename is null) || // Must specify a name if we include an attachment
				(attachmentStream is null && attachmentPath is null && filename is not null)) { // Cannot specify a filename without attachment data
				throw new ArgumentException($"You may specify neither or one of {nameof(attachmentStream)} or {nameof(attachmentPath)}, but not both. You must specify {nameof(filename)} if you use one of them.");
			}

			IMessageChannel channel = context.Command is not null && context.Command.Attributes.Any(attr => attr is RespondInPrivateAttribute) ? await context.User.GetOrCreateDMChannelAsync() : context.Channel;

			if (context.Response == null) {
				IUserMessage message;
				if (filename is null) {
					message = await channel.SendMessageAsync(text: text, embed: embed?.Build(), messageReference: channel == context.Channel ? context.Message.GetReference() : null);
				} else {
					bool openedStream = false;
					try {
						if (attachmentStream is null) {
							openedStream = true;
							attachmentStream = File.OpenRead(attachmentPath!); // The checks up above prevent an NRE
						}

						if (attachmentStream.Length > 8e6) {
							// TODO localize, emote
							return await channel.SendMessageAsync("I tried sending you a file, but it was too large.");
						}

						message = await channel.SendFileAsync(attachmentStream, filename, text: text, embed: embed?.Build(), messageReference: context.Message.GetReference());
					} finally {
						if (openedStream && attachmentStream is not null) {
							await attachmentStream.DisposeAsync();
						}
					}
				}

				if (channel != context.Channel) {
					// No need to return the private message, it won't matter if the user deletes their public command and the bot doesn't delete the DM.
					return await context.Channel.SendMessageAsync("Replied in DM");
				} else {
					return message;
				}
			} else {
				await context.Response.ModifyAsync(props => {
					props.Content = text;
					props.Embed = embed?.Build();
				});
				return context.Response;
			}
		}
	}
}
