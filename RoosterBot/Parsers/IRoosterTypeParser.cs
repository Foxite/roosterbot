using System;

namespace RoosterBot.Readers {
	public interface IRoosterTypeParser {
		Type Type { get; }

		/// <summary>
		/// The display name of the Type that this TypeReader parses. This may be a resolvable resource.
		/// </summary>
		string TypeDisplayName { get; }
	}
}
