using Newtonsoft.Json;

namespace StyexFleetManagement.Salus.Models
{
    public class DeviceSettingsRequest
    {
        [JsonProperty("b_cid")]
        public string DeviceId { get; set; }
        [JsonProperty("uid")]
        public string UserId { get; set; }
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
    }
}