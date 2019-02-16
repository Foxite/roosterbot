using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using PublicTransitComponent.DataTypes;

namespace PublicTransitComponent.Services {
	public class NSAPI {
		// Documentation: https://www.ns.nl/en/travel-information/ns-api/
		private XmlRestApi m_RestApi;
		private Regex m_DelayRegex;

		public NSAPI(XmlRestApi restApi) {
			m_RestApi = restApi;
			m_DelayRegex = new Regex("[0-9]+");
		}

		public async Task<Journey[]> GetTravelRecommendation(int amount, string from, string to) {
			XmlDocument response = await m_RestApi.GetXmlOutputAsync("treinplanner", new Dictionary<string, string>() {
				{ "fromStation", from },
				{ "toStation", to },
				{ "previousAdvices", "1" }
			});

			XmlNodeList xmlOptions = response["ReisMogelijkheden"].ChildNodes;
			Journey[] journeys = new Journey[amount];

			for (int i = 0; i < amount; i++) {
				XmlNode xmlJourney = xmlOptions[i];

				Func<string, int> parseDeparture = (input) => {
					if (input == null) {
						return 0;
					}
					Match depDelayMatch = m_DelayRegex.Match(input);
					if (depDelayMatch.Success) {
						var result = depDelayMatch.Value;
						return int.Parse(result);
					} else {
						throw new XmlException("Departure delay is not valid: " + input);
					}
				};

				journeys[i] = new Journey() {
					Transfers = int.Parse(xmlJourney["AantalOverstappen"].InnerText),
					PlannedDuration = TimeSpan.ParseExact(xmlJourney["GeplandeReisTijd"].InnerText, @"h\:mm", CultureInfo.InvariantCulture),
					ActualDuration = TimeSpan.ParseExact(xmlJourney["ActueleReisTijd"].InnerText, @"h\:mm", CultureInfo.InvariantCulture),
					PlannedDepartureTime = DateTime.Parse(xmlJourney["GeplandeVertrekTijd"].InnerText),
					ActualDepartureTime = DateTime.Parse(xmlJourney["ActueleVertrekTijd"].InnerText),
					PlannedArrivalTime = DateTime.Parse(xmlJourney["GeplandeAankomstTijd"].InnerText),
					ActualArrivalTime = DateTime.Parse(xmlJourney["ActueleAankomstTijd"].InnerText),
					DepartureDelayMinutes = parseDeparture(xmlJourney["VertrekVertraging"]?.InnerText),
					ArrivalDelayMinutes = parseDeparture(xmlJourney["AankomstVertraging"]?.InnerText),
					Status = JourneyStatusFunctions.JStatusFromString(xmlJourney["Status"].InnerText)
				};

				XmlNodeList xmlComponents = xmlJourney.SelectNodes("ReisDeel");
				journeys[i].Components = new List<JourneyComponent>();
				foreach (XmlNode xmlComponent in xmlComponents) {
					XmlNodeList xmlStops = xmlComponent.SelectNodes("ReisStop");

					Func<XmlNode, JourneyStop> parseJourneyStop = (xmlStop) => {
						XmlNode xmlStopPlatform = xmlStop["Spoor"];
						return new JourneyStop() {
							Location = xmlStop["Naam"].InnerText,
							Platform = xmlStopPlatform.InnerText,
							PlatformModified = bool.Parse((xmlStopPlatform.Attributes["wijziging"]?.InnerText) ?? "false"),
							Time = DateTime.Parse(xmlStop["Tijd"].InnerText),
							DelayMinutes = parseDeparture(xmlStop["VertrekVertraging"]?.InnerText)
						};
					};
					
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
	}
}
