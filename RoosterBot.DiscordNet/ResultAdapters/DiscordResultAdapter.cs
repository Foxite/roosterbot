using System.Threading.Tasks;
using Discord;

namespace RoosterBot.DiscordNet {
	public abstract class DiscordResultAdapter<TResult> : ResultAdapter where TResult : RoosterCommandResult {
		public override bool CanHandleResult(RoosterCommandContext context, RoosterCommandResult result) => context is DiscordCommandContext && result is TResult;

		public sealed async override Task<IMessage> HandleResult(RoosterCommandContext context, RoosterCommandResult result) => new DiscordMessage(await HandleResult((DiscordCommandContext) context, (TResult) result, ((DiscordMessage?) context.Response)?.DiscordEntity));
		protected abstract Task<IUserMessage> HandleResult(DiscordCommandContext context, TResult result, IUserMessage? existingResponse);

		protected Task<IUserMessage> SendMessage(DiscordCommandContext context, IUserMessage? existingResponse, string? text = null, EmbedBuilder? embed = null) {
			if (existingResponse == null) {
				// Can't make this function non-async and return the task, because it returns Task<IUserMessage> and we return Task.
				return context.Channel.SendMessageAsync(text: text, embed: embed?.Build(), messageReference: context.Message.GetReference());
			} else {
				return existingResponse.ModifyAsync(props => {
					props.Content = text;
					props.Embed = embed?.Build();
				}).ContinueWith(t => existingResponse);
			}
		}
	}
 
}
