using Newtonsoft.Json;

namespace StyexFleetManagement.Salus.Models
{
    public class LogEventRequest
    {
        [JsonProperty("b_cid")]
        public string DeviceId { get; set; }
        [JsonProperty("event_type")]
        public string EventType { get; set; }
        [JsonProperty("lat")]
        public string Latitude { get; set; }
        [JsonProperty("lng")]
        public string Longitude { get; set; }
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty("language_code")]
        public string LanguageCode { get; set; }
    }
}