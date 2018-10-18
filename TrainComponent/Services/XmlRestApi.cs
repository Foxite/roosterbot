using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace PublicTransitComponent.Services {
	public class XmlRestApi : IRestApiAsync {
		private string m_CallDomain;
		private string m_Username;
		private string m_Password;

		private WebClient m_HTTP;

		public XmlRestApi(WebClient http, string callDomain, string username, string password) {
			m_CallDomain = callDomain;
			m_Username = username;
			m_Password = password;

			m_HTTP = http;
			CredentialCache creds = new CredentialCache {
				{ new Uri(new Uri(callDomain).GetLeftPart(UriPartial.Authority)), "Basic", new NetworkCredential(username, password) }
			};
			m_HTTP.Credentials = creds;
		}
		
		public string GetCallDomain() {
			return m_CallDomain;
		}

		public string GetDirectOutput(string call, IDictionary<string, string> parameters) {
			return GetDirectOutputAsync(call, parameters).GetAwaiter().GetResult();
		}

		public async Task<string> GetDirectOutputAsync(string call, IDictionary<string, string> parameters) {
			string uri = m_CallDomain + call + "?";
			bool notFirstParam = false;
			foreach (KeyValuePair<string, string> kvp in parameters) {
				if (notFirstParam) {
					uri += "&";
				}
				uri += $"{kvp.Key}={kvp.Value}";
				notFirstParam = true;
			}
			return await m_HTTP.DownloadStringTaskAsync(uri);
		}

		public XmlDocument GetXmlOutput(string call, IDictionary<string, string> parameters) {
			return GetXmlOutputAsync(call, parameters).GetAwaiter().GetResult();
		}

		public async Task<XmlDocument> GetXmlOutputAsync(string call, IDictionary<string, string> parameters) {
			string result = await GetDirectOutputAsync(call, parameters);
			XmlDocument xmlResult = new XmlDocument();
			xmlResult.LoadXml(result);
			return xmlResult;
		}
	}
}
