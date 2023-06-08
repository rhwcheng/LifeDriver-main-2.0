using Newtonsoft.Json;

namespace StyexFleetManagement.MProfiler.Entities
{
	public class Message 
	{
		[JsonProperty(PropertyName = "id")]
		public uint Id { get; set; }

		[JsonProperty(PropertyName = "unit_id")]
		public string UnitId { get; set; }

		[JsonProperty(PropertyName = "template_id")]
		public uint TemplateId { get; set; }

		[JsonProperty(PropertyName = "body")]
		public byte[] Data { get; set; }
	}
}
