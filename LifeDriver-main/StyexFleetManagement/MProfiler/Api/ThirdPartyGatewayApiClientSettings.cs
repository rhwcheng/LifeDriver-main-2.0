namespace StyexFleetManagement.MProfiler.Api
{
	public class ThirdPartyGatewayApiClientSettings : IThirdPartyGatewayApiClientSettings
	{
		/// <summary>
		/// API base url
		/// </summary>
		public string BaseUrl { get; set; }

		/// <summary>
		/// Service provider name for basic authentication
		/// </summary>
		public string ServiceProviderName { get; set; }

		/// <summary>
		/// Service provider ApiKey for basic authentication
		/// </summary>
		public string ServiceProviderApiKey { get; set; }
	}
}
