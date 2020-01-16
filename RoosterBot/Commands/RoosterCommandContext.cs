using System.Globalization;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	public class RoosterCommandContext : CommandContext {
		public PlatformComponent Platform { get; }
		public IMessage Message { get; }
		public IUser User { get; }
		public IChannel Channel { get; }
		public bool IsPrivate { get; }

		public UserConfig UserConfig { get; }
		public ChannelConfig ChannelConfig { get; }
		public CultureInfo Culture => UserConfig.Culture ?? ChannelConfig.Culture;

		public RoosterCommandContext(PlatformComponent platform, IMessage message, UserConfig userConfig, ChannelConfig channelConfig) : base(Program.Instance.Components.Services) {
			Platform = platform;
			Message = message;
			User = message.User;
			Channel = message.Channel;

			UserConfig = userConfig;
			ChannelConfig = channelConfig;
		}

		public override string ToString() {
			return $"{User.UserName} in channel `{Channel.Name}`: {Message.Content}";
		}

		/// <summary>
		/// Convert a <see cref="RoosterCommandResult"/> to a string that can be passed into this context's <see cref="IChannel.SendMessageAsync(string, string)"/>.
		/// </summary>
		public async Task<IMessage> RespondAsync(RoosterCommandResult result) {
			//Channel.SendMessageAsync(result.ToString(this), result.UploadFilePath);
			CommandResponsePair? crp = UserConfig.GetResponse(Message);
			IMessage? response = crp == null ? null : await Channel.GetMessageAsync(crp.Response.Id);
			if (response == null) {
				// The response was already deleted, or there was no response to begin with.
				response = await SendResultAsync(result);
				UserConfig.SetResponse(Message, response);
			} else {
				// The command was edited.
				await response.ModifyAsync(result.ToString(this), result.UploadFilePath);
			}

			return response;
		}

		protected virtual Task<IMessage> SendResultAsync(RoosterCommandResult result) {
			return Channel.SendMessageAsync(result.ToString(this), result.UploadFilePath);
		}
	}
}
