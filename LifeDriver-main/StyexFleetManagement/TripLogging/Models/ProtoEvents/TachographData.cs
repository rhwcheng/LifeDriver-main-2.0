namespace StyexFleetManagement.TripLogging.Models.ProtoEvents
{
    [global::ProtoBuf.ProtoContract(Name = @"TachographData")]
    public partial class TachographData : global::ProtoBuf.IExtensible
    {
        private EventHeader _header;
        [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name = @"header", DataFormat = global::ProtoBuf.DataFormat.Default)]
        public EventHeader Header
        {
            get => _header;
            set => _header = value;
        }
        private uint _rpm = default(uint);
        [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name = @"rpm", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        public uint Rpm
        {
            get => _rpm;
            set => _rpm = value;
        }
        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }
}
