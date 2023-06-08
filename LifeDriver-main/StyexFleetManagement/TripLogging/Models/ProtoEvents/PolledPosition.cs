namespace StyexFleetManagement.TripLogging.Models.ProtoEvents
{
    [global::ProtoBuf.ProtoContract(Name = @"PolledPosition")]
    public partial class PolledPosition : global::ProtoBuf.IExtensible
    {
        private EventHeader _header;
        [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name = @"header", DataFormat = global::ProtoBuf.DataFormat.Default)]
        public EventHeader Header
        {
            get => _header;
            set => _header = value;
        }
        private uint _tripDistanceMeters = default(uint);
        [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name = @"trip_distance_meters", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
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
