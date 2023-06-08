namespace StyexFleetManagement
{
    public static class Constants
    {
        public static int REPORT_EXPIRES_AFTER_HOURS = 6; // How many hours the cached report data is good
        public static int TRIPS_EXPIRE_AFTER_HOURS = 1; // How many hours the cached trip data is good
        public static int LOCATIONS_EXPIRE_AFTER_MINUTES = 10; // How many minutes the cached vehicle locations are
                                                               // good

        public static string LOG_TAG = "DEBUG-";

        public static string ACCENT_COLOUR = "#3F51B5";
        
        //Event types
        public static string DRIVER_EXCEPTION_EVENTGROUP_ID = "a2b4344c-ec50-48c9-b28a-9abf354d8ce1";
        public static string DRIVER_BEHAVIOUR_EVENTGROUP_ID = "c9f30757-ef2e-4d7e-81af-e72d73eb1b5e";
        public static string ALL_EVENT_TYPES = "d36379f5-12d4-491b-a50c-4c7fc13eacb7";

        // Shared preferences
        public static string SHARED_PREFERENCES_NAME = "StyexFleetManagement";
        public static string PREF_USER_LOGGED_IN = "UserLoggedIn";
        public static string PREF_LAST_LOGIN_DATE = "LastLoginDate";
        public static string PREF_USER_ID = "UserId";
        public static string PREF_PREVIOUS_USERNAME = "PreviousUsername";
        public static string PREF_SERVER = "Server";
        public static string PREF_USERNAME = "Username";
        public static string PREF_PASSWORD = "Password";
        public static string PREF_USER_DESCRIPTION = "UserDescription";
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
        public static string PREF_MAP_GREEN_VEHICLE_COUNT = "MapGreenVehicleCount";
        public static string PREF_MAP_AMBER_VEHICLE_COUNT = "MapAmberVehicleCount";
        public static string PREF_MAP_RED_VEHICLE_COUNT = "MapRedVehicleCount";
        public static string PREF_MAP_GRAY_VEHICLE_COUNT = "MapGrayVehicleCount";
        public static string PREF_HIDE_MARKER_DEFINITIONS = "HideMarkerDefinitions";

        public static string PREF_FAVOURITE_REPORT_ID = "FavouriteReportId";
        public static string PREF_FAVOURITE_REPORT_TYPE = "FavouriteReportType";
        public static string PREF_FAVOURITE_REPORT_TITLE = "FavouriteReportTitle";
        public static string PREF_FAVOURITE_REPORT_DATE_RANGE = "FavouriteReportDateRange";
        public static string PREF_FAVOURITE_REPORT_CUSTOM_START_DATE = "FavouriteReportCustomStartDate";
        public static string PREF_FAVOURITE_REPORT_CUSTOM_END_DATE = "FavouriteReportCustomEndDate";
        public static string PREF_FAVOURITE_VEHICLE_GROUP_ID = "FavouriteVehicleGroupId";
        public static string PREF_FAVOURITE_VEHICLE_GROUP_DESCRIPTION = "FavouriteVehicleGroupDescription";

        public static string DEFAULT_MZONE_SERVER = "live.mzoneweb.net";

        public static string DATE_PREFERENCE_FORMAT = "YYYY.MM.dd";

        public static string INTENT_REMOVE_REPORT = "com.scope.mzoneformanager.intent.REMOVE_REPORT";
        public static string INTENT_REORDER_REPORTS = "com.scope.mzoneformanager.intent.REORDER_REPORTS";
        public static string INTENT_CHANGE_MAP_TYPE = "com.scope.mzoneformanager.intent.CHANGE_MAP_TYPE";
        public static string INTENT_REPORT_START_DRAG = "com.scope.mzoneformanager.intent.REPORT_START_DRAG";
        public static string INTENT_REPORT_END_DRAG = "com.scope.mzoneformanager.intent.REPORT_END_DRAG";
        public static string INTENT_UPDATE_REPORT = "com.scope.mzoneformanager.intent.UPDATE_REPORT";
        public static string INTENT_PLOT_TRIP = "com.scope.mzoneformanager.intent.PLOT_TRIP";

        public static string EXTRA_FAVOURITE_REPORT_ID = "FavouriteReportId";
        public static string EXTRA_PARAMETERS_CHANGED = "ParametersChanged";
        public static string EXTRA_CLEAR_MAP = "ClearMap";
        public static string EXTRA_MAP_TYPE = "MapType";
        public static string EXTRA_TRIP = "Trip";
        public static string EXTRA_TRIP_POSITIONS = "TripPositions";
        public static string EXTRA_TRIP_EXCEPTIONS = "TripExceptions";

        // MZone Web API
        public static string API_DATE_FORMAT = "yyyyMMdd'T'HHmmss";
        public static string SERVICE_URL_PREFIX = "https://";
        public static string SERVICE_API_URL = "/api/v3/";
    }
}
