namespace StyexFleetManagement.TripLogging.Models.ProtoEvents
{
    [ProtoBuf.ProtoContract(Name = @"TripStartup")]
    public partial class TripStartup : global::ProtoBuf.IExtensible
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
        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }
}
