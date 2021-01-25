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
		/// Indicates if the channel is private.
		/// </summary>
		bool IsPrivate { get; }

		/// <summary>
		/// Sends a message into the IChannel. Optionally includes a file attachment.
		/// </summary>
		/// <param name="content">The content of the message.</param>
		/// <returns>The <see cref="IMessage"/> object that was created.</returns>
		Task<IMessage> SendMessageAsync(string content);

		/// <summary>Retrieves a message in a channel.</summary>
		/// <exception cref="SnowflakeNotFoundException">When no message with the given id exists in the channel.</exception>
		Task<IMessage> GetMessageAsync(object id);
	}
}
