using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot {
	public abstract class RoosterModuleBase<T> : ModuleBase<T> where T : RoosterCommandContext {
		// Initializing a property with `null!` will prevent a warning about non-nullable properties being unassigned.
		// They will be assigned through reflection, and this is the best way to tell the compiler that it's fine.
		// https://stackoverflow.com/a/57343485/3141917
		public ConfigService Config { get; set; } = null!;
		public GuildConfigService GuildConfigService { get; set; } = null!;
		public UserConfigService UserConfigService { get; set; } = null!;
		public ResourceService ResourcesService { get; set; } = null!;
		public RoosterCommandService CmdService { get; set; } = null!;

		protected ModuleLogger Log { get; private set; } = null!;

		protected UserConfig UserConfig => Context.UserConfig;
		protected GuildConfig GuildConfig => Context.GuildConfig;
		protected CultureInfo Culture => UserConfig.Culture ?? GuildConfig.Culture;

		private readonly StringBuilder m_Response = new StringBuilder();

		protected override ValueTask BeforeExecutedAsync() {
			Log = new ModuleLogger(GetType().Name);

			Log.Debug("Executing: " + Context.ToString());
			// This is like Task.CompletedTask but with ValueTask
			// https://github.com/dotnet/corefx/issues/33609#issuecomment-440123253
			return default;
		}

		/// <summary>
		/// Adds a line to the command's response. If you use this, you should have <code>return <see cref="Ok(Emote)"/></code> at the end of your command.
		/// </summary>
		protected virtual void ReplyDeferred(string message) {
			lock (m_Response) {
				m_Response.AppendLine(message);
			}
		}

		/// <summary>
		/// Gets a TextResult based on the strings passed to <see cref="ReplyDeferred(string)"/>
		/// </summary>
		/// <remarks>
		/// If <see cref="ReplyDeferred(string)"/>, then the TextResult will inform the user that the command returned no response.
		/// </remarks>
		protected TextResult Ok(Emote? emote) {
			if (m_Response.Length > 0) {
				return new TextResult(emote, m_Response.ToString());
			} else {
				// TODO (localize) This message
				return TextResult.Info("The command returned no response.");
			}
		}

		protected virtual TextResult MinorError(string message) => TextResult.Error(message);

		protected async Task<TextResult> Error(string message, Exception? exception = null) {
			string report = $"Fatal error executing {Context}\nAttached error message: {message}";

			Log.Error(report, exception);

			if (exception != null) {
				report += $"\nAttached exception: {StringUtil.EscapeString(exception.ToStringDemystified())}\n";
			}
			
			if (Config.BotOwner != null) {
				await Config.BotOwner.SendMessageAsync(report);
			}

			return TextResult.Error(GetString("RoosterBot_FatalError"));
		}

		/// <summary>
		/// In non-async methods, this function serves as a shortcut for this:
		/// <code>
		/// return Task.FromResult((CommandResult) new RoosterCommandResult(...))
		/// </code>
		/// Instead, you can do this:
		/// <code>
		/// return Result(new RoosterCommandResult(...));
		/// </code>
		/// In async mmethods you should not use this function, simply return the result directly.
		/// </summary>
		protected Task<CommandResult> Result(RoosterCommandResult result) => Task.FromResult((CommandResult) result);

		protected string GetString(string name) {
			return ResourcesService.GetString(Assembly.GetCallingAssembly(), Culture, name);
		}

		protected string GetString(string name, params object[] args) {
			return string.Format(ResourcesService.GetString(Assembly.GetCallingAssembly(), Culture, name), args);
		}

		protected sealed class ModuleLogger {
			private readonly string m_Tag;

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
}
