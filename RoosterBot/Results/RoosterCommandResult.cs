using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// The base class for all <see cref="CommandResult"/>s used within RoosterBot.
	/// </summary>
	public abstract class RoosterCommandResult : CommandResult {
		/// <summary>
		/// Indicates if the result was successful.
		/// </summary>
		// TODO ignore, use by resultadapter
		public sealed override bool IsSuccessful => true;
	}
}
