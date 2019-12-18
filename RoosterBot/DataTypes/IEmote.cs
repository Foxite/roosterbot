namespace RoosterBot {
	/// <summary>
	/// Represents a graphic that can be added to an <see cref="IMessage"/> object.
	/// </summary>
	public interface IEmote {
		/// <summary>
		/// Gets the string that, when added to an <see cref="IMessage"/>, will result in the IEmote being properly displayed in the user's client.
		/// </summary>
		string ToString();
	}
}
