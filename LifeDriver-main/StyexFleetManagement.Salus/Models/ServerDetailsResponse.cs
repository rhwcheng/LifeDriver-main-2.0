using Newtonsoft.Json;

namespace StyexFleetManagement.Salus.Models
{
    public class ServerDetailsResponse : StatusMessage
    {
        [JsonProperty("result")] public ServerDetails ServerDetails { get; set; }
    }

    public class ServerDetails
    {
        [JsonProperty("b_cid")]
        public string b_cid{ get; set; }
        [JsonProperty("uid")]
        public string uid{ get; set; }
        [JsonProperty("b_phonenr")]
        public string b_phonenr{ get; set; }
        [JsonProperty("number_of_btn")]
        public string number_of_btn{ get; set; }
        [JsonProperty("b_uniqueID")]
        public string b_uniqueID{ get; set; }
        [JsonProperty("b_imei")]
        public string b_imei{ get; set; }
        [JsonProperty("b_type")]
        public string b_type{ get; set; }
        [JsonProperty("b_name")]
        public string b_name{ get; set; }
        [JsonProperty("b_password")]
        public string b_password{ get; set; }
        [JsonProperty("b_apn")]
        public string b_apn{ get; set; }
        [JsonProperty("id")]
        public string id{ get; set; }
        [JsonProperty("b_sex")]
        public string b_sex{ get; set; }
        [JsonProperty("b_firstname")]
        public string b_firstname{ get; set; }
        [JsonProperty("b_lastname")]
        public string b_lastname{ get; set; }
        [JsonProperty("b_streetname")]
        public string b_streetname{ get; set; }
        [JsonProperty("b_streetnumber")]
        public string b_streetnumber{ get; set; }
        [JsonProperty("b_postcode")]
        public string b_postcode{ get; set; }
        [JsonProperty("b_city")]
        public string b_city{ get; set; }
        [JsonProperty("b_day")]
        public string b_day{ get; set; }
        [JsonProperty("b_country")]
        public string b_country{ get; set; }
        [JsonProperty("country_code")]
        public string country_code{ get; set; }
        [JsonProperty("b_lastupdated")]
        public string b_lastupdated{ get; set; }
        [JsonProperty("b_creationdate")]
        public string b_creationdate{ get; set; }
        [JsonProperty("latest_log")]
        public string latest_log{ get; set; }
        [JsonProperty("latest_longitude")]
        public string latest_longitude{ get; set; }
        [JsonProperty("latest_latitude")]
        public string latest_latitude{ get; set; }
        [JsonProperty("device_type_id")]
        public string device_type_id{ get; set; }
        [JsonProperty("device_brand")]
        public string device_brand{ get; set; }
        [JsonProperty("usr_timezone_id")]
        public string usr_timezone_id{ get; set; }
        [JsonProperty("usr_timezone_offset")]
        public string usr_timezone_offset{ get; set; }
        [JsonProperty("primary_host")]
        public string primary_host{ get; set; }
        [JsonProperty("primary_port")]
        public string primary_port{ get; set; }
        [JsonProperty("secondary_host")]
        public string secondary_host{ get; set; }
        [JsonProperty("secondary_port")]
        public string secondary_port{ get; set; }
        [JsonProperty("ebeacons")]
        public string ebeacons{ get; set; }
        [JsonProperty("smsc_gateway")]
        public string smsc_gateway{ get; set; }
        [JsonProperty("inbox")]
        public string inbox{ get; set; }
        [JsonProperty("enable_monitoring")]
        public string enable_monitoring{ get; set; }
        [JsonProperty("location_link")]
        public string location_link{ get; set; }
        [JsonProperty("silent_current_location")]
        public string silent_current_location{ get; set; }
        [JsonProperty("access_point_id")]
        public string access_point_id{ get; set; }
        [JsonProperty("wifi_access_point")]
        public string wifi_access_point{ get; set; }
        [JsonProperty("device_name")]
        public string device_name{ get; set; }
    }
}