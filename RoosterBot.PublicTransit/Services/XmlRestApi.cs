﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Xml;

namespace RoosterBot.PublicTransit {
	public class XmlRestApi : IRestApiAsync, IDisposable {
		private readonly string m_CallDomain;
		private readonly WebClient m_Web;

		public XmlRestApi(string callDomain, string username, string password) {
			m_CallDomain = callDomain;

			m_Web = new WebClient();
			var creds = new CredentialCache {
				{ new Uri(new Uri(callDomain).GetLeftPart(UriPartial.Authority)), "Basic", new NetworkCredential(username, password) }
			};
			m_Web.Credentials = creds;
		}
		
		public string GetCallDomain() {
			return m_CallDomain;
		}

		public string GetDirectOutput(string call, IDictionary<string, string> parameters) {
			return m_Web.DownloadString(GetCallUri(call, parameters));
		}

		public async Task<string> GetDirectOutputAsync(string call, IDictionary<string, string> parameters) {
			return await m_Web.DownloadStringTaskAsync(GetCallUri(call, parameters));
		}

		public XmlDocument GetXmlOutput(string call, IDictionary<string, string> parameters) {
			string result = GetDirectOutput(call, parameters);
			var xmlResult = new XmlDocument();
			xmlResult.LoadXml(result);
			return xmlResult;
		}

		public async Task<XmlDocument> GetXmlOutputAsync(string call, IDictionary<string, string> parameters) {
			string result = await GetDirectOutputAsync(call, parameters);
			var xmlResult = new XmlDocument();
			xmlResult.LoadXml(result);
			return xmlResult;
		}
		
		private string GetCallUri(string call, IDictionary<string, string> parameters) {
			string uri = m_CallDomain + call + "?";
			bool notFirstParam = false;
			foreach (KeyValuePair<string, string> kvp in parameters) {
				if (notFirstParam) {
					uri += "&";
				}
				uri += $"{kvp.Key}={kvp.Value}";
				notFirstParam = true;
			}

			return uri;
		}

		#region IDisposable Support
		private bool m_DisposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing) {
			if (!m_DisposedValue) {
				if (disposing) {
					m_Web.Dispose();
				}

				m_DisposedValue = true;
			}
		}
		
		public void Dispose() {
			Dispose(true);
		}
		#endregion
	}
}
