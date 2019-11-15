using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoosterBot {
	public interface IRoosterResult : IResult {
		/// <summary>
		/// Objects used when formatting ErrorReason.
		/// </summary>
		IReadOnlyList<object> ErrorReasonObjects { get; }

		/// <summary>
		/// Component used when resolving ErrorReason.
		/// </summary>
		ComponentBase? ErrorReasonComponent { get; }
	}
}
