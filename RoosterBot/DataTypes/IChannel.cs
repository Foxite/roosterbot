using System.Threading.Tasks;

namespace RoosterBot {
	/// <summary>
	/// Represents a channel that RoosterBot can receive commands from, and send replies into.
	/// </summary>
	public interface IChannel : ISnowflake {
		/// <summary>
		/// Gets the display name of this channel.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Sends a message into the IChannel. Optionally includes a file attachment.
		/// </summary>
		/// <param name="filePath">Path to the attached file, or null if no attachment.</param>
		/// <returns>The <see cref="IMessage"/> object that was created.</returns>
		Task<IMessage> SendMessageAsync(string content, string? filePath = null);
		Task<IMessage> GetMessageAsync(object id);
	}
}
