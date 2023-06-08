using Newtonsoft.Json;
using StyexFleetManagement.TripLogging.Models.ProtoEvents.Enums;

namespace StyexFleetManagement.TripLogging.Models.ProtoEvents
{
    [global::ProtoBuf.ProtoContract(Name = @"EventHeader")]
    public partial class EventHeader : global::ProtoBuf.IExtensible
    {
        private uint _TemplateId;
        [global::ProtoBuf.ProtoMember(1, IsRequired = true, Name = @"TemplateId", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        public uint TemplateId
        {
            get => _TemplateId;
            set => _TemplateId = value;
        }
        private string _UnitId = "";
        [global::ProtoBuf.ProtoMember(2, IsRequired = false, Name = @"UnitId", DataFormat = global::ProtoBuf.DataFormat.Default)]
        [JsonProperty]
        [global::System.ComponentModel.DefaultValue("")]
        public string UnitId
        {
            get => _UnitId;
            set => _UnitId = value;
        }
        private string _Description = "";
        [global::ProtoBuf.ProtoMember(3, IsRequired = false, Name = @"Description", DataFormat = global::ProtoBuf.DataFormat.Default)]
        [global::System.ComponentModel.DefaultValue("")]
        [JsonProperty]
        public string Description
        {
            get => _Description;
            set => _Description = value;
        }
        private ulong _UtcTimestampSeconds = default(ulong);
        [global::ProtoBuf.ProtoMember(4, IsRequired = false, Name = @"UtcTimestampSeconds", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(ulong))]
        [JsonProperty]
        public ulong UtcTimestampSeconds
        {
            get => _UtcTimestampSeconds;
            set => _UtcTimestampSeconds = value;
        }
        private double _Latitude = default(double);
        [global::ProtoBuf.ProtoMember(5, IsRequired = false, Name = @"Latitude", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(double))]
        [JsonProperty]
        public double Latitude
        {
            get => _Latitude;
            set => _Latitude = value;
        }
        private double _Longitude = default(double);
        [global::ProtoBuf.ProtoMember(6, IsRequired = false, Name = @"Longitude", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(double))]
        [JsonProperty]
        public double Longitude
        {
            get => _Longitude;
            set => _Longitude = value;
        }
        private uint _Speed = default(uint);
        [global::ProtoBuf.ProtoMember(7, IsRequired = false, Name = @"Speed", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        [JsonProperty]
        public uint Speed
        {
            get => _Speed;
            set => _Speed = value;
        }
        private uint _Direction = default(uint);
        [global::ProtoBuf.ProtoMember(8, IsRequired = false, Name = @"Direction", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        [JsonProperty]
        public uint Direction
        {
            get => _Direction;
            set => _Direction = value;
        }
        private uint _Odometer = default(uint);
        [global::ProtoBuf.ProtoMember(9, IsRequired = false, Name = @"Odometer", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        [JsonProperty]
        public uint Odometer
        {
            get => _Odometer;
            set => _Odometer = value;
        }
        private uint _InputStatus = default(uint);
        [global::ProtoBuf.ProtoMember(10, IsRequired = false, Name = @"InputStatus", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        [JsonProperty]
        public uint InputStatus
        {
            get => _InputStatus;
            set => _InputStatus = value;
        }
        private uint _OutputStatus = default(uint);
        [global::ProtoBuf.ProtoMember(11, IsRequired = false, Name = @"OutputStatus", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        [JsonProperty]
        public uint OutputStatus
        {
            get => _OutputStatus;
            set => _OutputStatus = value;
        }
        private uint _DriverKeyCode = default(uint);
        [global::ProtoBuf.ProtoMember(12, IsRequired = false, Name = @"DriverKeyCode", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        [JsonProperty]
        public uint DriverKeyCode
        {
            get => _DriverKeyCode;
            set => _DriverKeyCode = value;
        }
        private uint _Source = default(uint);
        [global::ProtoBuf.ProtoMember(13, IsRequired = false, Name = @"Source", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        [JsonProperty]
        public uint Source
        {
            get => _Source;
            set => _Source = value;
        }
        private uint _GeneralStatus = default(uint);
        [global::ProtoBuf.ProtoMember(14, IsRequired = false, Name = @"GeneralStatus", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(default(uint))]
        [JsonProperty]
        public uint GeneralStatus
        {
            get => _GeneralStatus;
            set => _GeneralStatus = value;
        }
        private TripSpeedSource _TripSpeedSource = TripSpeedSource.TRIPSPEEDSOURCENOTSUPPORTED;
        [global::ProtoBuf.ProtoMember(17, IsRequired = false, Name = @"TripSpeedSource", DataFormat = global::ProtoBuf.DataFormat.TwosComplement)]
        [global::System.ComponentModel.DefaultValue(TripSpeedSource.TRIPSPEEDSOURCENOTSUPPORTED)]
        [JsonProperty]
        public TripSpeedSource TripSpeedSource
        {
            get => _TripSpeedSource;
            set => _TripSpeedSource = value;
        }
        private global::ProtoBuf.IExtension extensionObject;
        global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
        { return global::ProtoBuf.Extensible.GetExtensionObject(ref extensionObject, createIfMissing); }
    }

}
