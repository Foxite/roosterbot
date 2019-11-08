using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace RoosterBot.PublicTransit {
	public class NSAPI : IDisposable {
		// Documentation: https://www.ns.nl/en/travel-information/ns-api/
		private XmlRestApi m_RestApi;
		private Regex m_DelayRegex;

		public NSAPI(string username, string password) {
			m_RestApi = new XmlRestApi("http://webservices.ns.nl/ns-api-", username, password);
			m_DelayRegex = new Regex("[0-9]+");
		}

		public async Task<Journey[]> GetTravelRecommendation(int amount, StationInfo from, StationInfo to) {
			XmlDocument response = await m_RestApi.GetXmlOutputAsync("treinplanner", new Dictionary<string, string>() {
				{ "fromStation", from.Code },
				{ "toStation", to.Code },
				{ "previousAdvices", "1" }
			});

			XmlNodeList xmlOptions = response["ReisMogelijkheden"].ChildNodes;
			Journey[] journeys = new Journey[amount];

			for (int i = 0; i < amount; i++) {
				XmlNode xmlJourney = xmlOptions[i];

				int parseDelay(string? input) {
					if (input == null) {
						return 0;
					}
					Match delayMatch = m_DelayRegex.Match(input);
					if (delayMatch.Success) {
						var result = delayMatch.Value;
						return int.Parse(result);
					} else {
						throw new XmlException("Departure delay is not valid: " + input);
					}
				}

				journeys[i] = new Journey() {
					Transfers = int.Parse(xmlJourney["AantalOverstappen"].InnerText),
					PlannedDuration = TimeSpan.ParseExact(xmlJourney["GeplandeReisTijd"].InnerText, @"h\:mm", CultureInfo.InvariantCulture),
					ActualDuration = TimeSpan.ParseExact(xmlJourney["ActueleReisTijd"].InnerText, @"h\:mm", CultureInfo.InvariantCulture),
					PlannedDepartureTime = DateTime.Parse(xmlJourney["GeplandeVertrekTijd"].InnerText),
					ActualDepartureTime = DateTime.Parse(xmlJourney["ActueleVertrekTijd"].InnerText),
					PlannedArrivalTime = DateTime.Parse(xmlJourney["GeplandeAankomstTijd"].InnerText),
					ActualArrivalTime = DateTime.Parse(xmlJourney["ActueleAankomstTijd"].InnerText),
					DepartureDelayMinutes = parseDelay(xmlJourney["VertrekVertraging"]?.InnerText),
					ArrivalDelayMinutes = parseDelay(xmlJourney["AankomstVertraging"]?.InnerText),
					Status = JourneyStatusFunctions.JStatusFromString(xmlJourney["Status"].InnerText)
				};

				XmlNodeList xmlComponents = xmlJourney.SelectNodes("ReisDeel");
				journeys[i].Components = new List<JourneyComponent>();
				// For no discernable reason, XmlNodeList implements IEnumerable - not IEnumerable<T>. The documentation is very good at dodging the question of what it enumerates,
				//  but I eventually determined that it's XmlNode. I still get the warning about it, which I have to manually disable like this.
#nullable disable
				foreach (XmlNode xmlComponent in xmlComponents) {
					XmlNodeList xmlStops = xmlComponent.SelectNodes("ReisStop");
#nullable restore

					JourneyStop parseJourneyStop(XmlNode xmlStop) {
						XmlNode xmlStopPlatform = xmlStop["Spoor"];
						return new JourneyStop() {
							Location = xmlStop["Naam"].InnerText,
							Platform = xmlStopPlatform.InnerText,
							PlatformModified = bool.Parse((xmlStopPlatform.Attributes["wijziging"]?.InnerText) ?? "false"),
							Time = DateTime.Parse(xmlStop["Tijd"].InnerText),
							DelayMinutes = parseDelay(xmlStop["VertrekVertraging"]?.InnerText)
						};
					}

					JourneyComponent component = new JourneyComponent() {
						Carrier = xmlComponent["Vervoerder"].InnerText,
						TransportType = xmlComponent["VervoerType"].InnerText,
						Status = JourneyStatusFunctions.JCStatusFromString(xmlComponent["Status"].InnerText),
						Departure = parseJourneyStop(xmlStops[0]),
						Arrival = parseJourneyStop(xmlStops[xmlStops.Count - 1])
					};

					journeys[i].Components.Add(component);
				}
			}
			return journeys;
		}

		#region IDisposable Support
		private bool m_DisposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing) {
			if (!m_DisposedValue) {
				if (disposing) {
					m_RestApi.Dispose();
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
