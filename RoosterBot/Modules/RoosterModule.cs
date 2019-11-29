using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	public abstract class RoosterModule<T> : ModuleBase<T> where T : RoosterCommandContext {
		// Initializing a property with `null!` will prevent a warning about non-nullable properties being unassigned.
		// They will be assigned through reflection, and this is the best way to tell the compiler that it's fine.
		// https://stackoverflow.com/a/57343485/3141917
		public ResourceService ResourcesService { get; set; } = null!;

		protected ModuleLogger Log { get; private set; } = null!;

		protected UserConfig  UserConfig  => Context.UserConfig;
		protected GuildConfig GuildConfig => Context.GuildConfig;
		protected CultureInfo Culture     => UserConfig.Culture ?? GuildConfig.Culture;

		protected override ValueTask BeforeExecutedAsync() {
			Log = new ModuleLogger(GetType().Name);

			Log.Debug("Executing: " + Context.ToString());
			// This is like Task.CompletedTask but with ValueTask
			// https://github.com/dotnet/corefx/issues/33609#issuecomment-440123253
			return default;
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
		/// In async methods you should not use this function, simply return the result directly.
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

			public void Verbose (string message, Exception? e = null) => Logger.Verbose (m_Tag, message, e);
			public void Debug   (string message, Exception? e = null) => Logger.Debug   (m_Tag, message, e);
			public void Info    (string message, Exception? e = null) => Logger.Info    (m_Tag, message, e);
			public void Warning (string message, Exception? e = null) => Logger.Warning (m_Tag, message, e);
			public void Error   (string message, Exception? e = null) => Logger.Error   (m_Tag, message, e);
			public void Critical(string message, Exception? e = null) => Logger.Critical(m_Tag, message, e);
		}
	}

	public abstract class RoosterModule : RoosterModule<RoosterCommandContext> { }
}
