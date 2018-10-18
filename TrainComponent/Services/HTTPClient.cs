using System.Net;
using RoosterBot;

namespace PublicTransitComponent.Services {
	public class HTTPClient {
		public WebClient Client { get; private set; }

		public HTTPClient() {
			Client = new WebClient();
			Program.Instance.ProgramStopping += (o, e) => {
				Client.Dispose();
			};
		}
	}
}
