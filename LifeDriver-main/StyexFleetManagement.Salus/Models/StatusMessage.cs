using Newtonsoft.Json;

namespace StyexFleetManagement.Salus.Models
{
    public class StatusMessage
    {
        [JsonProperty("status")] 
        public string Status { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("zone_status")]
        public string ZoneStatus { get; set; }
        [JsonProperty("transmission_interval")]
        public string TransmissionInterval { get; set; }
        [JsonProperty("track_interval")]
        public string TrackInterval { get; set; }

        public override string ToString()
        {
            return $"StatusMessage(status=\'{Status}\', message=\'{Message}\')";
        }
    }
}