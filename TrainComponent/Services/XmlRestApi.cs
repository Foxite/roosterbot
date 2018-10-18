using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TrainComponent.Services {
	public class XmlRestApi : IRestApiAsync {
		private string m_CallDomain;
		private string m_Username;
		private string m_Password;

		public XmlRestApi(string callDomain, string username, string password) {
			m_CallDomain = callDomain;
			m_Username = username;
			m_Password = password;
		}
		
		public string GetCallDomain() {
			return m_CallDomain;
		}

		public string GetDirectOutput(string call, IDictionary<string, string> parameters) {
			return GetDirectOutputAsync(call, parameters).GetAwaiter().GetResult();
		}

		public async Task<string> GetDirectOutputAsync(string call, IDictionary<string, string> parameters) {
			throw new NotImplementedException();
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
