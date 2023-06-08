namespace StyexFleetManagement.TripLogging.Models.ProtoEvents
{
    [global::ProtoBuf.ProtoContract(Name = @"TripShutdown")]
    public partial class TripShutdown : global::ProtoBuf.IExtensible
    {
        private EventHeader _header;
        [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name = @"header", DataFormat = global::ProtoBuf.DataFormat.Default)]
        public EventHeader Header
        {
            get => _header;
            set => _header = value;
        }
        private uint _tripId = default(uint);
        [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name = @"trip_id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        public uint TripId
        {
            get => _tripId;
            set => _tripId = value;
        }
        private uint _tripStartDelaySeconds = default(uint);
        [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name = @"trip_start_delay_seconds", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        public uint TripStartDelaySeconds
        {
            get => _tripStartDelaySeconds;
            set => _tripStartDelaySeconds = value;
        }
        private uint _greenTimePercent = default(uint);
        [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name = @"green_time_percent", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        public uint GreenTimePercent
        {
            get => _greenTimePercent;
            set => _greenTimePercent = value;
        }
        private uint _tripDurationSeconds = default(uint);
        [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name = @"trip_duration_seconds", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        public uint TripDurationSeconds
        {
            get => _tripDurationSeconds;
            set => _tripDurationSeconds = value;
        }
        private uint _tripDistanceMeters = default(uint);
        [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name = @"trip_distance_meters", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        public uint TripDistanceMeters
        {
            get => _tripDistanceMeters;
            set => _tripDistanceMeters = value;
        }
        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }

}
