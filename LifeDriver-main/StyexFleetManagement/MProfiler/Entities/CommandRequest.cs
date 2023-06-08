using System.Globalization;
using Newtonsoft.Json;
using StyexFleetManagement.MProfiler.Common;

namespace StyexFleetManagement.MProfiler.Entities
{
	public class CommandRequest
	{
		[JsonProperty(PropertyName = "type")]
		public string RequestType { get; set; }

		[JsonProperty(PropertyName = "body")]
		public byte[] Data { get; set; }


		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "Type: {0}, Data: {1}", RequestType, Utils.AsHexString(Data));
		}
	}
}
