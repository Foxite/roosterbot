using System;
using System.Collections.Generic;
using Qmmands;

namespace RoosterBot {
	public abstract class RoosterCommandResult : CommandResult, IRoosterResult {
		public override bool IsSuccessful => true;

		public virtual IReadOnlyList<object> ErrorReasonObjects => Array.Empty<object>();
		public virtual ComponentBase? ErrorReasonComponent => null;

		/// <summary>
		/// Converts this result to a string that can be sent to Discord.
		/// </summary>
		public abstract string Present();
	}
}
