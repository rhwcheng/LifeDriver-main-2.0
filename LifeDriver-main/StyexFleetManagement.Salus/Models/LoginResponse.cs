using Newtonsoft.Json;

namespace StyexFleetManagement.Salus.Models
{
    public class LoginResponse : StatusMessage
    {
        [JsonProperty("result")] public SalusUser User { get; set; }
    }
}