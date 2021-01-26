using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

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
		public async Task<IReadOnlyList<Trip>> GetTravelRecommendation(StationInfo from, StationInfo to, DateTimeOffset datetime, bool arrival, string? context) {
			using var request = new HttpRequestMessage(HttpMethod.Get, $"https://gateway.apiportal.ns.nl/reisinformatie-api/api/v3/trips?lang=en&fromStation={from.Code}&toStation={to.Code}&dateTime={datetime:s}&searchForArrival={arrival}"); //&context={context}");
			request.Headers.Add("Ocp-Apim-Subscription-Key", m_Key);

			using var response = await m_Http.SendAsync(request);

			var result = JsonConvert.DeserializeObject<TripsResponse>(await response.Content.ReadAsStringAsync());

			return result.Trips;
		}
	}

	public record TripsResponse(
		[JsonProperty("source")]
		string Source,

		[JsonProperty("trips")]
		Trip[] Trips,

		[JsonProperty("scrollRequestBackwardContext")]
		string BackwardContext,

		[JsonProperty("scrollRequestForwardContext")]
		string ForwardContext

	);

	public record Trip(
		int plannedDurationInMinutes,
		int actualDurationInMinutes,
		int transfers,
		string status, // TODO enum
		TripLeg[] legs,
		string crowdForecast, // TODO enum
		float punctuality,
		bool optimal,
		string type
	);

	public record TripLeg(
		string name,
		string travelType, // TODO enum
		string direction, // TODO StationInfo
		bool cancelled,
		bool changePossible,
		bool alternativeTransport,
		TripOrigin origin,
		TripDestination destination,
		Transport product,
		string crowdForecast, // TODO enum
		float punctuality,
		bool crossPlatformTransfer,
		bool shorterStock,
		bool reachable,
		int plannedDurationInMinutes
	);

	public record TripOrigin(
		string name,
		string type, // TODO enum
		int plannedTimeZoneOffset,
		DateTimeOffset plannedDateTime,
		int actualTimeZoneOffset,
		DateTimeOffset actualDateTime,
		string plannedTrack, // string. perhaps there's a track "seven and a half" somewhere out there.
		string actualTrack
	);

	public record TripDestination(
		string name,
		string type, // TODO enum
		int plannedTimeZoneOffset,
		DateTimeOffset plannedDateTime,
		int actualTimeZoneOffset,
		DateTimeOffset actualDateTime,
		string plannedTrack,
		string actualTrack,
		string exitSide // TODO enum
	);
	
	public record Transport(
		string displayName
	);
}
