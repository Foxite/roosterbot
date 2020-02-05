using System;
using System.IO;

namespace RoosterBot {
	internal class FileLogEndpoint : LogEndpoint {
		private readonly string m_LogPath = Path.Combine(Program.DataPath, "RoosterBot");

		public FileLogEndpoint() {
			// Keep the log from the previous launch as ".old.log"
			if (File.Exists(m_LogPath + ".log")) {
				if (File.Exists(m_LogPath + ".old.log")) {
					File.Delete(m_LogPath + ".old.log");
				}
				File.Move(m_LogPath + ".log", m_LogPath + ".old.log");
				m_LogPath += ".log";
				File.Create(m_LogPath).Dispose(); // File.Create automatically opens a stream to it, but we don't need that.
			} else {
				m_LogPath += ".log";
			}
		}

		public override void Log(LogMessage message) {
			File.AppendAllText(m_LogPath, FormatMessage(message) + Environment.NewLine);
		}
	}
}
