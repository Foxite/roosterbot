using System;
using System.IO;

namespace RoosterBot {
	/// <summary>
	/// A <see cref="RoosterCommandResult"/> with a stream containing data to be sent to the user.
	/// </summary>
	public class MediaResult : RoosterCommandResult {
		private readonly Func<Stream> m_GetStream;

		/// <summary>
		/// The additional text sent with the media.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// The filename of the attachment, if it will be sent as a file.
		/// </summary>
		public string Filename { get; }

		///
		public MediaResult(string message, string filename, Func<Stream> stream) {
			Message = message;
			Filename = filename;
			m_GetStream = stream;
		}

		/// <summary>
		/// Obtain a stream with the data to be sent.
		/// </summary>
		public Stream GetStream() => m_GetStream();
	}
}
