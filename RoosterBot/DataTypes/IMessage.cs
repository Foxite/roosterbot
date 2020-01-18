﻿using System.Threading.Tasks;

namespace RoosterBot {
	/// <summary>
	/// Represents a message that was either sent by a user or by RoosterBot.
	/// </summary>
	public interface IMessage : ISnowflake {
		/// <summary>
		/// The <see cref="IChannel"/> that this message was sent in.
		/// </summary>
		IChannel Channel { get; }
		
		/// <summary>
		/// The <see cref="IUser"/> who sent this message.
		/// </summary>
		IUser User { get; }

		/// <summary>
		/// The content of the message.
		/// </summary>
		string Content { get; }

		/// <summary>
		/// Delete the message.
		/// </summary>
		Task DeleteAsync();

		/// <summary>
		/// Modify the message.
		/// </summary>
		Task ModifyAsync(string newContent, string? filePath = null);
	}
}