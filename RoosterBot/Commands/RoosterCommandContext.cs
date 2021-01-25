using System;
using System.Globalization;
using System.Reflection;
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
	public abstract class RoosterCommandContext : CommandContext {
		private ResourceService? m_Resources = null;

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
		public bool IsPrivate => Channel.IsPrivate;

		/// <summary>
		/// The <see cref="RoosterBot.UserConfig"/> for the <see cref="User"/>.
		/// </summary>
		public UserConfig UserConfig { get; }
		
		/// <summary>
		/// The <see cref="RoosterBot.ChannelConfig"/> for the <see cref="Channel"/>.
		/// </summary>
		public ChannelConfig ChannelConfig { get; }

		/// <summary>
		/// The response sent by RoosterBot.
		/// </summary>
		public IMessage? Response { get; protected set; }

		/// <summary>
		/// Shorthand for <code><see cref="UserConfig.Culture"/> ?? <see cref="ChannelConfig.Culture"/></code>
		/// </summary>
		public CultureInfo Culture => UserConfig.Culture ?? ChannelConfig.Culture;

		private ResourceService Resources => m_Resources ??= ServiceProvider.GetRequiredService<ResourceService>();

		/// <summary>
		/// Construct a new <see cref="RoosterCommandContext"/> with the necessary information.
		/// </summary>
		protected RoosterCommandContext(IServiceProvider isp, PlatformComponent platform, IMessage message, UserConfig userConfig, ChannelConfig channelConfig) : base(isp) {
			Platform = platform;
			Message = message;
			User = message.User;
			Channel = message.Channel;

			UserConfig = userConfig;
			ChannelConfig = channelConfig;

			CommandResponsePair? crp = UserConfig.GetResponse(Message);
			Response = crp == null ? null : Channel.GetMessageAsync(crp.Response.Id).Result;
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
			
			if (Response == null) {
				// The response was already deleted, or there was no response to begin with.
				try {
					Response = await SendResultAsync(result);
				} catch (Exception e) {
					Logger.Error(Logger.Tags.Pipeline, "Error was caught in SendResultAsync. Sending a generic error result", e);
					Response = await SendResultAsync(TextResult.Error(GetString("CommandHandling_FatalError")));
				}
				UserConfig.SetResponse(Message, Response);
			} else {
				// The command was edited.
				await SendResultAsync(result);
			}
			await UserConfig.UpdateAsync();

			return Response;
		}

		/// <summary>
		/// Send the result to the channel. You may override this for your platform to provide custom presentations of built-in or external <see cref="RoosterCommandResult"/> types.
		/// </summary>
		protected abstract Task<IMessage> SendResultAsync(RoosterCommandResult result); /* {
			if (existingResponse == null) {
				return Channel.SendMessageAsync(result.ToString(this));
			} else {
				existingResponse.ModifyAsync(result.ToString(this));
				return Task.FromResult(existingResponse);
			}
		}//*/

		/// <summary>
		/// Get a string resource for <see cref="Culture"/>.
		/// </summary>
		public string GetString(string name) {
			return Resources.GetString(Assembly.GetCallingAssembly(), Culture, name);
		}

		/// <summary>
		/// Get a string resource for <see cref="Culture"/> and format it.
		/// </summary>
		public string GetString(string name, params object[] args) {
			return string.Format(Resources.GetString(Assembly.GetCallingAssembly(), Culture, name), args);
		}
		
		internal string GetString(Assembly assembly, string name) {
			return Resources.GetString(assembly, Culture, name);
		}

		internal string GetString(Assembly assembly, string name, params object[] args) {
			return string.Format(Resources.GetString(assembly, Culture, name), args);
		}
	}
}
