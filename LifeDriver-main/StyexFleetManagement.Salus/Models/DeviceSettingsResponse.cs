using Newtonsoft.Json;

namespace StyexFleetManagement.Salus.Models
{
    public class DeviceSettingsResponse : StatusMessage
    {
        [JsonProperty("result")]
        public DeviceSettings DeviceSettings { get; set; }
    }

    public class DeviceSettings
    {
        [JsonProperty("device_setting")]
        public DeviceSetting DeviceSetting { get; set; }
        [JsonProperty("guardian_angel")]
        public GuardianAngel GuardianAngel { get; set; }
        [JsonProperty("amber_alert")]
        public AmberAlert AmberAlert { get; set; }
        [JsonProperty("sos")]
        public Sos Sos { get; set; }
        [JsonProperty("nfc")]
        public Nfc Nfc { get; set; }
        [JsonProperty("power_status")]
        public PowerStatus PowerStatus { get; set; }
        [JsonProperty("battery_low_status")]
        public BatteryLowStatus BatteryLowStatus { get; set; }
        [JsonProperty("speed_alert")]
        public SpeedAlert SpeedAlert { get; set; }
        [JsonProperty("acceleration_sensor")]
        public AccelerationSensor AccelerationSensor { get; set; }
        [JsonProperty("emergency_protocol")]
        public EmergencyProtocol EmergencyProtocol { get; set; }
        [JsonProperty("non_movement")]
        public NonMovement NonMovement { get; set; }
        [JsonProperty("no_activity")]
        public NoActivity NoActivity { get; set; }
    }

    public class DeviceSetting
    {
        [JsonProperty("setting_device_id")] public string SettingDeviceId { get; set; }
        [JsonProperty("b_cid")] public string DeviceId { get; set; }
        [JsonProperty("service_start_time")] public string ServiceStartTime { get; set; }
        [JsonProperty("service_end_time")] public string ServiceEndTime { get; set; }
        [JsonProperty("track_interval")] public string TrackInterval { get; set; }

        [JsonProperty("transmission_interval")]
        public string TransmissionInterval { get; set; }

        [JsonProperty("monday")] public string Monday { get; set; }
        [JsonProperty("tuesday")] public string Tuesday { get; set; }
        [JsonProperty("wednesday")] public string Wednesday { get; set; }
        [JsonProperty("thursday")] public string Thursday { get; set; }
        [JsonProperty("friday")] public string Friday { get; set; }
        [JsonProperty("saturday")] public string Saturday { get; set; }
        [JsonProperty("sunday")] public string Sunday { get; set; }
        [JsonProperty("mobile_mute")] public string MobileMute { get; set; }
    }


    public class GuardianAngel
    {
        [JsonProperty("guardian_angel")] public string GuardianAngelString { get; set; }
        [JsonProperty("guardian_mode")] public string GuardianMode { get; set; }
        [JsonProperty("guardian_timeout")] public string GuardianTimeout { get; set; }

        [JsonProperty("guardian_warning_timeout")]
        public string GuardianWarningTimeout { get; set; }

        [JsonProperty("guardian_beep")] public string GuardianBeep { get; set; }
        [JsonProperty("guardian_vibrate")] public string GuardianVibrate { get; set; }
        [JsonProperty("guardian_hands")] public string GuardianHands { get; set; }
    }

    public class AmberAlert
    {
        [JsonProperty("amber_alert")] public string AmberAlertString { get; set; }
        [JsonProperty("amber_alert_mode")] public string AmberAlertMode { get; set; }
        [JsonProperty("amber_alert_timeout")] public string AmberAlertTimeout { get; set; }

        [JsonProperty("amber_alert_warn_timeout")]
        public string AmberAlertWarnTimeout { get; set; }

        [JsonProperty("amber_alert_beep")] public string AmberAlertBeep { get; set; }
        [JsonProperty("amber_alert_vibrate")] public string AmberAlertVibrate { get; set; }
        [JsonProperty("amber_alert_hands")] public string AmberAlertHands { get; set; }
    }

    public class Nfc
    {
        [JsonProperty("nfc")] public string NfcString { get; set; }
    }

    public class PowerStatus
    {
        [JsonProperty("power_status")] public string PowerStatusString { get; set; }
    }

    public class BatteryLowStatus
    {
        [JsonProperty("battery_low_status")] public string BatteryLowStatusString { get; set; }
        [JsonProperty("battery_lavel")] public string BatteryLevel { get; set; }
    }

    public class SpeedAlert
    {
        [JsonProperty("speed_alert")] public string SpeedAlertString { get; set; }
        [JsonProperty("max_speed")] public string MaxSpeed { get; set; }
        [JsonProperty("speed_alert_beep")] public string SpeedAlertBeep { get; set; }
        [JsonProperty("speed_alert_vibrate")] public string SpeedAlertVibrate { get; set; }
    }

    public class AccelerationSensor
    {
        [JsonProperty("accel_sensor")] public string AccelSensor { get; set; }
        [JsonProperty("sensitivity")] public string Sensitivity { get; set; }
        [JsonProperty("vetically_timeout")] public string VeticallyTimeout { get; set; }
        [JsonProperty("accell_event")] public string AccellEvent { get; set; }
        [JsonProperty("loss_of_vertically")] public string LossOfVertically { get; set; }
        [JsonProperty("mode")] public string Mode { get; set; }
    }

    public class EmergencyProtocol
    {
        [JsonProperty("emergency_protocol")] public string EmergencyProtocolString { get; set; }
        [JsonProperty("emergency_phone")] public string EmergencyPhone { get; set; }
        [JsonProperty("emergency_skill")] public string EmergencySkill { get; set; }
    }

    public class NonMovement
    {
        [JsonProperty("non_movement")] public string NonMovementString { get; set; }
        [JsonProperty("non_movement_mode")] public string NonMovementMode { get; set; }
        [JsonProperty("non_movement_timeout")] public string NonMovementTimeout { get; set; }

        [JsonProperty("non_movement_warning_timeout")]
        public string NonMovementWarningTimeout { get; set; }

        [JsonProperty("non_movement_warning_beep")]
        public string NonMovementWarningBeep { get; set; }

        [JsonProperty("non_movement_warning_vibrate")]
        public string NonMovementWarningVibrate { get; set; }

        [JsonProperty("non_movement_warning_hand")]
        public string NonMovementWarningHand { get; set; }
    }

    public class Sos
    {
        [JsonProperty("device_emergency_number")]
        public string DeviceEmergencyNumber { get; set; }

        [JsonProperty("emc_mode")] public string EmcMode { get; set; }

        [JsonProperty("sos_tracking_interval")]
        public string SosTrackingInterval { get; set; }

        [JsonProperty("timeout")] public string Timeout { get; set; }
        [JsonProperty("warning_mode_beep")] public string WarningModeBeep { get; set; }
        [JsonProperty("warning_mode_vibrate")] public string WarningModeVibrate { get; set; }
        [JsonProperty("warning_mode_hands")] public string WarningModeHands { get; set; }
    }

    public class NoActivity
    {
        [JsonProperty("no_activity_type")] public string NoActivityType { get; set; }
        [JsonProperty("no_activity_interval")] public string NoActivityInterval { get; set; }

        [JsonProperty("no_activity_interval_min")]
        public string NoActivityIntervalMin { get; set; }
    }
}