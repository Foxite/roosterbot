using System.Threading.Tasks;
using Discord;

namespace RoosterBot.DiscordNet {
	public abstract class DiscordResultAdapter<TResult> : ResultAdapter where TResult : RoosterCommandResult {
		public override bool CanHandleResult(RoosterCommandContext context, RoosterCommandResult result) => context is DiscordCommandContext && result is TResult;

		public sealed async override Task<IMessage> HandleResult(RoosterCommandContext context, RoosterCommandResult result) => new DiscordMessage(await HandleResult((DiscordCommandContext) context, (TResult) result));

		protected abstract Task<IUserMessage> HandleResult(DiscordCommandContext context, TResult result);

		protected Task<IUserMessage> SendMessage(DiscordCommandContext context, string? text = null, EmbedBuilder? embed = null) {
			if (context.Response == null) {
				return context.Channel.SendMessageAsync(text: text, embed: embed?.Build(), messageReference: context.Message.GetReference());
			} else {
				return context.Response.ModifyAsync(props => {
					props.Content = text;
					props.Embed = embed?.Build();
				}).ContinueWith(t => context.Response);
			}
		}
	}
}
