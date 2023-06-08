using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using StyexFleetManagement.Models.Enum;
using StyexFleetManagement.ViewModel;
using StyexFleetManagement.Salus.Models;

namespace StyexFleetManagement.Models
{
    public class Settings : BaseViewModel
    {

        static ISettings AppSettings => CrossSettings.Current;

        static Settings settings;
        public static Settings Current => settings ?? (settings = new Settings());

        #region Setting Constants

        public static string PREF_BUILD = "BuildNumber";
        public static string PREF_USER_LOGGED_IN = "UserLoggedIn";
        public static string PREF_LAST_LOGIN_DATE = "LastLoginDate";
        public static string PREF_USER_ID = "UserId";

        public static string PREF_PREVIOUS_USERNAME = "PreviousUsername";
        public static string PREF_SERVER = "Server";
        public static string PREF_USERNAME = "Username";
        public static string PREF_PASSWORD = "Password";
        public static string PREF_UTC_OFFSET = "UserUtcOffset";

        public static string PREF_DEVICE_ID = "DeviceId";
        public static string PREF_DRIVER_ID = "DriverId";

        public static string PREF_USER_DESCRIPTION = "UserDescription";

        private static readonly string StringDefault = string.Empty;
        private static readonly DateTime LastLoginDefault = default(DateTime);

        public static string PREF_REPORT_DATE_RANGE = "ReportDateRange";
        public static string PREF_REPORT_CUSTOM_START_DATE = "ReportCustomStartDate";
        public static string PREF_REPORT_CUSTOM_END_DATE = "ReportCustomEndDate";
        public static string PREF_VEHICLE_GROUP_ID = "VehicleGroupId";
        public static string PREF_VEHICLE_GROUP_DESCRIPTION = "VehicleGroupDescription";
        public static string PREF_DISTANCE_MEASUREMENT_UNIT = "DistanceMeasurementUnit";
        public static string PREF_FLUID_MEASUREMENT_UNIT = "FluidMeasurementUnit";
        public static string PREF_CURRENCY = "Currency";
        public static string PREF_LOGOUT_USER = "LogoutUser";
        public static string PREF_APP_VERSION = "AppVersion";
        public static string PREF_CLEAR_CACHE = "ClearCache";
        public static string PREF_REFRESH_USER_SETTINGS = "RefreshUserSettings";
        public static string PREF_IS_NEW_MAP_CREATED = "IsNewMapCreated";
        public static string PREF_MAP_TYPE = "MapType";
        public static string PREF_MAP_SHOW_GREEN_VEHICLES = "MapShowGreenVehicles";
        public static string PREF_MAP_SHOW_AMBER_VEHICLES = "MapShowAmberVehicles";
        public static string PREF_MAP_SHOW_RED_VEHICLES = "MapShowRedVehicles";
        public static string PREF_MAP_SHOW_GRAY_VEHICLES = "MapShowGrayVehicles";
        public static string PREF_MAP_SHOW_PURPLE_VEHICLES = "MapShowPurpleVehicles";
        public static string PREF_MAP_GREEN_VEHICLE_COUNT = "MapGreenVehicleCount";
        public static string PREF_MAP_AMBER_VEHICLE_COUNT = "MapAmberVehicleCount";
        public static string PREF_MAP_RED_VEHICLE_COUNT = "MapRedVehicleCount";
        public static string PREF_MAP_GRAY_VEHICLE_COUNT = "MapPurpleVehicleCount";
        public static string PREF_MAP_PURPLE_VEHICLE_COUNT = "MapGrayVehicleCount";
        public static string PREF_HIDE_MARKER_DEFINITIONS = "HideMarkerDefinitions";
        public static string PREF_LANDING_PAGE = "LandingPage";
        public static string PREF_MAP_MARKER = "MapMarker";
        public static string PREF_DEFAULT_VEHICLEGROUP = "DefaultVehicleGroup";

        public static string PREF_PLOT_UBI_EXCEPTIONS = "PlotUBIExceptions";
        public static string PREF_PLOT_FLEET_EXCEPTIONS = "PlotFleetExceptions";
        public static string PREF_MULTIPLE_TRIP_PLOTTING = "AllowMultipleTripPlotting";
        public static string PREF_SNAP_TRIP_TO_ROAD = "SnapTripToRoad";

