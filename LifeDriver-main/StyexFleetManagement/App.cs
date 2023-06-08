using Acr.UserDialogs;
using Akavache;
using Microsoft.AppCenter;
using Plugin.Connectivity;
using StyexFleetManagement.Messages;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmHelpers;
using StyexFleetManagement.Abstractions;
using StyexFleetManagement.Enums;
using StyexFleetManagement.Models;
using StyexFleetManagement.ViewModel.Base;
using StyexFleetManagement.Salus.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace StyexFleetManagement
{
    public class App : Application, INotifyPropertyChanged
    {
        public static string AppName => "Life Driver";

        public static double ScreenHeight;
        public static double ScreenWidth;

        public static double PageHeight;
        public static double PageWidth;

        //public static FavouriteReportDatabase FavouriteReports { get; set; }

        //Color definitions
        public static Color LightGray = Color.FromHex("#FFE2E2E2");
        public static Color WhiteGray = Color.FromHex("#FFF2F2F2");

        public static App Singleton { get; set; }

        public static List<Task<List<Event>>> EventTaskList { get; set; }

        public static VehicleGroupCollection VehicleGroups { get; set; }

        //Picker values
        public static string SelectedVehicleGroup { get; set; }

        public static int SelectedVehicleGroupIndex { get; set; }
        public static ReportDateRange SelectedDate { get; set; }

        public static FuelSummary FuelSummary { get; set; }

        public static MainPage MainDetailPage { get; set; }

        public HttpClient HttpClient { get; set; }

        public bool HasShownMProfilerNotProvisionedWarning { get; set; } // TEMP: Remove

        #region Tracking
        readonly string logTag = "App.cs";
        readonly int NotificationId = 101;

        private TripDataProcessor tripDataProcessor { get; set; }

        private bool loggingTrip;
        //private System.Diagnostics.Stopwatch timer;
        private DateTime lastLog;
        private DateTime regularPositionTimer;
        private DateTime tachoEventTimer;
        private DateTime pollingTimer;

        private readonly TimeSpan tripStopThreshold = TimeSpan.FromMinutes(3);

        private List<Position> coordinates;
        private LocationArgs previousCoord;

        //Traccar
        public static string KEY_DEVICE = "id";
        public static string KEY_URL = "url";
        public static string KEY_INTERVAL = "interval";
        public static string KEY_DISTANCE = "distance";
        public static string KEY_ANGLE = "angle";
        public static string KEY_PROVIDER = "provider";
        public static string KEY_STATUS = "status";

        readonly object sendDataLock = new object();
        bool isSendingBufferData;

        private bool hasPooled;
        private TripPoint previousPoint;

        public LogbookTrip CurrentTrip { get; private set; }
        public string ElapsedTime { get; private set; }

        #endregion

        public App()
        {
            try
            {
                Akavache.Registrations.Start("FleetBI");

                BlobCache.ApplicationName = "FleetBI";
                CancellationToken = new CancellationTokenSource();

                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzcxMzMzQDMxMzgyZTM0MmUzMFA3cGtTa3R5QXQxa3VvUVhYOXhQeVRFZnMzNUhLZndJM2k1RDNsdUowdUk9");

                AppResources.Culture = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();

                LatestVehicleEvents = new Dictionary<string, List<Event>>();

                MainDetailPage = new MainPage { MasterBehavior = MasterBehavior.Popover };
                if (Settings.Current.DeviceId == string.Empty)
                    Settings.Current.DeviceId = Guid.NewGuid().ToString().ToUpper().Substring(0, 4);

                if (Settings.Current.DriverId == default(uint))
                {
                    Random rnd = new Random();
                    int id = rnd.Next(1000, 9999);
                    Settings.Current.DriverId = Convert.ToUInt32(id);
                }

                //Tracking
                tripDataProcessor = new TripDataProcessor();
                loggingTrip = false;

                //timer = new System.Diagnostics.Stopwatch();
                lastLog = DateTime.Now;
                regularPositionTimer = DateTime.Now;
                tachoEventTimer = DateTime.Now;
                pollingTimer = DateTime.Now;

                //Task timerTask = Task.Run(async () => TimerTask());
                coordinates = new List<Position>();

                MessagingCenter.Subscribe<ILocationManager, LocationArgs>(this, "LocationUpdated", (vm, args) => HandleLocationChanged("LocationUpdated", args));
                MessagingCenter.Subscribe<ILocationManager>(this, "PeriodicUpdate", (vm) => HandlePeriodicUpdate("PeriodicUpdate"));
                MessagingCenter.Subscribe<TickedMessage>(this, "TickedMessage", message => HandlePeriodicUpdate("PeriodicUpdate"));
                Xamarin.Essentials.Connectivity.ConnectivityChanged += Current_ConnectivityChanged;

                SendTripEndEvents();

                Singleton = this;

                Current.MainPage = MainDetailPage;
            }
            catch (System.Exception ex)
            {
                Serilog.Log.Error(ex, ex.Message);
                Crashes.TrackError(ex);
            }
        }

        private void RestartTimer()
        {
            var stopMessage = new StopLongRunningTaskMessage();
            MessagingCenter.Send(stopMessage, "StopLongRunningTaskMessage");


            var startMessage = new StartLongRunningTaskMessage();
            MessagingCenter.Send(startMessage, "StartLongRunningTaskMessage");

        }
        private void StartTimer()
        {
            var startMessage = new StartLongRunningTaskMessage();
            MessagingCenter.Send(startMessage, "StartLongRunningTaskMessage");

        }
        private void StopTimer()
        {
            var stopMessage = new StopLongRunningTaskMessage();
            MessagingCenter.Send(stopMessage, "StopLongRunningTaskMessage");

        }

        private static async Task GetEventData()
        {
            var newEvents = EventAPI.GetEventsByVehicleGroup(App.SelectedVehicleGroup, DateTime.Now.AddMonths(-1), DateTime.Now);
        }

        public static ICustomMapRenderer MapRenderer { get; private set; }
        public static Dictionary<string, List<Event>> LatestVehicleEvents { get; set; }
        public static Task GetVehicleIconsTask { get; private set; }
        public static CancellationTokenSource CancellationToken { get; private set; }
        public static double MapPadding { get; internal set; }


        public static DateTime EndDateSelected { get; internal set; }
        public static DateTime StartDateSelected { get; internal set; }

        public static void Init(ICustomMapRenderer mapRenderer)
        {
            App.MapRenderer = mapRenderer;

        }

        public static void AddtoFavourites(ReportDateRange selectedDate, int dateIndex, int vehicleIndex, ReportType reportType, string title, string vehicleGroup, Type type)
        {
            //var item = new FavouriteReport { StartDate = DateHelper.GetDateRangeStartDate(selectedDate), SelectedDateIndex = dateIndex, SelectVehicleIndex = vehicleIndex, EndDate = DateHelper.GetDateRangeEndDate(selectedDate), DateRange = selectedDate, ReportType = reportType, Title = title, VehicleGroupId = vehicleGroup };

            //FavouriteReports.SaveItem(item);
        }
        public static void AddtoFavourites(string type)
        {
            if (Settings.Current.FavouriteReportOne == string.Empty)
                Settings.Current.FavouriteReportOne = type;
            else
            {
                if (Settings.Current.FavouriteReportTwo == string.Empty)
                    Settings.Current.FavouriteReportTwo = type;

                else
                    Settings.Current.FavouriteReportThree = type;
            }
        }

        private static void RemoveFromFavourites(string type)
        {
            if (Settings.Current.FavouriteReportOne == type)
            {
                Settings.Current.FavouriteReportOne = string.Empty;
                return;
            }

            if (Settings.Current.FavouriteReportTwo == type)
            {
                Settings.Current.FavouriteReportTwo = string.Empty;
                return;
            }

            if (Settings.Current.FavouriteReportThree == type)
            {
                Settings.Current.FavouriteReportThree = string.Empty;
                return;
            }
        }

        public static void RemoveFromFavourites(int id)
        {
            //FavouriteReports.DeleteItem(id);
        }

        public static int IsFavourited(ReportDateRange selectedDate, ReportType reportType, string vehicleGroup)
        {
            return 0;//FavouriteReports.IsFavourite(selectedDate, reportType, vehicleGroup);
        }



        public static App GetInstance()
        {
            return Singleton;
        }

        /// <summary>
        /// Shows the loading indicator.
        /// </summary>
        /// <param name="isRunning">If set to <c>true</c> is running.</param>
        /// <param name = "isCancel">If set to <c>true</c> user can cancel the loading event (just uses PopModalAync here)</param>
        public static void ShowLoading(bool isRunning, string message = "Loading", bool isCancel = false)
        {
            //Translate 'loading'
            if (message == "Loading")
                message = AppResources.loading;

            if (isRunning == true)
            {
                if (isCancel == true)
                {
                    UserDialogs.Instance.Loading(message, new Action(async () =>
                    {
                        if (Application.Current.MainPage.Navigation.ModalStack.Count > 1)
                        {
                            await Application.Current.MainPage.Navigation.PopModalAsync();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("Navigation: Can't pop modal without any modals pushed");
                        }
                        UserDialogs.Instance.Loading().Hide();
                    }), cancelText: AppResources.button_cancel);
                }
                else
                {
                    UserDialogs.Instance.Loading(message);
                }
            }
            else
            {
                UserDialogs.Instance.Loading().Hide();
            }
        }

        protected override void OnStart()
        {
            AppCenter.Start("android=2bf904a2-e1ce-49d0-a121-4c81f23958f4;" +
                     "ios=51d7e0d5-fe28-4b7c-a576-ae334f76c833;",
                     typeof(Analytics), typeof(Crashes));

            // Handle when your app starts
            VehicleGroups = new VehicleGroupCollection();

        }

        public static async Task InitializeData()
        {
            var authenticationService = ViewModelLocator.Resolve<IAuthenticationService>();

            if (authenticationService.IsLoggedIn(AccountType.MZone))
            {
                EventTaskList = new List<Task<List<Event>>>();

                await GetVehicleGroups();

                GetVehicleIconsTask = GetVehicleImages();

                //GetLatestEvents();

                //GetLatestEvents();

                SelectedDate = ReportDateRange.TODAY;

                StartDateSelected = DateHelper.GetDateRangeStartDate(App.SelectedDate);
                EndDateSelected = DateHelper.GetDateRangeEndDate(App.SelectedDate);

                //await GetEventData();
            }

            if (authenticationService.IsLoggedIn(AccountType.Salus))
            {
                var salusUser = Settings.Current.SalusUser;

                Settings.Current.SalusDeviceSettings = (await ViewModelLocator.Resolve<IDeviceService>()
                    .GetDeviceSettingsAsync(salusUser.UserId, salusUser.DeviceId))?.DeviceSettings;

                Settings.Current.SalusServerDetails = (await ViewModelLocator.Resolve<IServerDetailsService>()
                    .GetServerDetails(salusUser.DeviceId))?.ServerDetails;
            }
        }

        private static async Task GetVehicleImages()
        {
            try
            {
                var isCancelled = CancellationToken.IsCancellationRequested;

                if (isCancelled)
                    throw new OperationCanceledException();

                var vehicles = await VehicleAPI.GetVehicles();

                foreach (VehicleItem vehicle in vehicles)
                {
                    isCancelled = CancellationToken.IsCancellationRequested;

                    if (isCancelled)
                        throw new OperationCanceledException();

                    await VehicleAPI.GetVehicleImage(vehicle.Id);
                }
            }
            // *** If cancellation is requested, an OperationCanceledException results.  
            catch (OperationCanceledException)
            {
                Debug.WriteLine("Task Cancelled");
            }
            catch (Exception)
            {
                Debug.WriteLine("Task Cancelled");
            }
        }

        private static async Task GetLatestEvents()
        {
            /*
             var vehicles = await VehicleAPI.GetVehiclesAsync(true);

             if (vehicles.Count > 0)
             {
                 foreach (VehicleItem vehicle in vehicles)
                 {
                     EventAPI.GetStatusEventsByVehicleAsync(vehicle.Id);
                 }

             }*/

            //Get cached events
            foreach (var group in App.VehicleGroups.VehicleGroups)
            {
                var cachedEvents = await EventAPI.GetLatestEventsFromCache(group.Id);

                if (cachedEvents != null)
                    LatestVehicleEvents[group.Id] = cachedEvents;
            };

            foreach (var group in App.VehicleGroups.VehicleGroups)
            {
                var eventTask = await EventAPI.GetLatestEvents(group.Id);

                LatestVehicleEvents[group.Id] = eventTask;
            }
        }

        protected static async Task GetLatestEvents(string vehicleGroup)
        {
            IEnumerable<Event> result = null;
            var cache = BlobCache.LocalMachine;

            /*var cachedEventsPromise = cache.GetAndFetchLatest(
        vehicleGroup,
        () => EventAPI.GetLatestEventsAsync(),
        offset =>
        {
            TimeSpan elapsed = DateTimeOffset.Now - offset;
            return elapsed > new TimeSpan(hours: 0, minutes: 10, seconds: 0);
        });

            cachedPostsPromise.Subscribe(subscribedPosts =>
            {
                Debug.WriteLine("Subscribed Posts ready");
                result = subscribedPosts;
            });

            result = await cachedPostsPromise.FirstOrDefaultAsync();
            return result;*/

        }



        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        async static Task GetVehicleGroups()
        {

            var cache = BlobCache.LocalMachine;

            var username = DependencyService.Get<ICredentialsService>().MZoneUserName;
            try
            {
                VehicleGroups = await cache.GetObject<VehicleGroupCollection>($"{username}:{"VehicleGroups"}");
            }
            catch (System.Collections.Generic.KeyNotFoundException keyNotFound)
            {
                ShowLoading(true);
                Debug.WriteLine("Debug: Fetching vehicle groups");

                VehicleGroups = await RestService.GetVehicleGroupsAsync();

                ShowLoading(false);
            }




            /*VehicleGroupCollection result = null;
            var cachedVehicleGroupPromise = cache.GetAndFetchLatest(
                "VehicleGroups",
                () => RestService.GetVehicleGroupsAsync(),
                offset =>
                {
                    TimeSpan elapsed = DateTimeOffset.Now - offset;
                    return elapsed > new TimeSpan(hours: 120, minutes: 0, seconds: 0);
                });

            cachedVehicleGroupPromise.Subscribe(subscribedVehicleGroups =>
            {
                Debug.WriteLine("Subscribed Vehicles Groups ready");
                result = subscribedVehicleGroups;
            });

            result = await cachedVehicleGroupPromise.FirstOrDefaultAsync();

            VehicleGroups = result;*/

            //Set an initial selection
            SelectedVehicleGroupIndex = 0;
            SelectedVehicleGroup = VehicleGroups.VehicleGroups[SelectedVehicleGroupIndex].Id;

        }

        public static string GetFavouriteVehicleGroup()
        {

            try
            {
                return VehicleGroups.VehicleGroups[0].Id;

            }
            catch (NullReferenceException e)
            {
                return null;
            }

        }

        public static string GetFluidAbbreviation()
        {
            var volume = Settings.Current.FluidMeasurementUnit;
            return FluidMeasurementUnit.GetAbbreviation(volume);
        }

        public static string GetDistanceAbbreviation()
        {
            var distance = Settings.Current.DistanceMeasurementUnit;
            return DistanceMeasurementUnit.GetAbbreviation(distance);
        }

        public static string GetConsumptionAbbreviation()
        {
            var distance = Settings.Current.DistanceMeasurementUnit;
            var volume = Settings.Current.FluidMeasurementUnit;

            return FluidMeasurementUnit.GetAbbreviation(volume) + "/100" + DistanceMeasurementUnit.GetAbbreviation(distance);
        }

        public bool GetMapShowVehicles(MapMarker.MapMarkerType type)
        {
            switch (type)
            {
                case MapMarker.MapMarkerType.GREEN:
                    return GetBooleanPreference(Constants.PREF_MAP_SHOW_GREEN_VEHICLES, true);
                case MapMarker.MapMarkerType.AMBER:
                    return GetBooleanPreference(Constants.PREF_MAP_SHOW_AMBER_VEHICLES, true);
                case MapMarker.MapMarkerType.RED:
                    return GetBooleanPreference(Constants.PREF_MAP_SHOW_RED_VEHICLES, true);
                case MapMarker.MapMarkerType.BLACK:
                    return GetBooleanPreference(Constants.PREF_MAP_SHOW_GRAY_VEHICLES, true);
                default:
                    return true;
            }
        }

        private bool GetBooleanPreference(string key, bool defaultValue)
        {
            //TODO: Impement
            return defaultValue;
        }




        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string name = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;
            changed(this, new PropertyChangedEventArgs(name));
        }

        #endregion


        #region Tracking

        public async void SendTripEndEvents()
        {
            try
            {
                var tripEndEvents = new List<TripEndBlob>(await BlobCache.LocalMachine.GetAllObjects<TripEndBlob>());

                if (tripEndEvents == null || tripEndEvents.Count == 0)
                    return;

                //Make sure that this thread can't be kicked off concurrently
                lock (sendDataLock)
                {
                    if (isSendingBufferData)
                    {
                        return;
                    }
                    else
                    {
                        isSendingBufferData = true;
                    }
                }


                while (Connectivity.NetworkAccess == NetworkAccess.Internet && tripEndEvents.Any())
                {
                    try
                    {
                        var tripEnd = tripEndEvents[0];
                        if (tripDataProcessor != null)
                        {
                            //Push the trip data packaged with the OBD data to the Hub
                            if (tripEnd.Point != null)
                            {
                                tripDataProcessor.SendTripPointToHub(TripEventType.TripEnd, tripEnd.Point, tripEnd.Trip);
                                tripDataProcessor.SendTripPointToHub(TripEventType.TripSummary, tripEnd.Point, tripEnd.Trip);
                            }
                            else
                            {
                                tripDataProcessor.SendTripPointToHub(TripEventType.TripEnd, null, tripEnd.Trip);
                                tripDataProcessor.SendTripPointToHub(TripEventType.TripSummary, null, tripEnd.Trip);
                            }

                            BlobCache.LocalMachine.InsertObject<LogbookEventItem>(Guid.NewGuid().ToString(), new LogbookEventItem("Stopped logging trip"));

                            await BlobCache.LocalMachine.InvalidateObject<TripEndBlob>(tripEnd.Trip.Id);

                        }
                        else
                        {

                            BlobCache.LocalMachine.InsertObject<LogbookEventItem>(Guid.NewGuid().ToString(), new LogbookEventItem("Trip Data Processor null"));
                        }
                    }
                    catch (System.Exception ex)
                    {
                        //Logger.Instance.Report(ex);
                        BlobCache.LocalMachine.InsertObject<LogbookEventItem>(Guid.NewGuid().ToString(), new LogbookEventItem(ex.ToString()));
                        Serilog.Log.Error(ex, ex.Message);
                        //loggingTrip = true;
                    }
                }

                lock (sendDataLock)
                {
                    isSendingBufferData = false;
                }
            }
            catch (System.Exception ex)
            {
                Crashes.TrackError(ex);
                Serilog.Log.Error(ex, ex.Message);
            }
        }

        private void HandlePeriodicUpdate(string v)
        {
            if (loggingTrip == true && (DateTime.Now - lastLog).TotalMilliseconds > tripStopThreshold.TotalMilliseconds)
            {
                StopLoggingTrip(previousPoint);
            }
        }

        private async void Current_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
            {
                //If connection is re-established, then kick off background thread to push buffered data to IOT Hub
                tripDataProcessor.SendBufferedDataToHub();
            }
        }

        private async Task StartLoggingTrip(LocationArgs e)
        {
            DependencyService.Get<INotificationService>().SendNotification(AppResources.app_is_recording, NotificationId);
            regularPositionTimer = DateTime.Now;
            tachoEventTimer = DateTime.Now;

            //if (timer.IsRunning)
            //    timer.Restart();
            //else
            //{
            //    timer.Reset();
            //    timer.Start();
            //}
            lastLog = DateTime.Now;
            StartTimer();

            ToastConfig cfg = new ToastConfig(AppResources.logging_started);
            cfg.Duration = TimeSpan.FromSeconds(0.8);
            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.Toast(cfg);

            });

            //AppDroid.Current.LocationService.UpdateNotification("Recording trip");

            //Start logging trip
            loggingTrip = true;
            CurrentTrip = new LogbookTrip
            {
                UnitId = Settings.Current.DeviceId,
                Points = new ObservableRangeCollection<TripPoint>(),
                StartLocalTimestamp = DateTimeOffset.UtcNow,
                DriverId = Convert.ToUInt32(Settings.Current.DriverId),
                StartLatitude = e.Latitude,
                StartLongitude = e.Longitude
            };

            //TripLogbookService.SendTripStartEvent(CurrentTrip.StartLocalTimestamp.UtcDateTime, e.Location.Latitude, e.Location.Longitude, CurrentTrip.UnitId);
            var point = new TripPoint
            {
                TripId = CurrentTrip.Id,
                RecordedTimeStamp = DateTime.UtcNow,
                Latitude = e.Latitude,
                Longitude = e.Longitude,
                Sequence = CurrentTrip.Points.Count,
                Speed = e.ConvertSpeed(),
                Bearing = e.Bearing

            };
            try
            {
                if (tripDataProcessor != null)
                {
                    //Push the trip data packaged with the OBD data to the IOT Hub
                    tripDataProcessor.SendTripPointToHub(TripEventType.TripStart, point, CurrentTrip);
                    tripDataProcessor.SendTripPointToHub(TripEventType.DriverRegistered, point, CurrentTrip);
                    await BlobCache.LocalMachine.InsertObject<LogbookEventItem>(Guid.NewGuid().ToString(), new LogbookEventItem("Started logging trip"));
                }
            }
            catch (System.Exception ex)
            {
                //Logger.Instance.Report(ex);
                await BlobCache.LocalMachine.InsertObject<LogbookEventItem>(Guid.NewGuid().ToString(), new LogbookEventItem(ex.ToString()));
                Debug.WriteLine(ex);
            }
        }

        private void StopLoggingTrip(TripPoint point)
        {
            loggingTrip = false;

            ToastConfig cfg = new ToastConfig("Logging stopped");
            cfg.Duration = TimeSpan.FromSeconds(0.8);

            Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
            {
                UserDialogs.Instance.Toast(cfg);

            });

            DependencyService.Get<INotificationService>().CancelNotification(NotificationId);

            //AppDroid.Current.LocationService.UpdateNotification("FleetBI is running");

            //stop timer and reset
            //timer.Stop();
            //timer.Reset();
            lastLog = DateTime.Now;
            StopTimer();

            //Stop logging trip and store trip details

            CurrentTrip.EndLocalTimestamp = DateTimeOffset.Now.AddMinutes(-1 * tripStopThreshold.TotalMinutes);

            BlobCache.LocalMachine.InvalidateObject<TripEndBlob>(CurrentTrip.Id);
            try
            {
                if (tripDataProcessor != null)
                {
                    //Push the trip data packaged with the OBD data to the Hub
                    if (previousPoint != null)
                    {
                        tripDataProcessor.SendTripPointToHub(TripEventType.TripEnd, point, CurrentTrip);
                        tripDataProcessor.SendTripPointToHub(TripEventType.TripSummary, point, CurrentTrip);
                    }
                    else
                    {
                        tripDataProcessor.SendTripPointToHub(TripEventType.TripEnd, null, CurrentTrip);
                        tripDataProcessor.SendTripPointToHub(TripEventType.TripSummary, null, CurrentTrip);
                    }

                    BlobCache.LocalMachine.InsertObject<LogbookEventItem>(Guid.NewGuid().ToString(), new LogbookEventItem("Stopped logging trip"));
                }
                else
                {

                    BlobCache.LocalMachine.InsertObject<LogbookEventItem>(Guid.NewGuid().ToString(), new LogbookEventItem("Trip Data Processor null"));
                }
            }
            catch (System.Exception ex)
            {
                //Logger.Instance.Report(ex);
                BlobCache.LocalMachine.InsertObject<LogbookEventItem>(Guid.NewGuid().ToString(), new LogbookEventItem(ex.ToString()));

                Serilog.Log.Error(ex, ex.Message);
                //loggingTrip = true;
            }


            //string json = JsonConvert.SerializeObject(CurrentTrip, Formatting.Indented);
            //CurrentTrip.SerializedString = json;

            //BlobCache.LocalMachine.InsertObject<LogbookTrip>(CurrentTrip.Id.ToString(), CurrentTrip);

        }
        private async Task SendPolledPosition(LocationArgs location)
        {
            pollingTimer = DateTime.Now;

            var point = new TripPoint
            {
                RecordedTimeStamp = DateTime.UtcNow,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Speed = location.ConvertSpeed(),
                Bearing = location.Bearing

            };
            try
            {
                if (tripDataProcessor != null)
                {
                    //Push the trip data packaged with the OBD data to the IOT Hub
                    tripDataProcessor.SendTripPointToHub(TripEventType.PolledPosition, point, isIgnitionOn: false);
                }
            }
            catch (System.Exception ex)
            {
                //Logger.Instance.Report(ex);
            }
            //StyexFleetManagement.Services.Firebase.SaveData<>();
        }
        private void HandleLocationChanged(string v, LocationArgs args)
        {
            try
            {
                var location = args;

                if (Settings.Current.TripRecording)
                {
                    //If not logging trip and speed is greater than 10, then start logging
                    if (loggingTrip == false && location.ConvertSpeed() > 10)
                    {
                        StartLoggingTrip(location);
                        return;
                    }


                    //If code reaches here, it will send trip position to backend
                    if (loggingTrip == true)
                    {
                        TripEventType eventType;

                        var point = new TripPoint
                        {
                            TripId = CurrentTrip.Id,
                            RecordedTimeStamp = DateTime.UtcNow,
                            Latitude = location.Latitude,
                            Longitude = location.Longitude,
                            Sequence = CurrentTrip.Points.Count,
                            Speed = location.ConvertSpeed(),
                            Bearing = location.Bearing

                        };
                        //If logging and speed has slowed down, then start the timer
                        //if (location.Speed < 5/* && !timer.IsRunning*/)
                        //{
                        //    ToastConfig cfg = new ToastConfig("Start timer");
                        //    cfg.Duration = TimeSpan.FromSeconds(0.8);
                        //    Device.BeginInvokeOnMainThread(() =>
                        //    {
                        //        UserDialogs.Instance.Toast(cfg);

                        //    });

                        //}


                        //If logging trip, but max time has elapsed, then stop logging
                        if ((DateTime.Now - lastLog).TotalMilliseconds > tripStopThreshold.TotalMilliseconds)
                        {
                            StopLoggingTrip(point);
                            return;
                        }

                        //If logging trip and trip is still in progress, then stop timer
                        if (/*timer.IsRunning && */location.ConvertSpeed() > 5)
                        {
                            //    ToastConfig cfg = new ToastConfig("Timer stopped and reset");
                            //    cfg.Duration = TimeSpan.FromSeconds(0.5);
                            //    Device.BeginInvokeOnMainThread(() =>
                            //    {
                            //        UserDialogs.Instance.Toast(cfg);

                            //    });

                            //restart timer
                            //timer.Restart();
                            lastLog = DateTime.Now;
                            RestartTimer();
                        }


                        if (previousCoord != null)
                        {
                            if (previousCoord.Latitude == location.Latitude && previousCoord.Longitude == location.Longitude && previousCoord.Bearing == location.Bearing)
                                return;

                        }

                        CurrentTrip.Duration = point.RecordedTimeStamp - CurrentTrip.StartLocalTimestamp;
                        //Tachograph event
                        if ((previousCoord == null || Math.Abs(previousCoord.Bearing - location.Bearing) > 15) && location.ConvertSpeed() > 10 && (DateTime.Now - tachoEventTimer).TotalMilliseconds >= TimeSpan.FromSeconds(5).TotalMilliseconds)
                        {
                            eventType = TripEventType.Tachograph;
                            tachoEventTimer = DateTime.Now;
                        }
                        else
                        {
                            //Regular position
                            if ((DateTime.Now - regularPositionTimer).TotalMilliseconds >= TimeSpan.FromSeconds(60).TotalMilliseconds)
                            {
                                eventType = TripEventType.PeriodicPosition;
                                regularPositionTimer = DateTime.Now;
                            }
                            else
                            {
                                return;
                            }

                        }

                        CurrentTrip.Points.Add(point);

                        if (previousCoord != null)
                        {
                            //Calculate distance
                            //CurrentTrip.Distance += DistanceCalculation.GeoCodeCalc.CalcDistance(previousCoord.Latitude, previousCoord.Longitude, location.Latitude, location.Longitude);
                            CurrentTrip.Distance += previousCoord.DistanceTo(location, UnitOfLength.Kilometers);
                        }

                        previousCoord = location;
                        previousPoint = point;

                        //For trip end events, store in blob cache in case this is the last event
                        CurrentTrip.EndLocalTimestamp = point.RecordedTimeStamp;
                        var tripEnd = new TripEndBlob { Trip = CurrentTrip, EventType = TripEventType.TripEnd, Point = point };
                        BlobCache.LocalMachine.InsertObject<TripEndBlob>(CurrentTrip.Id, tripEnd);

                        try
                        {
                            if (tripDataProcessor != null)
                            {
                                //Push the trip data packaged with the OBD data to the IOT Hub
                                tripDataProcessor.SendTripPointToHub(eventType, point, CurrentTrip);

                                BlobCache.LocalMachine.InsertObject<LogbookEventItem>(Guid.NewGuid().ToString(), new LogbookEventItem("Logged: " + eventType.ToString()));
                            }
                        }
                        catch (System.Exception ex)
                        {
                            BlobCache.LocalMachine.InsertObject<LogbookEventItem>(Guid.NewGuid().ToString(), new LogbookEventItem(ex.ToString()));
                            //Logger.Instance.Report(ex);
                        }

                        var timeDif = point.RecordedTimeStamp - CurrentTrip.StartLocalTimestamp;

                        //track seconds, minutes, then hours
                        if (timeDif.TotalMinutes < 1)
                            ElapsedTime = $"{timeDif.Seconds}s";
                        else if (timeDif.TotalHours < 1)
                            ElapsedTime = $"{timeDif.Minutes}m {timeDif.Seconds}s";
                        else
                            ElapsedTime = $"{(int)timeDif.TotalHours}h {timeDif.Minutes}m {timeDif.Seconds}s";


                        //AppDroid.Current.LocationService.UpdateNotification("Logged: " + coord.ToString());
                        //ToastConfig cfg = new ToastConfig("Logged: " + coord.ToString());
                        //cfg.Duration = TimeSpan.FromSeconds(0.8);
                        //this.RunOnUiThread(() =>
                        //{
                        //    UserDialogs.Instance.Toast(cfg);

                        //});
                    }
                }

                //Poll position every 60 minutes
                if (Settings.Current.LocationUpdates && loggingTrip == false && ((DateTime.Now - pollingTimer).TotalMilliseconds >= TimeSpan.FromMinutes(Settings.Current.ReportingInterval).TotalMilliseconds || !hasPooled))
                {
                    if (!hasPooled)
                        hasPooled = true;
                    SendPolledPosition(location);
                }


            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }



            Current.MainPage = MainDetailPage;
        }

        #endregion

    }
}

