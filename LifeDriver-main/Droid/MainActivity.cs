using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.Locations;
using Android.OS;
using Android.Widget;
using FFImageLoading.Forms.Platform;
using ImageCircle.Forms.Plugin.Droid;
using IO.Flic.Lib;
using Plugin.NFC;
using Serilog;
using Shiny;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Droid.Services;
using StyexFleetManagement.Messages;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel.Base;
using System;
using UniversalBeacon.Library;
using UniversalBeacon.Library.Core.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Log = Android.Util.Log;
using Result = Android.App.Result;

namespace StyexFleetManagement.Droid
{

    [Activity(Label = "Life Driver", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]

    public partial class MainActivity : FormsAppCompatActivity
    {
        public const string PACKAGE_NAME = "com.fleet.fleetbi";
        private int DefaultAppPoolThreads;
        private int DefaultAppPoolCompletionThreads;
        readonly string logTag = "MainActivity";

        private bool loggingTrip;
        private System.Diagnostics.Stopwatch timer;

        private System.Diagnostics.Stopwatch pollingTimer;

        private readonly TimeSpan tripStopThreshold = TimeSpan.FromMinutes(3);
        private Coordinate previousCoord;
        private AlarmManager alarmManager;

        //Traccar
        public static string KEY_DEVICE = "id";
        public static string KEY_URL = "url";
        public static string KEY_INTERVAL = "interval";
        public static string KEY_DISTANCE = "distance";
        public static string KEY_ANGLE = "angle";
        public static string KEY_PROVIDER = "provider";
        public static string KEY_STATUS = "status";
        private PendingIntent alarmIntent;

        private bool hasPooled;

        public LogbookTrip CurrentTrip { get; private set; }
        public string ElapsedTime { get; private set; }

        public static MainActivity Instance { get; set; }

        #region Lifecycle
        protected override void OnCreate(Bundle bundle)
        {
            FormsAppCompatActivity.ToolbarResource = Resource.Layout.Toolbar;

            bool isTablet = Resources.GetBoolean(Resource.Boolean.isTablet);
            if (isTablet)
                RequestedOrientation = ScreenOrientation.SensorLandscape;
            else
                RequestedOrientation = ScreenOrientation.SensorPortrait;

            base.OnCreate(bundle);
            Log.Debug(logTag, "OnCreate: Location app is becoming active");

            Xamarin.Essentials.Platform.Init(this, bundle); // add this line to your code, it may also be called: bundle

            UserDialogs.Init(() => (Activity)Forms.Context);

            Rg.Plugins.Popup.Popup.Init(this);

            CrossNFC.Init(this);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            //System.Threading.ThreadPool.GetMinThreads(out DefaultAppPoolThreads, out DefaultAppPoolCompletionThreads);
            //System.Threading.ThreadPool.SetMinThreads(100, 100);
            //System.Threading.ThreadPool.SetMaxThreads(150, 150);

            Xamarin.FormsGoogleMaps.Init(this, bundle); //Initialize GoogleMaps here

            this.ShinyOnCreate();

            //GC.KeepAlive(new SfChartRenderer());

            CachedImageRenderer.Init(true);

            var width = Resources.DisplayMetrics.WidthPixels;
            var height = Resources.DisplayMetrics.HeightPixels;
            var density = Resources.DisplayMetrics.Density;

            StyexFleetManagement.App.ScreenWidth = (width - 0.5f) / density;
            StyexFleetManagement.App.ScreenHeight = (height - 0.5f) / density;

            //loggingTrip = false;
            //timer = new System.Diagnostics.Stopwatch();
            //pollingTimer = new System.Diagnostics.Stopwatch();
            //Task timerTask = Task.Run(async () => TimerTask());
            //coordinates = new List<Xamarin.Forms.Maps.Position>();

            ImageCircleRenderer.Init();

            FlicService.Init();

            MessagingCenter.Subscribe<LoginPage>(this, "LoggedIn", (vm) => LoggedIn("LoggedIn"));
            MessagingCenter.Subscribe<SettingsContent>(this, "LocationUpdatesPreferenceChanged", message => {
                UpdateLocationManager();
            });

            MessagingCenter.Subscribe<StartLongRunningTaskMessage>(this, "StartLongRunningTaskMessage", message => {
                var intent = new Intent(this, typeof(LongRunningTaskService));
                StartService(intent);
            });

            MessagingCenter.Subscribe<StopLongRunningTaskMessage>(this, "StopLongRunningTaskMessage", message => {
                var intent = new Intent(this, typeof(LongRunningTaskService));
                StopService(intent);
            });

            //allowing the device to change the screen orientation based on the rotation
            MessagingCenter.Subscribe<ReportPage>(this, "forceLandscape", sender =>
            {
                RequestedOrientation = ScreenOrientation.Landscape;
            });

            //during page close setting back to portrait
            MessagingCenter.Subscribe<ReportPage>(this, "preventLandscape", sender =>
            {
                RequestedOrientation = ScreenOrientation.Portrait;
            });

            //MessagingCenter.Subscribe<StartLocationTaskMessage>(this, "StartlocationTaskMessage", message => {
            //    var intent = new Intent(this, typeof(LongRunningTaskService));
            //    StartService(intent);
            //});

            //MessagingCenter.Subscribe<StopLocationTaskMessage>(this, "StopLocationTaskMessage", message => {
            //    var intent = new Intent(this, typeof(LongRunningTaskService));
            //    StopService(intent);
            //});

            Serilog.Log.Logger = new LoggerConfiguration()
                              .MinimumLevel.Debug()
                              .Enrich.FromLogContext()
                              //.WriteTo.NSLog(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj} ({SourceContext}) {Exception}")
                              .CreateLogger();

            Instance = this;

            LoadApplication(new StyexFleetManagement.App());


            // TEMP

                var provider = new AndroidBluetoothPacketProvider(this);
                ViewModelLocator.Register<IBluetoothPacketProvider, AndroidBluetoothPacketProvider>(provider);

            // TEMP
        }

        private void LoggedIn(string v)
        {
            UpdateLocationManager();
        }

        private void UpdateLocationManager()
        {
            if (Settings.Current.LocationUpdates && Settings.Current.LocationUpdates)
            {
                //Location Manager
                // This event fires when the ServiceConnection lets the client (our App class) know that
                // the Service is connected. We use this event to start updating the UI with location
                // updates from the Service
                AppDroid.Current.LocationServiceConnected += (object sender, ServiceConnectedEventArgs e) =>
                {
                    Log.Debug(logTag, "ServiceConnected Event Raised");
                    // notifies us of location changes from the system
                    AppDroid.Current.LocationService.LocationChanged += HandleLocationChanged;
                    //notifies us of user changes to the location provider (ie the user disables or enables GPS)
                    AppDroid.Current.LocationService.ProviderDisabled += HandleProviderDisabled;
                    AppDroid.Current.LocationService.ProviderEnabled += HandleProviderEnabled;
                    // notifies us of the changing status of a provider (ie GPS no longer available)
                    AppDroid.Current.LocationService.StatusChanged += HandleStatusChanged;
                };

                alarmManager = (AlarmManager)this.GetSystemService(AlarmService);
                alarmIntent = PendingIntent.GetBroadcast(Android.App.Application.Context, 0, new Intent(Android.App.Application.Context, typeof(AutostartReceiver)), 0);

                // Start the location service:
                AppDroid.StartLocationService(alarmManager, alarmIntent);
            }
        }

        /*
private async Task SendPolledPosition(LocationEventArgs position)
{
   pollingTimer.Reset();
   if (!pollingTimer.IsRunning)
       pollingTimer.Start();

   var point = new TripPoint
   {
       RecordedTimeStamp = DateTime.UtcNow,
       Latitude = position.Location.Latitude,
       Longitude = position.Location.Longitude,
       Speed = position.Location.Speed,
       Bearing = position.Location.Bearing

   };
   try
   {
       if (AppDroid.Current.LocationService != null)
       {
           //Push the trip data packaged with the OBD data to the IOT Hub
           AppDroid.Current.LocationService.TripDataProcessor.SendTripPointToHub(CurrentTrip.TripId, CurrentTrip.UnitId, TripEventType.Polling, point);
       }
   }
   catch (System.Exception ex)
   {
       //Logger.Instance.Report(ex);
   }
   //StyexFleetManagement.Services.Firebase.SaveData<>();
}

private Task TimerTask()
{
   while (true)
   {
       if (loggingTrip == true && timer.ElapsedMilliseconds > tripStopThreshold.TotalMilliseconds)
       {
           StopLoggingTrip();
       }
       Thread.Sleep(1000 * 10);
   }
}*/

        //public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        //{
        //    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //    PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        //}

        //public override void OnConfigurationChanged(Configuration newConfig)
        //{
        //    base.OnConfigurationChanged(newConfig);
        //    DeviceOrientationImplementation.NotifyOrientationChange(newConfig);
        //}

        private void HandleLocationChanged(object sender, LocationEventArgs e)
        {
            if (e == null)
                return;
            var args = new LocationArgs { Latitude = e.Location.Latitude, Longitude = e.Location.Longitude, Speed = e.Location.Speed, Bearing = e.Location.Bearing, Altitude = e.Location.Altitude, BatteryLevel = e.BatteryLevel, Time = e.Location.Time, Accuracy = e.Location.Accuracy };
            MessagingCenter.Send<ILocationManager, LocationArgs>((ILocationManager)sender, "LocationUpdated", args);

            //Android.Locations.Location location = e.Location;
            //if (loggingTrip == false && location.Speed > 10)
            //{
            //    StartLoggingTrip(e);
            //    return;
            //}
            //if (loggingTrip == true && timer.ElapsedMilliseconds > tripStopThreshold.TotalMilliseconds)
            //{
            //    StopLoggingTrip();
            //    return;
            //}

            //if (loggingTrip == true && timer.IsRunning && location.Speed > 5)
            //{
            //    ToastConfig cfg = new ToastConfig("Timer stopped and reset");
            //    cfg.Duration = TimeSpan.FromSeconds(0.5);
            //    this.RunOnUiThread(() =>
            //    {
            //        UserDialogs.Instance.Toast(cfg);

            //    });

            //    //stop timer and reset
            //    timer.Stop();
            //    timer.Reset();
            //}
            //if (loggingTrip == true && location.Speed < 5 && !timer.IsRunning)
            //{
            //    ToastConfig cfg = new ToastConfig("Start timer");
            //    cfg.Duration = TimeSpan.FromSeconds(0.8);
            //    this.RunOnUiThread(() =>
            //    {
            //        UserDialogs.Instance.Toast(cfg);

            //    });

            //    timer.Start();
            //}

            //if (loggingTrip == true)
            //{
            //    var point = new TripPoint
            //    {
            //        TripId = CurrentTrip.Id,
            //        RecordedTimeStamp = DateTime.UtcNow,
            //        Latitude = e.Location.Latitude,
            //        Longitude = e.Location.Longitude,
            //        Sequence = CurrentTrip.Points.Count,
            //        Speed = e.Location.Speed,
            //        Bearing = e.Location.Bearing

            //    };

            //    //Add co-ordinate
            //    var coord = new Coordinate(e.Location.Latitude, e.Location.Longitude);

            //    if (previousCoord != null && previousCoord.Latitude == coord.Latitude && previousCoord.Longitude == coord.Longitude)
            //        return;

            //    if (previousCoord != null)
            //    {
            //        //Calculate distance
            //        CurrentTrip.Distance += DistanceCalculation.GeoCodeCalc.CalcDistance(previousCoord.Latitude, previousCoord.Longitude, coord.Latitude, coord.Longitude);
            //    }

            //    CurrentTrip.Points.Add(point);

            //    previousCoord = coord;

            //    try
            //    {
            //        if (AppDroid.Current.LocationService != null)
            //        {
            //            //Push the trip data packaged with the OBD data to the IOT Hub
            //            AppDroid.Current.LocationService.TripDataProcessor.SendTripPointToHub(CurrentTrip.TripId, CurrentTrip.UnitId, TripEventType.TachoGraph, point);
            //        }
            //    }
            //    catch (System.Exception ex)
            //    {
            //        //Logger.Instance.Report(ex);
            //    }

            //    var timeDif = point.RecordedTimeStamp - CurrentTrip.StartLocalTimestamp;

            //    //track seconds, minutes, then hours
            //    if (timeDif.TotalMinutes < 1)
            //        ElapsedTime = $"{timeDif.Seconds}s";
            //    else if (timeDif.TotalHours < 1)
            //        ElapsedTime = $"{timeDif.Minutes}m {timeDif.Seconds}s";
            //    else
            //        ElapsedTime = $"{(int)timeDif.TotalHours}h {timeDif.Minutes}m {timeDif.Seconds}s";


            //    //AppDroid.Current.LocationService.UpdateNotification("Logged: " + coord.ToString());
            //    //ToastConfig cfg = new ToastConfig("Logged: " + coord.ToString());
            //    //cfg.Duration = TimeSpan.FromSeconds(0.8);
            //    //this.RunOnUiThread(() =>
            //    //{
            //    //    UserDialogs.Instance.Toast(cfg);

            //    //});
            //}

            //if (loggingTrip == false && (pollingTimer.ElapsedMilliseconds >= 60000 || !hasPooled))
            //{
            //    SendPolledPosition(e);
            //}
        }

        //private async Task StartLoggingTrip(LocationEventArgs e)
        //{
        //    ToastConfig cfg = new ToastConfig("Logging started");
        //    cfg.Duration = TimeSpan.FromSeconds(0.8);
        //    this.RunOnUiThread(() =>
        //    {
        //        UserDialogs.Instance.Toast(cfg);

        //    });

        //    AppDroid.Current.LocationService.UpdateNotification("Recording trip");

        //    //Start logging trip
        //    loggingTrip = true;
        //    CurrentTrip = new LogbookTrip
        //    {
        //        UnitId = Settings.Current.DeviceId,
        //        Points = new ObservableRangeCollection<TripPoint>(),
        //        StartLocalTimestamp = DateTimeOffset.UtcNow
        //    };

        //    //TripLogbookService.SendTripStartEvent(CurrentTrip.StartLocalTimestamp.UtcDateTime, e.Location.Latitude, e.Location.Longitude, CurrentTrip.UnitId);
        //    var point = new TripPoint
        //    {
        //        TripId = CurrentTrip.Id,
        //        RecordedTimeStamp = DateTime.UtcNow,
        //        Latitude = e.Location.Latitude,
        //        Longitude = e.Location.Longitude,
        //        Sequence = CurrentTrip.Points.Count,
        //        Speed = e.Location.Speed,
        //        Bearing = e.Location.Bearing

        //    };
        //    try
        //    {
        //        if (AppDroid.Current.LocationService != null)
        //        {
        //            //Push the trip data packaged with the OBD data to the IOT Hub
        //            await AppDroid.Current.LocationService.TripDataProcessor.SendTripPointToHub(CurrentTrip.TripId, CurrentTrip.UnitId, TripEventType.Start, point, logbookTrip: CurrentTrip);
        //            var s = "Success";
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        //Logger.Instance.Report(ex);
        //        Console.WriteLine(ex);
        //    }
        //}

        //private void StopLoggingTrip()
        //{
        //    ToastConfig cfg = new ToastConfig("Logging stopped");
        //    cfg.Duration = TimeSpan.FromSeconds(0.8);

        //    this.RunOnUiThread(() =>
        //    {
        //        UserDialogs.Instance.Toast(cfg);

        //    });

        //    AppDroid.Current.LocationService.UpdateNotification("FleetBI is running");

        //    //stop timer and reset
        //    timer.Stop();
        //    timer.Reset();

        //    //Stop logging trip and store trip details

        //    CurrentTrip.EndLocalTimestamp = DateTimeOffset.Now.AddMinutes(-1 * tripStopThreshold.TotalMinutes);

        //    if (CurrentTrip.Coordinates?.Count < 1)
        //    {
        //        //Logger.Instance.Track("Attempt to save a trip with no points!");
        //        return;
        //    }


        //    try
        //    {
        //        if (AppDroid.Current.LocationService != null)
        //        {
        //            //Push the trip data packaged with the OBD data to the IOT Hub
        //            AppDroid.Current.LocationService.TripDataProcessor.SendTripPointToHub(CurrentTrip.TripId, CurrentTrip.UnitId, TripEventType.Stop, null, CurrentTrip);
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        //Logger.Instance.Report(ex);
        //    }


        //    //string json = JsonConvert.SerializeObject(CurrentTrip, Formatting.Indented);
        //    //CurrentTrip.SerializedString = json;

        //    BlobCache.LocalMachine.InsertObject<LogbookTrip>(CurrentTrip.Id.ToString(), CurrentTrip);

        //    StyexFleetManagement.Services.Firebase.SaveData<LogbookTrip>(CurrentTrip, "Trip");


        //    loggingTrip = false;
        //}

        public override void OnBackPressed()
        {
            // This prevents a user from being able to hit the back button and leave the login page.
            // TODO: if (!CredentialsService.IsUserLoggedIn()) return;

            //https://github.com/rotorgames/Rg.Plugins.Popup/wiki/Getting-started#android-back-button

            base.OnBackPressed();
        }

        protected override void OnPause()
        {
            Serilog.Log.Debug(logTag, "OnPause: App is moving to background");
            base.OnPause();
        }


        protected override void OnResume()
        {
            Serilog.Log.Debug(logTag, "OnResume: App is moving into foreground");
            base.OnResume();

            // Plugin NFC: Restart NFC listening on resume (needed for Android 10+) 
            CrossNFC.OnResume();

            FlicService.RegisterBroadcastReceiver();
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            // Plugin NFC: Tag Discovery Interception
            CrossNFC.OnNewIntent(intent);
        }

        protected override void OnDestroy()
        {
            Serilog.Log.Debug(logTag, "OnDestroy: App is becoming inactive");
            base.OnDestroy();

            //// Stop the location service:
            //AppDroid.StopLocationService();
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                FlicManager.GetInstance(this, new FlicManagerResultInitializedCallback(requestCode, requestCode, data));
            }
            catch (FlicAppNotInstalledException err)
            {
                Toast.MakeText(this, AppResources.flic_not_installed, ToastLength.Short).Show();
            }
        }

        #endregion

        #region Android Location Service methods

        public void HandleProviderDisabled(object sender, ProviderDisabledEventArgs e)
        {
            Serilog.Log.Debug(logTag, "Location provider disabled event raised");
        }

        public void HandleProviderEnabled(object sender, ProviderEnabledEventArgs e)
        {
            Serilog.Log.Debug(logTag, "Location provider enabled event raised");
        }

        public void HandleStatusChanged(object sender, StatusChangedEventArgs e)
        {
            Serilog.Log.Debug(logTag, "Location status changed, event raised");
        }

        #endregion
    }
}