        public static string PREF_EXCEPTION_THRESHOLD = "ExceptionThreshold";
        public static string PREF_DISTANCE_THRESHOLD = "DistanceThreshold";
        public static string PREF_DURATION_THRESHOLD = "DurationThreshold";
        public static string PREF_IDLE_THRESHOLD = "IdleThreshold";
        public static string PREF_NON_REPORTING_THRESHOLD = "NonReportingThreshold";
        public static string PREF_STOPPED_TIME_THRESHOLD = "StoppedTimeThreshold";
        public static string PREF_ACCIDENT_COUNT_THRESHOLD = "AccidentCountThreshold";
        public static string PREF_FUEL_CONSUMPTION_THRESHOLD = "FuelConsumptionThreshold";
        public static string PREF_FUEL_THEFT_THRESHOLD = "FuelTheftThreshold";
        public static string PREF_VEHICLE_DTC_THRESHOLD = "VehicleDTCCount";
        public static string PREF_REPORTING_INTERVAL = "ReportingInterval";
        public static string PREF_TRIP_RECORDING = "TripRecording";
        public static string PREF_LOCATION_UPDATES = "LocationUpdates";

        public static string PREF_LAST_EVENT = "LastEventReceived";

        public static string PREF_DASHBOARD_VG = "DashboardVehicleGroup";

        //Dashboard Reports
        public static string PREF_REPORT_ONE = "ReportOne";
        public static string PREF_REPORT_TWO = "ReportTwo";
        public static string PREF_REPORT_THREE = "ReportThree";
        public static string PREF_REPORT_FOUR = "ReportFour";
        public static string PREF_REPORT_FIVE = "ReportFive";
        public static string PREF_REPORT_SIX = "ReportSix";


        public static string PREF_FAVOURITE_REPORT_ONE = "FavouriteReportOne";
        public static string PREF_FAVOURITE_REPORT_TWO = "FavouriteReportTwo";
        public static string PREF_FAVOURITE_REPORT_THREE = "FavouriteReportThree";

        public static string PREF_SALUS_USER = "SalusUser";
        public static string PREF_SALUS_DEVICE_SETTINGS = "SalusDeviceSettings";
        public static string PREF_SALUS_SERVER_DETAILS = "SalusServerDetails";
        public static string PREF_SALUS_DRIVER_ID_IMAGE = "SalusDriverIdImage";
        public static string PREF_SALUS_COVID_HOTLINE = "SalusCovidHotline";

        public static string PREF_SALUS_USER_FIRST_NAME = "SalusUserFirstName";
        public static string PREF_SALUS_USER_LAST_NAME = "SalusUserLastName";
        public static string PREF_SALUS_USER_EMAIL_ADDRESS = "SalusUserEmailAddress";
        public static string PREF_SALUS_USER_PHONE_NUMBER = "SalusUserPhoneNumber";
        public static string PREF_SALUS_USER_SOS_NUMBER = "SalusUserSosNumber";
        public static string PREF_SALUS_USER_PRIORITY_NUMBER = "SalusUserPriorityNumber";


        #endregion

