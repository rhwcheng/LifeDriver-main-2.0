using Newtonsoft.Json;

namespace StyexFleetManagement.Salus.Models
{
    public class SalusUser
    {
        [JsonProperty("b_cid")] public string DeviceId { get; set; }
        [JsonProperty("uid")] public string UserId { get; set; }
    }
}