using Newtonsoft.Json;

namespace RoosterBot.Schedule {
	/// <summary>
	/// This class should only ever be used when de/serializing IdentifierInfo.
	/// This class works around the fact that you can't serialize an object, on its own, and include type information. You can only do that if you're serializing the object as part of another.
	/// </summary>
	[JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
	public class IdentifierInfoWrapper {
		public IdentifierInfo WrappedIdentifier { get; }

		public IdentifierInfoWrapper(IdentifierInfo wrappedIdentifier) {
			WrappedIdentifier = wrappedIdentifier;
		}
		
		public static implicit operator IdentifierInfo(IdentifierInfoWrapper wrapper) => wrapper.WrappedIdentifier;
		public static explicit operator IdentifierInfoWrapper(IdentifierInfo info) => new IdentifierInfoWrapper(info);
	}
}
