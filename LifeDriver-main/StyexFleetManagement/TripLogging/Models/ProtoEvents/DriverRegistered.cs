namespace StyexFleetManagement.TripLogging.Models.ProtoEvents
{
    [global::ProtoBuf.ProtoContract(Name = @"DriverRegistered")]
    public partial class DriverRegistered : global::ProtoBuf.IExtensible
    {
        private EventHeader _header;
        [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name = @"header", DataFormat = global::ProtoBuf.DataFormat.Default)]
        public EventHeader Header
        {
            get => _header;
            set => _header = value;
        }
        private uint _fleetCode = default(uint);
        [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name = @"fleet_code", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        public uint FleetCode
        {
            get => _fleetCode;
            set => _fleetCode = value;
        }
        private uint _vehicleClass = default(uint);
        [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name = @"vehicle_class", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        public uint VehicleClass
        {
            get => _vehicleClass;
            set => _vehicleClass = value;
        }
        private ulong _driverId = default(ulong);
        [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name = @"driver_id", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(ulong))]
        public ulong DriverId
        {
            get => _driverId;
            set => _driverId = value;
        }
        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }
}
