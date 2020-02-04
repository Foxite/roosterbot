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

		/// <inheritdoc/>
		protected override ValueTask BeforeExecutedAsync() {
			Logger.Debug(GetType().Name, "Executing: " + Context.ToString());
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
	}

	/// <summary>
	/// Shorthand for <code>RoosterModule&lt;RoosterCommandContext&gt;</code>. This module type accepts all contexts.
	/// </summary>
	public abstract class RoosterModule : RoosterModule<RoosterCommandContext> { }
}
