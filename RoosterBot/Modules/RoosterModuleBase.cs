using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace RoosterBot {
	public abstract class RoosterModuleBase<T> : ModuleBase<T>, IRoosterModuleBase where T : RoosterCommandContext {
#nullable disable
		// Another case of local nullability overrides, but this one's a bit more hairy.
		// There are two ways for modules to get service instances:
		// - Public settable properties
		// - Constructor parameters
		// For every module I write, I've switched to using constructors. I like it less than properties, but at least it works without generating warnings or having to disable nullability.
		// I would do it here as well but I'd get PTSD. Why?
		// Back before this bot was remotely structured the way it is now, before I even released the program called "RoosterBot", I used constructors for dependency injection, everywhere, and the longest
		//  inheritance chain for modules at that time was 3.
		// Every single module had to extend the constructors of its base class, and with as much as 7 services in one module (including inherited dependencies) you can see where this is going.
		// I don't want to go back to that, and while there's less inheritance surrounding modues now, there's still 5 dependencies here that need to be set. All of them are specific to this
		//  class only, and are not supposed to be used by subclasses, but they would still need to be inherited by subclasses if I added a constructor here.
		// So again the best solution is to just disable the warnings here.
		// I'd love a way to disable all warnings that come with DI somehow, as an exception is thrown during AddModuleAsync if a dependency is not found, but there's no feasible way to do that.
		public ConfigService Config { get; set; }
		public GuildConfigService GuildConfigService { get; set; }
		public ResourceService ResourcesService { get; set; }
		public RoosterCommandService CmdService { get; set; }
		public CommandResponseService CommandResponses { get; set; }
		public new T Context { get; internal set; }
		// TODO (investigate) Can analyzers disable other analyzers? Make an analyzer for ModuleBase<T> that disables nullability warnings on public settable properties.

		protected ModuleLogger Log { get; private set; }
		protected GuildConfig GuildConfig { get; private set; }
#nullable restore

		protected CultureInfo Culture => GuildConfig.Culture;

		private StringBuilder m_Response = new StringBuilder();
		private bool m_Replied = false;

		void IRoosterModuleBase.BeforeExecuteInternal(CommandInfo command) => BeforeExecute(command);
		void IRoosterModuleBase.AfterExecuteInternal(CommandInfo command) => AfterExecute(command);

		protected override void BeforeExecute(CommandInfo command) {
			if (Context == null) {
				Context = base.Context;
			}
			IGuild? guild = Context.Guild ?? Context.UserGuild;
			if (guild == null) {
				GuildConfig = GuildConfigService.GetDefaultConfig();
			} else {
				GuildConfig = GuildConfigService.GetConfigAsync(guild).GetAwaiter().GetResult()!; // Change to await after switching to Qmmands in 3.0
			}
			
			Log = new ModuleLogger(GetType().Name);

			string logMessage = $"Executing `{Context.Message.Content}` for `{Context.User.Username}#{Context.User.Discriminator}` in ";
			if (Context.Guild == null) {
				if (Context.Channel is IDMChannel) {
					logMessage += "DM";
				} else {
					logMessage = $"group {Context.Channel.Name}";
				}
			} else {
				logMessage += $"{Context.Guild.Name} channel {Context.Channel.Name}";
			}
			Log.Debug(logMessage);
		}

		protected override void AfterExecute(CommandInfo command) {
			if (m_Response.Length != 0) {
				string message = m_Response.ToString();
				m_Response.Clear();
				ReplyAsync(message).GetAwaiter().GetResult();
			}
		}

		/// <summary>
		/// Queues a message to be sent after the command has finished executing.
		/// </summary>
		protected virtual void ReplyDeferred(string message) {
			lock (m_Response) {
				m_Response.AppendLine(message);
			}
		}

		protected async virtual Task<IUserMessage?> SendDeferredResponseAsync() {
			if (m_Response.Length != 0) {
				string message = m_Response.ToString();
				m_Response.Clear();
				// Ending with `return await` is not preferred because it creates an unnecessary async state machine, but ReplyAsync never returns null but this function might.
				// The only way to make it work without warnings is to make this an async function and await ReplyAsync. I initially tried returning Task.FromResult(null) but Task<T> cannot be used in place of Task<T?>.
				// This is by design. Consider this:
				// 
				//     List<string?> list = new List<string>();
				//     list.Add(null); // List that doesn't handle null types now contains a null item
				// 
				// The opposite doesn't work either:
				// 
				//     List<string> list = new List<string?>();
				//     foreach (string item in list) { ... } // item may be null despite not being nullable
				return await ReplyAsync(message);
			} else {
				return null;
			}
		}

		protected override async Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed? embed = null, RequestOptions? options = null) {
			if (m_Response.Length != 0) {
				message = m_Response
					.AppendLine(message)
					.ToString();
				m_Response.Clear();
			}

			IUserMessage ret;
			if (Context.Responses == null) {
				// The command was not edited, or the command somehow did not invoke a reply.
				IUserMessage response = await Context.Channel.SendMessageAsync(message, isTTS, embed, options);
				CommandResponses.AddResponse(Context.Message, response);
				ret = response;
			} else {
				// The command was edited.
				ret = await Util.ModifyResponsesIntoSingle(message, Context.Responses, m_Replied);

				CommandResponses.ModifyResponse(Context.Message, new[] { ret });
			}

			m_Replied = true;
			return ret;
		}

		// Discord.NET offers a command result system (IResult), we may be able to use that instead of MinorError and FatalError
		protected virtual Task MinorError(string message) {
			return ReplyAsync(Util.Error + message);
		}

		protected virtual async Task FatalError(string message, Exception? exception = null) {
			string report = $"Fatal error executing `{Context.Message.Content}` for `{Context.User.Mention}` in {Context.Guild?.Name ?? "DM"} channel {Context.Channel.Name}: {message}";

			Log.Error(report, exception);

			if (exception != null) {
				report += $"\nAttached exception: {Util.EscapeString(exception.ToStringDemystified())}\n";
			}
			
			if (Config.BotOwner != null) {
				await Config.BotOwner.SendMessageAsync(report);
			}
			
			string response = Util.Error + GetString("RoosterBot_FatalError");
			await ReplyAsync(response);
		}

		protected string GetString(string name) {
			return ResourcesService.GetString(Assembly.GetCallingAssembly(), Culture, name);
		}

		protected string GetString(string name, params object[] args) {
			return string.Format(ResourcesService.GetString(Assembly.GetCallingAssembly(), Culture, name), args);
		}

		protected class ModuleLogger {
			protected internal string m_Tag;

			private ModuleLogger() { m_Tag = "ErrorModule"; }

			internal ModuleLogger(string tag) {
				m_Tag = tag;
			}

			public void Verbose(string message, Exception? e = null) {
				Logger.Verbose(m_Tag, message, e);
			}

			public void Debug(string message, Exception? e = null) {
				Logger.Debug(m_Tag, message, e);
			}

			public void Info(string message, Exception? e = null) {
				Logger.Info(m_Tag, message, e);
			}

			public void Warning(string message, Exception? e = null) {
				Logger.Warning(m_Tag, message, e);
			}

			public void Error(string message, Exception? e = null) {
				Logger.Error(m_Tag, message, e);
			}

			public void Critical(string message, Exception? e = null) {
				Logger.Critical(m_Tag, message, e);
			}
		}
	}

	public abstract class RoosterModuleBase : RoosterModuleBase<RoosterCommandContext> { }

	public class RoosterCommandContext : ICommandContext {
		public IDiscordClient Client { get; }
		public IUserMessage Message { get; }
		public IUser User { get; }
		public IMessageChannel Channel { get; }
		public IGuild? Guild { get; }
		public bool IsPrivate { get; }
		// If this is null, we should make a new message.
		public IReadOnlyCollection<IUserMessage> Responses { get; }

		/// <summary>
		/// If IsPrivate is true, then this guild will be suitable to use for config information. It is the first mutual guild between the user and the bot user.
		/// </summary>
		public IGuild UserGuild { get; }
		
		// TODO review all instantiations, it now throws an exception if there's no mutual guilds
		public RoosterCommandContext(IDiscordClient client, IUserMessage message, IReadOnlyCollection<IUserMessage> originalResponses) {
			Client = client;
			Message = message;
			User = message.Author;
			Channel = message.Channel;
			IsPrivate = Channel is IPrivateChannel;
			Guild = (Channel as IGuildChannel)?.Guild;
			Responses = originalResponses;

			UserGuild = User.GetMutualGuilds().First();
		}

		public override string ToString() {
			if (Guild != null) {
				return $"{User.Username}#{User.Discriminator} in `{Guild.Name}` channel `{Channel.Name}`: {Message.Content}";
			} else {
				return $"{User.Username}#{User.Discriminator} in private channel `{Channel.Name}`: {Message.Content}";
			}
		}
	}

	internal interface IRoosterModuleBase {
		void BeforeExecuteInternal(CommandInfo command);
		void AfterExecuteInternal(CommandInfo command);
	}
}
