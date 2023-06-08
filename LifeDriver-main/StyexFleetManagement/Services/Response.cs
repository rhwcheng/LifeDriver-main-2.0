using System;
using StyexFleetManagement.Resx;

namespace StyexFleetManagement.Services
{
	public class Response
	{
		public ServiceError ErrorStatus { get; set; }
		public object ResponseToken { get; set;}

		public Response(ServiceError error, object response)
		{
			ErrorStatus = error;
			ResponseToken = response;
		}

		public Response()
		{
			ErrorStatus = ServiceError.NO_ERROR;
			ResponseToken = new object();
		}

		public string GetErrorMessage()
		{
			switch (ErrorStatus)
			{
				case ServiceError.SERVER_CONNECTION_ERROR:
					return AppResources.server_connection_error;
				case ServiceError.MISSING_CREDENTIALS_ERROR:
					return AppResources.login_error_credentials_missing;	 
				case ServiceError.AUTHENTICATION_ERROR:
					return AppResources.authentication_error;
				case ServiceError.PARSE_ERROR:
					return AppResources.parse_error;
				case ServiceError.NO_DATA:
					return AppResources.no_data_error;
				case ServiceError.UNKNOWN_SERVICE_ERROR:
					return AppResources.unknown_service_error;
				default:
					return AppResources.unknown_service_error;
			}
		}
	}
}

