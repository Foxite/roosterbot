using System;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// The base class for all <see cref="CommandResult"/>s used within RoosterBot.
	/// </summary>
	public abstract class RoosterCommandResult : CommandResult {
		/// <summary>
		/// Indicates if the result was successful.
		/// </summary>
		public sealed override bool IsSuccessful => true;

		/// <summary>
		/// <b>DO NOT USE THIS!</b> Specify a <see cref="RoosterCommandContext"/>. This method does not work.
		/// </summary>
		[Obsolete("Use ToString(RoosterCommandContext)")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
		public sealed override string ToString() => throw new NotSupportedException("A " + nameof(RoosterCommandContext) + " is required for " + nameof(RoosterCommandContext.ToString));
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

		/// <summary>
		/// Convert the result to a string that can be displayed to the user.
		/// </summary>
		public abstract string ToString(RoosterCommandContext rcc);
	}
}
