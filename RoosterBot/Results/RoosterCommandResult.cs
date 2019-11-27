using Qmmands;

namespace RoosterBot {
	public abstract class RoosterCommandResult : CommandResult {
		public sealed override bool IsSuccessful => true;

		/// <summary>
		/// Converts this result to a string that can be sent to Discord.
		/// </summary>
		public abstract string Present();
	}
}
