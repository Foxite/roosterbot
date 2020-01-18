using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// The base class for all <see cref="CommandResult"/>s used within RoosterBot.
	/// </summary>
	public abstract class RoosterCommandResult : CommandResult {
		/// <summary>
		/// The optional path to the file attached to the result.
		/// </summary>
		public string? UploadFilePath { get; set; }

		/// <summary>
		/// Indicates if the result was successful.
		/// </summary>
		public sealed override bool IsSuccessful => true;

		/// <summary>
		/// <b>DO NOT USE THIS!</b> Specify a <see cref="RoosterCommandContext"/>. This method does not work.
		/// </summary>
		public sealed override string ToString() => throw new System.NotSupportedException("A " + nameof(RoosterCommandContext) + " is required for " + nameof(RoosterCommandContext.ToString));

		/// <summary>
		/// Convert the result to a string that can be displayed to the user.
		/// </summary>
		public abstract string ToString(RoosterCommandContext rcc);
	}
}
