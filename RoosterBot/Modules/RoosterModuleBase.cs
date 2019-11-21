using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot {
	public abstract class RoosterModuleBase<T> : ModuleBase<T>, IRoosterModuleBase where T : RoosterCommandContext {
		// Initializing a property with `null!` will prevent a warning about non-nullable properties being unassigned.
		// They will be assigned through reflection, and this is the best way to tell the compiler that it's fine.
		// https://stackoverflow.com/a/57343485/3141917
		public ConfigService Config { get; set; } = null!;
		public GuildConfigService GuildConfigService { get; set; } = null!;
		public UserConfigService UserConfigService { get; set; } = null!;
		public ResourceService ResourcesService { get; set; } = null!;
		public RoosterCommandService CmdService { get; set; } = null!;
		public new T Context { get; internal set; } = null!;

		protected ModuleLogger Log { get; private set; } = null!;

		protected UserConfig UserConfig => Context.UserConfig;
		protected GuildConfig GuildConfig => Context.GuildConfig;
		protected CultureInfo Culture => UserConfig.Culture ?? GuildConfig.Culture;

		private readonly StringBuilder m_Response = new StringBuilder();

		void IRoosterModuleBase.BeforeExecuteInternal(CommandInfo command) => BeforeExecute(command);
		void IRoosterModuleBase.AfterExecuteInternal(CommandInfo command) => AfterExecute(command);

		protected override void BeforeExecute(CommandInfo command) {
			if (Context == null) {
				Context = base.Context;
			}
			
			Log = new ModuleLogger(GetType().Name);

			Log.Debug(Context.ToString());
		}

		protected override void AfterExecute(CommandInfo command) {
			SendDeferredResponseAsync().GetAwaiter().GetResult();
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

		protected override Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed? embed = null, RequestOptions? options = null) {
			if (m_Response.Length != 0) {
				message = m_Response
					.AppendLine(message)
					.ToString();
				m_Response.Clear();
			}

			return CommandResponseUtil.RespondAsync(Context, message, isTTS, embed, options);
		}

		// Discord.NET offers a command result system (IResult), we may be able to use that instead of MinorError and FatalError
		protected virtual Task MinorError(string message) {
			return ReplyAsync(Util.Error + message);
		}

		protected async virtual Task FatalError(string message, Exception? exception = null) {
			string report = $"Fatal error executing {Context}\nAttached error message: {message}";

			Log.Error(report, exception);

			if (exception != null) {
				report += $"\nAttached exception: {StringUtil.EscapeString(exception.ToStringDemystified())}\n";
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
			protected internal string m_Tag = "ErrorModule";

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

		public UserConfig UserConfig { get; }
		public GuildConfig GuildConfig { get; }
		public CultureInfo Culture => UserConfig.Culture ?? GuildConfig.Culture;

		public RoosterCommandContext(IDiscordClient client, IUserMessage message, UserConfig userConfig, GuildConfig guildConfig) {
			Client = client;
			Message = message;
			User = message.Author;
			Channel = message.Channel;
			IsPrivate = Channel is IPrivateChannel;
			Guild = (Channel as IGuildChannel)?.Guild;

			UserConfig = userConfig;
			GuildConfig = guildConfig;
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
