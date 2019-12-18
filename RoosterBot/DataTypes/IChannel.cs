using System.Collections.Generic;
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
		/// Obtains an amount of messages from the Platform, with an optional amount of messages to be downloaded at once.
		/// </summary>
		/// <param name="batchCount">The amount of messages that will be downloaded at once. This can accelerate enumeration if the amount of messages needed is known.</param>
		IAsyncEnumerable<IMessage> GetMessagesAsync(int batchCount = 1);

		/// <summary>
		/// Sends a message into the IChannel. Optionally includes a file attachment.
		/// </summary>
		/// <param name="filePath">Path to the attached file, or null if no attachment.</param>
		/// <returns>The <see cref="IMessage"/> object that was created.</returns>
		Task<IMessage> SendMessageAsync(string content, string? filePath = null);
		Task<IMessage> GetMessageAsync(object id);
	}
}
