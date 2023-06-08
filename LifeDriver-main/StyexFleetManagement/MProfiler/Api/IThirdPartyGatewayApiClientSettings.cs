namespace StyexFleetManagement.MProfiler.Api
{
	public interface IThirdPartyGatewayApiClientSettings
	{
		/// <summary>
		/// API base url
		/// </summary>
		string BaseUrl { get; }

		/// <summary>
		/// Service provider name for basic authentication
		/// </summary>
		string ServiceProviderName { get; }

		/// <summary>
		/// Service provider ApiKey for basic authentication
		/// </summary>
		string ServiceProviderApiKey { get; }
	}
}
