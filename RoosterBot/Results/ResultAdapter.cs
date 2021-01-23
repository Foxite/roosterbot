using System.Threading.Tasks;

namespace RoosterBot {
	/// <summary>
	/// Handles a <see cref="RoosterCommandResult"/> using platform-specific features.
	/// </summary>
	public abstract class ResultAdapter {
		/// <summary>
		/// Checks if this <see cref="ResultAdapter"/> can work with the given combination of <see cref="RoosterCommandContext"/> and <see cref="RoosterCommandResult"/>.
		/// </summary>
		public abstract bool CanHandleResult(RoosterCommandContext context, RoosterCommandResult result);

		/// <summary>
		/// Process a <paramref name="result"/> on the given <paramref name="context"/>.
		/// 
		/// This will not be called if <see cref="CanHandleResult(RoosterCommandContext, RoosterCommandResult)"/> returned false for the given parameters.
		/// </summary>
		public abstract Task<IMessage> HandleResult(RoosterCommandContext context, RoosterCommandResult result);
	}
}
