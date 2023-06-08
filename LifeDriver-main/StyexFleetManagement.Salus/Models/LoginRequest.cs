using Newtonsoft.Json;

namespace StyexFleetManagement.Salus.Models
{
    public class LoginRequest
    {
        [JsonProperty("web_id")]
        public string UserId { get; set; }
        [JsonProperty("web_pass")]
        public string Password { get; set; }
        [JsonProperty("IMEI")]
        public string Imei { get; set; }
        [JsonProperty("device_token")]
        public string DeviceToken { get; set; }
        [JsonProperty("device_type")]
        public string DeviceType { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty("language_code")]
        public string LanguageCode { get; set; }
    }
}