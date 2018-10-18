using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace PublicTransitComponent.Services {
	public interface IRestApi {
		string GetCallDomain();

		string GetDirectOutput(string call, IDictionary<string, string> parameters);
		XmlDocument GetXmlOutput(string call, IDictionary<string, string> parameters);
	}

	public interface IRestApiAsync : IRestApi {
		Task<string> GetDirectOutputAsync(string callUrl, IDictionary<string, string> parameters);
		Task<XmlDocument> GetXmlOutputAsync(string callUrl, IDictionary<string, string> parameters);
	}
}
