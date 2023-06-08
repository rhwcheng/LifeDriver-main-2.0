using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace StyexFleetManagement.MProfiler.Entities
{
	public class SendMessagesResult
	{
		// in case of error
		public string Code { get; set; }
		public string Message { get; set; }
		
		// in case of success
		[JsonProperty(PropertyName = "batch_id")]
		public string BatchId { get; set; }
		
		[JsonProperty(PropertyName = "results")]
		public List<MessageDeliveryResult> Results { get; set; }

		public bool Success { get; set; }


		public override string ToString()
		{
			if (Success)
			{
				return string.Format(CultureInfo.InvariantCulture, "Success: {0}, BatchId: {1}, MessageDeliveryResult: {2}",
					Success, BatchId, string.Join(",", Results.Select(v => "{" + v + "}")));
			}

			return string.Format(CultureInfo.InvariantCulture, "Success: {0}, Code: {1}, Message: {2}", Success, Code, Message);
		}
	}
}
