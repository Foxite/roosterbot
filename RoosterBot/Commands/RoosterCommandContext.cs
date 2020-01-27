using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// The object that represents the context in which a command is being exeucted. It persists from message reception to post-command handling, and it holds the message being responded to,
	/// as well as the <see cref="PlatformComponent"/> which received the message that originated this context.
	/// It also holds the <see cref="RoosterBot.UserConfig"/> for the <see cref="User"/> and the <see cref="RoosterBot.ChannelConfig"/> for the <see cref="Channel"/>, which is
	/// often accessed by multiple unrelated classes in the execution process, so that these classes do not have to retrieve the config data independently.
	/// </summary>
	public class RoosterCommandContext : CommandContext {
		/// <summary>
		/// The <see cref="PlatformComponent"/> that started execution.
		/// </summary>
		public PlatformComponent Platform { get; }

		/// <summary>
		/// The <see cref="IMessage"/> being responded to.
		/// </summary>
		public IMessage Message { get; }

		/// <summary>
		/// The <see cref="IUser"/> who sent the message being responded to.
		/// </summary>
		public IUser User { get; }
		
		/// <summary>
		/// The <see cref="IChannel"/> in which the <see cref="Message"/> was sent.
		/// </summary>
		public IChannel Channel { get; }

		/// <summary>
		/// Indicates if the context is private or not. Communication in a private context is only visible to the <see cref="User"/> and RoosterBot.
		/// </summary>
		public bool IsPrivate { get; }

		/// <summary>
		/// The <see cref="RoosterBot.UserConfig"/> for the <see cref="User"/>.
		/// </summary>
		public UserConfig UserConfig { get; }
		
		/// <summary>
		/// The <see cref="RoosterBot.ChannelConfig"/> for the <see cref="Channel"/>.
		/// </summary>
		public ChannelConfig ChannelConfig { get; }

		/// <summary>
		/// Shorthand for <code><see cref="UserConfig.Culture"/> ?? <see cref="ChannelConfig.Culture"/></code>
		/// </summary>
		public CultureInfo Culture => UserConfig.Culture ?? ChannelConfig.Culture;

		/// <summary>
		/// Construct a new <see cref="RoosterCommandContext"/> with the necessary information.
		/// </summary>
		public RoosterCommandContext(PlatformComponent platform, IMessage message, UserConfig userConfig, ChannelConfig channelConfig) : base(Program.Instance.Components.Services) {
			Platform = platform;
			Message = message;
			User = message.User;
			Channel = message.Channel;

			UserConfig = userConfig;
			ChannelConfig = channelConfig;
		}

		/// <summary>
		/// Return a string representation of this context, useful for logging purposes.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return $"{User.UserName} in {Platform.PlatformName} channel `{Channel.Name}`: {Message.Content}";
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
				try {
					response = await SendResultAsync(result, response);
				} catch (Exception e) {
					Logger.Error("Result", "Error was caught in SendResultAsync. Sending a generic error result", e);
					response = await SendResultAsync(TextResult.Error(ServiceProvider.GetService<ResourceService>().GetString(Culture, "CommandHandling_FatalError")), response);
				}
				UserConfig.SetResponse(Message, response);
			} else {
				// The command was edited.
				await response.ModifyAsync(result.ToString(this), result.UploadFilePath);
			}

			return response;
		}

		/// <summary>
		/// Send the result to the channel. You may override this for your platform to provide custom presentations of built-in or external <see cref="RoosterCommandResult"/> types.
		/// </summary>
		protected virtual Task<IMessage> SendResultAsync(RoosterCommandResult result, IMessage? existingResponse) {
			if (existingResponse == null) {
				return Channel.SendMessageAsync(result.ToString(this), result.UploadFilePath);
			} else {
				existingResponse.ModifyAsync(result.ToString(this), result.UploadFilePath);
				return Task.FromResult(existingResponse);
			}
		}
	}
}
