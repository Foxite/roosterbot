using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	public abstract class RoosterCommandResult : CommandResult {
		public sealed override bool IsSuccessful => true;

		public abstract Task PresentAsync(RoosterCommandContext context);
	}
}
