using Qmmands;

namespace RoosterBot {
	public abstract class RoosterCommandResult : CommandResult {
		public string? UploadFilePath { get; set; }

		public sealed override bool IsSuccessful => true;

		/// <summary>
		/// <b>DO NOT USE THIS!</b> Specify a <see cref="RoosterCommandContext"/>. This method does not work.
		/// </summary>
		public sealed override string ToString() => throw new System.NotSupportedException("A " + nameof(RoosterCommandContext) + " is required for " + nameof(RoosterCommandContext.ToString));
		public abstract string ToString(RoosterCommandContext rcc);
	}
}
