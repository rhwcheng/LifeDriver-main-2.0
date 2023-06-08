using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Acr.UserDialogs;
using RestSharp.Portable;
using RestSharp.Portable.Authenticators;
using RestSharp.Portable.HttpClient;
using StyexFleetManagement.MProfiler.Common;
using StyexFleetManagement.MProfiler.Entities;

namespace StyexFleetManagement.MProfiler.Api
{
	public class ThirdPartyGatewayApiClient
	{
		private const string HeaderContentTypeKey = "Content-Type";
		private const string HeaderAcceptKey = "Accept";
		private const string HeaderDateKey = "Date";
		private const string ContentTypeJson = "application/json";
		private const string GmtDateFormat = "r"; // Sat, 16 Aug 2008 10:38:39 GMT
		
		private const string SendMessagesRequestPath = "api/v1/messages";
		private const string GetCommandRequestsRequestPath = "api/v1/requests";


        /// <summary>
		/// Send a batch of messages
		/// </summary>
		/// <param name="messageBatch"></param>
		/// <returns></returns>
		public async Task<SendMessagesResult> SendMessages(MessageBatch messageBatch)
		{
			
			IRestRequest request = new RestRequest(SendMessagesRequestPath, Method.POST);
			//request.RequestFormat = DataFormat.Json;
			request.Serializer = new CustomJsonSerializer();
			request.AddHeader(HeaderContentTypeKey, ContentTypeJson);
			request.AddHeader(HeaderAcceptKey, ContentTypeJson);
			request.AddHeader(HeaderDateKey, DateTime.UtcNow.ToString(GmtDateFormat, CultureInfo.InvariantCulture));
			request.AddJsonBody(messageBatch);

			//Logger.Debug("Connecting to '{0}' with ServiceProviderName '{1}'", _settings.BaseUrl, _settings.ServiceProviderName);
			IRestResponse<SendMessagesResult> response = await NewRestClient.Execute<SendMessagesResult>(request);

			//Logger.Debug("ResponseStatus: {0}", response.ResponseStatus);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				//Logger.Warn("Could not send messages. ResponseStatus: {0}, ErrorMessage: {1}", response.ResponseStatus, response.ErrorMessage);
				return response.Data;
			}

			if (response.StatusCode == HttpStatusCode.OK)
			{
				//Logger.Debug("StatusCode: {0}: {1}", response.StatusCode, response.StatusDescription);
				//response.Data.Success = true;
                if (!response.Data.Success && response.Data.Results.FirstOrDefault()?.Result == "UnitNotAccessible" && !App.Singleton.HasShownMProfilerNotProvisionedWarning)
                {
					Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
                    {
                        UserDialogs.Instance.Toast( new ToastConfig("The Device ID needs to be provisioned before trips can be logged.")
                        {
                            Duration = TimeSpan.FromSeconds(3)
						});

                    });
                    App.Singleton.HasShownMProfilerNotProvisionedWarning = true;
				}
				return response.Data;
			}

			//Logger.Warn("Could not send messages. StatusCode: {0}: {1}", response.StatusCode, response.StatusDescription);
			return response.Data;
		}


		/// <summary>
		/// Get command requests for the specified service provider units
		/// </summary>
		/// <returns></returns>
		public async Task<GetCommandRequestsResult> GetCommandRequests()
		{
			//Logger.Debug("GetCommandRequests");

			IRestRequest request = new RestRequest(GetCommandRequestsRequestPath, Method.GET);
			//request.RequestFormat = DataFormat.Json;
			request.Serializer = new CustomJsonSerializer();
			request.AddHeader(HeaderContentTypeKey, ContentTypeJson);
			request.AddHeader(HeaderAcceptKey, ContentTypeJson);

			//Logger.Debug("Connecting to '{0}' with ServiceProviderName '{1}'", _settings.BaseUrl, _settings.ServiceProviderName);
			IRestResponse<GetCommandRequestsResult> response = await NewRestClient.Execute<GetCommandRequestsResult>(request);

			//Logger.Debug("ResponseStatus: {0}", response.ResponseStatus);
			if (response.StatusCode != HttpStatusCode.OK)
			{
				//Logger.Warn("Could not get command requests. ResponseStatus: {0}, ErrorMessage: {1}", response.ResponseStatus, response.ErrorMessage);
				return response.Data;
			}

			if (response.StatusCode == HttpStatusCode.OK)
			{
				//Logger.Debug("StatusCode: {0}: {1}", response.StatusCode, response.StatusDescription);
				response.Data.Success = true;
				return response.Data;
			}

			//Logger.Warn("Could not get command requests. StatusCode: {0}: {1}", response.StatusCode, response.StatusDescription);
			return response.Data;
		}


		/// <summary>
		/// Create a new rest client with basic authenitcation and json serialization
		/// </summary>
		private IRestClient NewRestClient
		{
			get
			{
				var authenticator = new HttpBasicAuthenticator(Settings.ServiceProviderName, Settings.ServiceProviderApiKey);
				RestClient restClient = new RestClient(Settings.BaseUrl) { Authenticator = authenticator };

				return restClient;
			}
		}

	}
}
