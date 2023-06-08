namespace StyexFleetManagement.TripLogging.Models.ProtoEvents
{
    [global::ProtoBuf.ProtoContract(Name = @"TripSummary")]
    public partial class TripSummary : global::ProtoBuf.IExtensible
    {
        private EventHeader _header;
        [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name = @"header", DataFormat = global::ProtoBuf.DataFormat.Default)]
        public EventHeader Header
        {
            get => _header;
            set => _header = value;
        }
        private double _latitudeStart = default;
        [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name = @"latitude_start", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(double))]
        public double LatitudeStart
        {
            get => _latitudeStart;
            set => _latitudeStart = value;
        }
        private double _longitudeStart = default(double);
        [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name = @"longitude_start", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(double))]
        public double LongitudeStart
        {
            get => _longitudeStart;
            set => _longitudeStart = value;
        }
        private uint _tripDuration = default;
        [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name = @"trip_duration", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        public uint TripDuration
        {
            get => _tripDuration;
            set => _tripDuration = value;
        }
        private uint _tripDistance = default;
        [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name = @"trip_distance", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        public uint TripDistance
        {
            get => _tripDistance;
            set => _tripDistance = value;
        }
        private uint _tripMaxSpeed = default;
        [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name = @"trip_max_speed", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        public uint TripMaxSpeed
        {
            get => _tripMaxSpeed;
            set => _tripMaxSpeed = value;
        }
        private uint _gprsStatusStart = default;
        [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name = @"gprs_status_start", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        public uint GprsStatusStart
        {
            get => _gprsStatusStart;
            set => _gprsStatusStart = value;
        }
        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }
}
