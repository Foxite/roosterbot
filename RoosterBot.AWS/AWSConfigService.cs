using Amazon;
using Amazon.Runtime;

namespace RoosterBot.AWS {
	public class AWSConfigService {
		public AWSCredentials Credentials { get; }
		public RegionEndpoint Region { get; }

		public AWSConfigService(string accessKey, string secretKey, RegionEndpoint region) {
			Credentials = new BasicAWSCredentials(accessKey, secretKey);
			Region = region;
		}
	}
}
