namespace RoosterBot {
	public interface IInstalledService {
		/// <summary>
		/// Returns if the service has been properly installed and is ready for use.
		/// </summary>
		bool Installed();
	}
}