        public void Clear(bool isLoggingOut = false)
        {
            try
            {
                if (isLoggingOut)
                {
                    UserDescription = StringDefault;
                    Server = Constants.DEFAULT_MZONE_SERVER;
                    MzoneUserId = Guid.Empty;
                    SalusUser = null;
                    SalusDeviceSettings = null;
                    LastLoginDate = LastLoginDefault;
                    DistanceMeasurementUnit = Guid.Empty;
                    FluidMeasurementUnit = Guid.Empty;
                    Currency = StringDefault;

                }

                MapType = 1;
                LastEventReceived = StringDefault;
                PlotFleetExceptions = true;
                PlotUBIExceptions = false;
                AllowMultipleTripPlotting = false;
                SnapTripToRoad = true;
                LandingPage = 0;
                DeviceId = StringDefault;
                DriverId = default(uint);
                ReportingInterval = 60;
                TripRecording = false;
                LocationUpdates = false;
                SalusDriverIdImage = null;
                SalusUserFirstName = StringDefault;
                SalusUserLastName = StringDefault;
                SalusUserEmailAddress = StringDefault;
                SalusUserPhoneNumber = StringDefault;

                ClearDashboardSettings();
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, e.Message);
            }

        }
        public void ClearDashboardSettings()
        {
            ExceptionCountTreshold = 20;
            DistanceDrivenThreshold = 400;
            DriveDurationThreshold = 20;
            NonReportingThreshold = 72;
            IdleThreshold = 60;
            StoppedTimeThreshold = 10;
            AccidentCountThreshold = 1;
            VehicleDTCCountThreshold = 2;
            FuelConsumptionThreshold = 15;
            FuelTheftThreshold = 2;

            AppSettings.Remove(PREF_DASHBOARD_VG);
        }
        public string BuildNumber
        {
            get => AppSettings.GetValueOrDefault(PREF_BUILD, StringDefault);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_BUILD, value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public string UserDescription
        {
            get => AppSettings.GetValueOrDefault(PREF_USER_DESCRIPTION, StringDefault);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_USER_DESCRIPTION, value))
                {
                    OnPropertyChanged();
                }
            }
        }


        public int UtcOffset
        {
            get => AppSettings.GetValueOrDefault(PREF_UTC_OFFSET, 0);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_UTC_OFFSET, value))
                {
                    OnPropertyChanged();
                }
            }
        }


        public int MapType
        {
            get => AppSettings.GetValueOrDefault(PREF_MAP_TYPE, 1);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_MAP_TYPE, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public string Currency
        {
            get => AppSettings.GetValueOrDefault(PREF_CURRENCY, string.Empty);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_CURRENCY, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public string Server
        {
            get => AppSettings.GetValueOrDefault(PREF_SERVER, Constants.DEFAULT_MZONE_SERVER);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_SERVER, value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public int ReportingInterval
        {
            get => AppSettings.GetValueOrDefault(PREF_REPORTING_INTERVAL, 60);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_REPORTING_INTERVAL, value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public bool TripRecording
        {
            get => AppSettings.GetValueOrDefault(PREF_TRIP_RECORDING, false) && MzoneUserId != Guid.Empty;
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_TRIP_RECORDING, value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public bool LocationUpdates
        {
            get => AppSettings.GetValueOrDefault(PREF_LOCATION_UPDATES, false) && MzoneUserId != Guid.Empty;
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_LOCATION_UPDATES, value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public string DeviceId
        {
            get => AppSettings.GetValueOrDefault(PREF_DEVICE_ID, string.Empty);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_DEVICE_ID, value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public uint DriverId
        {
            get => Convert.ToUInt32(AppSettings.GetValueOrDefault(PREF_DRIVER_ID, default(int)));
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_DRIVER_ID, Convert.ToInt32(value)))
                {
                    OnPropertyChanged();
                }
            }
        }
        public Guid DistanceMeasurementUnit
        {
            get => AppSettings.GetValueOrDefault(PREF_DISTANCE_MEASUREMENT_UNIT, Guid.Empty);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_DISTANCE_MEASUREMENT_UNIT, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public Guid FluidMeasurementUnit
        {
            get => AppSettings.GetValueOrDefault(PREF_FLUID_MEASUREMENT_UNIT, Guid.Empty);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_FLUID_MEASUREMENT_UNIT, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public Guid MzoneUserId
        {
            get => AppSettings.GetValueOrDefault(PREF_USER_ID, Guid.Empty);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_USER_ID, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public DateTime LastLoginDate
        {
            get => AppSettings.GetValueOrDefault(PREF_LAST_LOGIN_DATE, LastLoginDefault);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_LAST_LOGIN_DATE, value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public string LastEventReceived
        {
            get => AppSettings.GetValueOrDefault(PREF_LAST_EVENT, StringDefault);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_LAST_EVENT, value))
                {
                    OnPropertyChanged();
                }
            }
        }


        public bool PlotFleetExceptions
        {
            get => AppSettings.GetValueOrDefault(PREF_PLOT_FLEET_EXCEPTIONS, true);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_PLOT_FLEET_EXCEPTIONS, value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public bool PlotUBIExceptions
        {
            get => AppSettings.GetValueOrDefault(PREF_PLOT_UBI_EXCEPTIONS, false);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_PLOT_UBI_EXCEPTIONS, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public bool AllowMultipleTripPlotting
        {
            get => AppSettings.GetValueOrDefault(PREF_MULTIPLE_TRIP_PLOTTING, false);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_MULTIPLE_TRIP_PLOTTING, value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public bool SnapTripToRoad
        {
            get => AppSettings.GetValueOrDefault(PREF_SNAP_TRIP_TO_ROAD, true);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_SNAP_TRIP_TO_ROAD, value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public LandingPage LandingPage
        {
            get => (LandingPage)AppSettings.GetValueOrDefault(PREF_LANDING_PAGE, 0);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_LANDING_PAGE, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }


        public MapMarkerImage MapMarker
        {
            get => (MapMarkerImage)AppSettings.GetValueOrDefault(PREF_MAP_MARKER, 0);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_MAP_MARKER, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public int DefaultVehicleGroup
        {
            get => AppSettings.GetValueOrDefault(PREF_DEFAULT_VEHICLEGROUP, 0);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_DEFAULT_VEHICLEGROUP, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public int ExceptionCountTreshold
        {
            get => AppSettings.GetValueOrDefault(PREF_EXCEPTION_THRESHOLD, 20);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_EXCEPTION_THRESHOLD, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public int DistanceDrivenThreshold
        {
            get => AppSettings.GetValueOrDefault(PREF_DISTANCE_THRESHOLD, 400);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_DISTANCE_THRESHOLD, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public int DriveDurationThreshold
        {
            get => AppSettings.GetValueOrDefault(PREF_DURATION_THRESHOLD, 20);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_DURATION_THRESHOLD, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public int NonReportingThreshold
        {
            get => AppSettings.GetValueOrDefault(PREF_NON_REPORTING_THRESHOLD, 72);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_NON_REPORTING_THRESHOLD, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public int StoppedTimeThreshold
        {
            get => AppSettings.GetValueOrDefault(PREF_STOPPED_TIME_THRESHOLD, 10);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_STOPPED_TIME_THRESHOLD, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public int AccidentCountThreshold
        {
            get => AppSettings.GetValueOrDefault(PREF_ACCIDENT_COUNT_THRESHOLD, 1);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_ACCIDENT_COUNT_THRESHOLD, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public int VehicleDTCCountThreshold
        {
            get => AppSettings.GetValueOrDefault(PREF_VEHICLE_DTC_THRESHOLD, 2);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_VEHICLE_DTC_THRESHOLD, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public int IdleThreshold
        {
            get => AppSettings.GetValueOrDefault(PREF_IDLE_THRESHOLD, 60);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_IDLE_THRESHOLD, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public int FuelConsumptionThreshold
        {
            get => AppSettings.GetValueOrDefault(PREF_FUEL_CONSUMPTION_THRESHOLD, 15);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_FUEL_CONSUMPTION_THRESHOLD, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public int FuelTheftThreshold
        {
            get => AppSettings.GetValueOrDefault(PREF_FUEL_THEFT_THRESHOLD, 2);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_FUEL_THEFT_THRESHOLD, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }


        public string DashboardVehicleGroup
        {
            get => AppSettings.GetValueOrDefault(PREF_DASHBOARD_VG, App.SelectedVehicleGroup);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_DASHBOARD_VG, (string)value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public string FavouriteReportOne
        {
            get => AppSettings.GetValueOrDefault(PREF_FAVOURITE_REPORT_ONE, StringDefault);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_FAVOURITE_REPORT_ONE, (string)value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public string FavouriteReportTwo
        {
            get => AppSettings.GetValueOrDefault(PREF_FAVOURITE_REPORT_TWO, StringDefault);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_FAVOURITE_REPORT_TWO, (string)value))
                {
                    OnPropertyChanged();
                }
            }
        }
        public string FavouriteReportThree
        {
            get => AppSettings.GetValueOrDefault(PREF_FAVOURITE_REPORT_THREE, StringDefault);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_FAVOURITE_REPORT_THREE, (string)value))
                {
                    OnPropertyChanged();
                }
            }
        }

        #region Dashboard Reports
        public DashboardReport ReportOne
        {
            get => (DashboardReport)AppSettings.GetValueOrDefault(PREF_REPORT_ONE, 0);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_REPORT_ONE, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public DashboardReport ReportTwo
        {
            get => (DashboardReport)AppSettings.GetValueOrDefault(PREF_REPORT_TWO, 1);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_REPORT_TWO, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public DashboardReport ReportThree
        {
            get => (DashboardReport)AppSettings.GetValueOrDefault(PREF_REPORT_THREE, 2);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_REPORT_THREE, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public DashboardReport ReportFour
        {
            get => (DashboardReport)AppSettings.GetValueOrDefault(PREF_REPORT_FOUR, 3);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_REPORT_FOUR, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public DashboardReport ReportFive
        {
            get => (DashboardReport)AppSettings.GetValueOrDefault(PREF_REPORT_FIVE, 4);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_REPORT_FIVE, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public DashboardReport ReportSix
        {
            get => (DashboardReport)AppSettings.GetValueOrDefault(PREF_REPORT_SIX, 5);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_REPORT_SIX, (int)value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public List<DashboardReport> DashboardReports
        {
            get
            {
                var reports = new List<DashboardReport>();

                if (ReportOne != DashboardReport.None)
                    reports.Add(ReportOne);
                if (ReportTwo != DashboardReport.None)
                    reports.Add(ReportTwo);
                if (ReportThree != DashboardReport.None)
                    reports.Add(ReportThree);
                if (ReportFour != DashboardReport.None)
                    reports.Add(ReportFour);
                if (ReportFive != DashboardReport.None)
                    reports.Add(ReportFive);
                if (ReportSix != DashboardReport.None)
                    reports.Add(ReportSix);

                return reports;
            }
        }

        public SalusUser SalusUser
        {
            get
            {
                var user = AppSettings.GetValueOrDefault(PREF_SALUS_USER, null);
                return user != null ? JsonConvert.DeserializeObject<SalusUser>(user) : null;
            }
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_SALUS_USER, JsonConvert.SerializeObject(value)))
                {
                    OnPropertyChanged();
                }
            }
        }

        public DeviceSettings SalusDeviceSettings
        {
            get
            {
                var deviceSettings = AppSettings.GetValueOrDefault(PREF_SALUS_DEVICE_SETTINGS, null);
                return deviceSettings != null ? JsonConvert.DeserializeObject<DeviceSettings>(deviceSettings) : null;
            }
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_SALUS_DEVICE_SETTINGS, JsonConvert.SerializeObject(value)))
                {
                    OnPropertyChanged();
                }
            }
        }

        public ServerDetails SalusServerDetails
        {
            get
            {
                var deviceSettings = AppSettings.GetValueOrDefault(PREF_SALUS_SERVER_DETAILS, null);
                return deviceSettings != null ? JsonConvert.DeserializeObject<ServerDetails>(deviceSettings) : null;
            }
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_SALUS_SERVER_DETAILS, JsonConvert.SerializeObject(value)))
                {
                    OnPropertyChanged();
                }
            }
        }

        public byte[] SalusDriverIdImage
        {
            get
            {
                var imageAsBase64 = AppSettings.GetValueOrDefault(PREF_SALUS_DRIVER_ID_IMAGE, StringDefault);
                return !string.IsNullOrEmpty(imageAsBase64) ? Convert.FromBase64String(imageAsBase64) : null;
            }
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_SALUS_DRIVER_ID_IMAGE, Convert.ToBase64String(value)))
                {
                    OnPropertyChanged();
                }
            }
        }

        public string SalusCovidHotline
        {
            get => AppSettings.GetValueOrDefault(PREF_SALUS_COVID_HOTLINE, "1800 675 398");
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_SALUS_COVID_HOTLINE, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public string SalusUserFirstName
        {
            get => AppSettings.GetValueOrDefault(PREF_SALUS_USER_FIRST_NAME, StringDefault);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_SALUS_USER_FIRST_NAME, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public string SalusUserLastName
        {
            get => AppSettings.GetValueOrDefault(PREF_SALUS_USER_LAST_NAME, StringDefault);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_SALUS_USER_LAST_NAME, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public string SalusUserEmailAddress
        {
            get => AppSettings.GetValueOrDefault(PREF_SALUS_USER_EMAIL_ADDRESS, StringDefault);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_SALUS_USER_EMAIL_ADDRESS, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public string SalusUserPhoneNumber
        {
            get => AppSettings.GetValueOrDefault(PREF_SALUS_USER_PHONE_NUMBER, StringDefault);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_SALUS_USER_PHONE_NUMBER, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public string SalusUserSosNumber
        {
            get => AppSettings.GetValueOrDefault(PREF_SALUS_USER_SOS_NUMBER, StringDefault);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_SALUS_USER_SOS_NUMBER, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        public string SalusUserPriorityNumber
        {
            get => AppSettings.GetValueOrDefault(PREF_SALUS_USER_PRIORITY_NUMBER, StringDefault);
            set
            {
                if (AppSettings.AddOrUpdateValue(PREF_SALUS_USER_PRIORITY_NUMBER, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        #endregion

    }
}

