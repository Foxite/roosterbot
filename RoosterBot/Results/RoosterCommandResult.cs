using Qmmands;

namespace RoosterBot {
	public abstract class RoosterCommandResult : CommandResult {
		public string? UploadFilePath { get; set; }

		public sealed override bool IsSuccessful => true;

		public abstract string ToString(RoosterCommandContext rcc);
	}
}
