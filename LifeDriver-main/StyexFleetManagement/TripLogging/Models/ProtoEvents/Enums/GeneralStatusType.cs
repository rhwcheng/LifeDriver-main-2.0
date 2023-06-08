namespace StyexFleetManagement.TripLogging.Models.ProtoEvents.Enums
{
    [global::ProtoBuf.ProtoContract(Name = @"GeneralStatusType")]
    public enum GeneralStatusType
    {

        [global::ProtoBuf.ProtoEnum(Name = @"GENERAL_STATUS_IGNITION")]
        GENERALSTATUSIGNITION = 1,

        [global::ProtoBuf.ProtoEnum(Name = @"GENERAL_STATUS_BATTERY")]
        GENERALSTATUSBATTERY = 2,

        [global::ProtoBuf.ProtoEnum(Name = @"GENERAL_STATUS_GPS_INFORMATION_AVAILABLE")]
        GENERALSTATUSGPSINFORMATIONAVAILABLE = 4,

        [global::ProtoBuf.ProtoEnum(Name = @"GENERAL_STATUS_INPUT_OUTPUT_INFORMATION_AVAILABLE")]
        GENERALSTATUSINPUTOUTPUTINFORMATIONAVAILABLE = 8,

        [global::ProtoBuf.ProtoEnum(Name = @"GENERAL_STATUS_GPS_POSITION_INVALID")]
        GENERALSTATUSGPSPOSITIONINVALID = 16,

        [global::ProtoBuf.ProtoEnum(Name = @"GENERAL_STATUS_GPS_STATUS_VALID")]
        GENERALSTATUSGPSSTATUSVALID = 32,

        [global::ProtoBuf.ProtoEnum(Name = @"GENERAL_STATUS_GPS_ANTENNA_CONNECTED")]
        GENERALSTATUSGPSANTENNACONNECTED = 64
    }
}
