using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace StyexFleetManagement.MProfiler.Entities
{
	public class GetCommandRequestsResult
	{
		// in case of error
		public string Code { get; set; }
		public string Message { get; set; }

		// in case of success
		[JsonProperty(PropertyName = "batch_id")]
		public string BatchId { get; set; }

		[JsonProperty(PropertyName = "messages")]
		public List<CommandRequest> CommandRequests { get; set; }


		public bool Success { get; set; }


		public GetCommandRequestsResult()
		{
			CommandRequests = new List<CommandRequest>();
		}


		public override string ToString()
		{
			if (Success)
			{
				return string.Format(CultureInfo.InvariantCulture, "Success: {0}, BatchId: {1}, CommandRequests: {2}",
					Success, BatchId, string.Join(",", CommandRequests));
			}

			return string.Format(CultureInfo.InvariantCulture, "Success: {0}, Code: {1}, Message: {2}", Success, Code, Message);
		}
	}
}
