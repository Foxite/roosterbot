using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoosterBot.PublicTransit {
	public class NSAPI {
		// TODO reimplement using json api
		// Documentation: https://www.ns.nl/en/travel-information/ns-api/

		public NSAPI(string username, string password) { }

		public Task<IList<Journey>> GetTravelRecommendation(int amount, StationInfo from, StationInfo to) => throw new NotImplementedException();
	}
}
