using System.Threading.Tasks;

namespace RoosterBot {
	public class FileResult : RoosterCommandResult {
		public string Caption { get; }
		public string FilePath { get; }

		public FileResult(string caption, string filePath) {
			Caption = caption;
			FilePath = filePath;
		}

		public override Task PresentAsync(RoosterCommandContext context) => context.RespondAsync(Caption, FilePath);
	}
}
