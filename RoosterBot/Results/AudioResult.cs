using System.Threading.Tasks;

namespace RoosterBot {
	public class AudioResult : RoosterCommandResult {
		public string Caption { get; }
		public string FilePath { get; }

		public AudioResult(string caption, string filePath) {
			Caption = caption;
			FilePath = filePath;
		}

		public override Task PresentAsync(RoosterCommandContext context) => context.RespondAsync(Caption, FilePath);
	}
}
