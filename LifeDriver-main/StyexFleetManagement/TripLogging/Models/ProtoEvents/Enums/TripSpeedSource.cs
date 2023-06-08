using Newtonsoft.Json;

namespace StyexFleetManagement.TripLogging.Models.ProtoEvents.Enums
{
    [global::ProtoBuf.ProtoContract(Name = @"TripSpeedSource")]
    public enum TripSpeedSource
    {

        [global::ProtoBuf.ProtoEnum(Name = @"TRIP_SPEED_SOURCE_NOT_SUPPORTED")]
        [JsonProperty]
        TRIPSPEEDSOURCENOTSUPPORTED = 0,

        [global::ProtoBuf.ProtoEnum(Name = @"TRIP_SPEED_SOURCE_SPEED_SENSOR")]
        [JsonProperty]
        TRIPSPEEDSOURCESPEEDSENSOR = 1,

        [global::ProtoBuf.ProtoEnum(Name = @"TRIP_SPEED_SOURCE_GPS")]
        [JsonProperty]
        TRIPSPEEDSOURCEGPS = 2,

        [global::ProtoBuf.ProtoEnum(Name = @"TRIP_SPEED_SOURCE_CANBUS")]
        [JsonProperty]
        TRIPSPEEDSOURCECANBUS = 4
    }
}
