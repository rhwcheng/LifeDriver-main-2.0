using System.ComponentModel;

namespace StyexFleetManagement.Salus.Enums
{
    public enum EventType
    {
        [Description("SOS")]
        Sos,

        [Description("GUARDIAN_ANGEL")]
        GuardianAngel,

        [Description("AMBER_ALERT")]
        AmberAlert,

        [Description("NFC")]
        Nfc,

        [Description("CHECK_IN")]
        CheckIn,

        [Description("CHECK_OUT")]
        CheckOut
    }
}