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

		protected async override ValueTask AfterExecutedAsync() {
			// Can't return this task because ValueTask.
			// ValueTask has a constructor that takes Task, should we use that? Don't do anything unless you know how ValueTask works
			await SendDeferredResponseAsync();
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

		// TODO (refactor) This function shouldn't be used anymore, we should use ReplyDeferred. Multiple responses is bad UI.
		protected virtual Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed? embed = null, RequestOptions? options = null) {
			if (m_Response.Length != 0) {
				message = m_Response
					.AppendLine(message)
					.ToString();
				m_Response.Clear();
			}

			return CommandResponseUtil.RespondAsync(Context, message, isTTS, embed, options);
		}

		protected virtual void MinorError(string message) {
			ReplyDeferred(Util.Error + message);
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
			ReplyDeferred(response);
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
}
