using Newtonsoft.Json;

namespace StyexFleetManagement.Salus.Models
{
    public class ServerDetailsRequest
    {
        [JsonProperty("b_cid")]
        public string DeviceId { get; set; }
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}