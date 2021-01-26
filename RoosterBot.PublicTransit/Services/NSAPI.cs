using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RoosterBot.PublicTransit {
	public class NSAPI {
		private readonly HttpClient m_Http;
		private readonly string m_Key;
		
		public NSAPI(string key, HttpClient http) {
			m_Key = key;
			m_Http = http;
		}

		// There is no way to specify how many trips you want. It just gives you 2 at a time.
		// The minified json response during testing was 12kb.
		public async Task<TripsResponse> GetTravelRecommendation(StationInfo from, StationInfo to, DateTimeOffset datetime, bool arrival, string? context) {
			using var request = new HttpRequestMessage(HttpMethod.Get, $"https://gateway.apiportal.ns.nl/reisinformatie-api/api/v3/trips?lang=en&fromStation={from.Code}&toStation={to.Code}&dateTime={datetime:s}&searchForArrival={arrival}&context={context}");
			request.Headers.Add("Ocp-Apim-Subscription-Key", m_Key);

			using var response = await m_Http.SendAsync(request);

			var result = JsonConvert.DeserializeObject<TripsResponse>(await response.Content.ReadAsStringAsync());

			return result;
		}
	}
}
