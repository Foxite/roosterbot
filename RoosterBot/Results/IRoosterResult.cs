using System.Collections.Generic;
using Qmmands;

namespace RoosterBot {
	public interface IRoosterResult : IResult {
		/// <summary>
		/// Objects used when formatting ErrorReason.
		/// </summary>
		IReadOnlyList<object> ErrorReasonObjects { get; }

		/// <summary>
		/// Component used when resolving ErrorReason.
		/// </summary>
		Component? ErrorReasonComponent { get; }
	}
}
