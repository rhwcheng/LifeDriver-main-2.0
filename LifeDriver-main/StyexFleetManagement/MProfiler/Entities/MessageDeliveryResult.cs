using System.Globalization;
using Newtonsoft.Json;

namespace StyexFleetManagement.MProfiler.Entities
{
	public class MessageDeliveryResult
	{
		[JsonProperty(PropertyName = "id")]
		public uint Id { get; set; }

		[JsonProperty(PropertyName = "result")]
		public string Result { get; set; }


		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "Id: {0}, Result: {1}", Id, Result);
		}
	}
}
