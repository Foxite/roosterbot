using System;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// The base class for all command modules used within RoosterBot.
	/// </summary>
	/// <typeparam name="T">The type of <see cref="RoosterCommandContext"/> required by this module.</typeparam>
	public abstract class RoosterModule<T> : ModuleBase<T> where T : RoosterCommandContext {
		// Initializing a property with `null!` will prevent a warning about non-nullable properties being unassigned.
		// They will be assigned through reflection, and this is the best way to tell the compiler that it's fine.
		// https://stackoverflow.com/a/57343485/3141917
		///
		public ResourceService Resources { get; set; } = null!;

		/// <summary>
		/// The <see cref="ModuleLogger"/> instance for this module.
		/// </summary>
		protected ModuleLogger Log { get; private set; } = null!;

		/// <summary>
		/// Shorthand for <code>Context.UserConfig</code>
		/// </summary>
		protected UserConfig UserConfig => Context.UserConfig;

		/// <summary>
		/// Shorthand for <code>Context.ChannelConfig</code>
		/// </summary>
		protected ChannelConfig ChannelConfig => Context.ChannelConfig;
		
		/// <summary>
		/// Shorthand for <code>Context.Culture</code>
		/// </summary>
		protected CultureInfo Culture => UserConfig.Culture ?? ChannelConfig.Culture;

		///
		protected override ValueTask BeforeExecutedAsync() {
			Log = new ModuleLogger(GetType().Name);

			Log.Debug("Executing: " + Context.ToString());
			// This is like Task.CompletedTask but with ValueTask
			// https://github.com/dotnet/corefx/issues/33609#issuecomment-440123253
			return default;
		}

		/// <summary>
		/// Get a string resource for the current culture.
		/// </summary>
		protected string GetString(string name) {
			return Resources.GetString(Assembly.GetCallingAssembly(), Culture, name);
		}

		/// <summary>
		/// Get a string resource for the current culture and format it.
		/// </summary>
		protected string GetString(string name, params object[] args) {
			return string.Format(Resources.GetString(Assembly.GetCallingAssembly(), Culture, name), args);
		}

		/// <summary>
		/// A helper class that stops you from having to specify a log tag with your <see cref="Logger"/> calls.
		/// </summary>
		protected sealed class ModuleLogger {
			private readonly string m_Tag;

			internal ModuleLogger(string tag) {
				m_Tag = tag;
			}

			/// <summary>Log a message at verbose level.</summary>
			public void Verbose (string message, Exception? e = null) => Logger.Verbose (m_Tag, message, e);
			/// <summary>Log a message at debug level.</summary>
			public void Debug   (string message, Exception? e = null) => Logger.Debug   (m_Tag, message, e);
			/// <summary>Log a message at informational level.</summary>
			public void Info    (string message, Exception? e = null) => Logger.Info    (m_Tag, message, e);
			/// <summary>Log a message at verbose level.</summary>
			public void Warning (string message, Exception? e = null) => Logger.Warning (m_Tag, message, e);
			/// <summary>Log a message at error level.</summary>
			public void Error   (string message, Exception? e = null) => Logger.Error   (m_Tag, message, e);
			/// <summary>Log a message at critical level.</summary>
			public void Critical(string message, Exception? e = null) => Logger.Critical(m_Tag, message, e);
		}
	}

	/// <summary>
	/// Shorthand for <code>RoosterModule&lt;RoosterCommandContext&gt;</code>. This module type accepts all contexts.
	/// </summary>
	public abstract class RoosterModule : RoosterModule<RoosterCommandContext> { }
}
