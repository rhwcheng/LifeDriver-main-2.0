using Android.Annotation;
using Android.App;
using Android.Content;
using Android.Locations;
using Android.OS;
using Android.Util;
using Plugin.Connectivity.Abstractions;
using StyexFleetManagement.Helpers;
using StyexFleetManagement.Models;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using System;

namespace StyexFleetManagement.Droid.Services
{
    [Service]
    public class LocationService : Service, ILocationListener, ILocationManager
    {
        public event EventHandler<LocationEventArgs> LocationChanged = delegate { };


        public event EventHandler<ProviderDisabledEventArgs> ProviderDisabled = delegate { };
        public event EventHandler<ProviderEnabledEventArgs> ProviderEnabled = delegate { };
        public event EventHandler<StatusChangedEventArgs> StatusChanged = delegate { };

        protected static string TAG = typeof(LocationService).Name;

        // Set our location manager as the system location service
        protected LocationManager LocationManager = Android.App.Application.Context.GetSystemService("location") as LocationManager;

        readonly string logTag = "LocationService";
        IBinder binder;
        private Notification.Builder notificationBuilder;

        private int notifyId;
        private NotificationManager notificationManager;
        private Android.Locations.Location lastLocation;
        private long interval;
        private int distance;
        private int angle;
        private string deviceId;
        private string locationProvider;
        private readonly TimeSpan updateInterval = TimeSpan.FromMinutes(1);

        public TripDataProcessor TripDataProcessor { get; set; }

        public LocationService()
        {
            TripDataProcessor = new TripDataProcessor();
        }
        //private async void RunUpdateLoop()
        //{
        //    await Task.Delay(updateInterval);
        //    while (true)
        //    {
        //        try
        //        {
        //            if (this.LocationManager != null)
        //            {
        //                MessagingCenter.Send<ILocationManager>((ILocationManager)this, "PeriodicUpdate");
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //        finally
        //        {
        //            await Task.Delay(updateInterval);
        //        }
        //    }
        //}

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Debug(logTag, "OnCreate called in the Location Service");
        }

        // This gets called when StartService is called in our App class
        //[Obsolete("deprecated in base class")]
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Debug(logTag, "LocationService started");

            notificationManager = (NotificationManager)GetSystemService(Context.NotificationService);

            if (notificationManager == null)
            {
                throw new ArgumentNullException(nameof(notificationManager));
            }

            var notificationChannelId = "com.aw.fleetbi";

            var chan = new NotificationChannel(notificationChannelId, "Fleet BI",
                Android.App.NotificationImportance.High);

            notificationManager.CreateNotificationChannel(chan);

            var builder = new Notification.Builder(this);

            var newIntent = new Intent(this, typeof(MainActivity));
            newIntent.PutExtra("tracking", true);
            newIntent.AddFlags(ActivityFlags.ClearTop);
            newIntent.AddFlags(ActivityFlags.SingleTop);

            notifyId = 1;
            var pendingIntent = PendingIntent.GetActivity(this, 0, newIntent, 0);
            notificationBuilder = builder.SetContentIntent(pendingIntent)
                .SetSmallIcon(Resource.Drawable.ic_explore_white_24dp)
                .SetAutoCancel(false)
                .SetTicker(AppResources.app_is_recording)
                .SetContentTitle(AppResources.app_name)
                .SetContentText(AppResources.app_is_running)
                .SetChannelId(notificationChannelId);


            //RunUpdateLoop();

            StartForeground((int)NotificationFlags.Insistent, notificationBuilder.Build());

            return StartCommandResult.Sticky;



        }

        public void UpdateNotification(string message, int notificationId)
        {
            notificationBuilder.SetContentText(message).SetSmallIcon(Resource.Drawable.ic_timer_white_24dp);

            notificationManager.Notify(notificationId, notificationBuilder.Build());
        }

        internal void CancelNotification(int notificationId)
        {
            notificationManager.Cancel(notificationId);
        }
        // This gets called once, the first time any client bind to the Service
        // and returns an instance of the LocationServiceBinder. All future clients will
        // reuse the same instance of the binder
        public override IBinder OnBind(Intent intent)
        {
            Log.Debug(logTag, "Client now bound to service");

            binder = new LocationServiceBinder(this);
            return binder;
        }

        // Handle location updates from the location manager
        public void StartLocationUpdates()
        {
            //we can set different location criteria based on requirements for our app -
            //for example, we might want to preserve power, or get extreme accuracy
            var locationCriteria = new Criteria();

            locationCriteria.Accuracy = Accuracy.NoRequirement;
            locationCriteria.PowerRequirement = Power.NoRequirement;

            // get provider: GPS, Network, etc.
            locationProvider = LocationManager.GetBestProvider(locationCriteria, true);
            Log.Debug(logTag, string.Format("You are about to get location updates via {0}", locationProvider));

            interval = 4000;
            deviceId = Settings.Current.DeviceId;

            // Get an initial fix on location
            LocationManager.RequestLocationUpdates(locationProvider, interval, 0, this);


            //Start listening for connectivity change event so that we know if connection is restablished\dropped when pushing data to the IOT Hub
            //CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;

            Log.Debug(logTag, "Now sending location updates");

            TripDataProcessor.SendBufferedDataToHub();
        }

        private async void Current_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.IsConnected)
            {
                //If connection is re-established, then kick of background thread to push buffered data to IOT Hub
                TripDataProcessor.SendBufferedDataToHub();
            }
        }



        public override void OnDestroy()
        {
            base.OnDestroy();
            Log.Debug(logTag, "Service has been terminated");

            // Stop getting updates from the location manager:
            LocationManager.RemoveUpdates(this);
        }

        #region ILocationListener implementation
        // ILocationListener is a way for the Service to subscribe for updates
        // from the System location Service

        public void OnLocationChanged(Android.Locations.Location location)
        {
            if (location != null && (lastLocation == null
                   || location.Time - lastLocation.Time >= interval
                   || distance > 0 && DistanceCalculation.GeoCodeCalc.CalcDistance(location.Latitude, location.Longitude, lastLocation.Latitude, location.Longitude, DistanceCalculation.GeoCodeCalcMeasurement.Kilometers) >= distance
                   || angle > 0 && Math.Abs(location.Bearing - lastLocation.Bearing) >= angle))
            {
                Console.WriteLine(TAG, "location new");
                lastLocation = location;
                this.LocationChanged(this, new LocationEventArgs(deviceId, location, GetBatteryLevel(this)));

                //this.LocationChanged(this, new LocationChangedEventArgs(location));

                // This should be updating every time we request new location updates
                // both when teh app is in the background, and in the foreground
                Log.Debug(logTag, String.Format("Latitude is {0}", location.Latitude));
                Log.Debug(logTag, String.Format("Longitude is {0}", location.Longitude));
                Log.Debug(logTag, String.Format("Altitude is {0}", location.Altitude));
                Log.Debug(logTag, String.Format("Speed is {0}", location.Speed));
                Log.Debug(logTag, String.Format("Accuracy is {0}", location.Accuracy));
                Log.Debug(logTag, String.Format("Bearing is {0}", location.Bearing));
            }
            else
            {
                Console.WriteLine(TAG, location != null ? "location ignored" : "location nil");
            }



        }

        [TargetApi(Value = (int)Android.OS.BuildVersionCodes.Eclair)]
        private static double GetBatteryLevel(Context context)
        {
            if (Android.OS.Build.VERSION.SdkInt > BuildVersionCodes.Eclair)
            {
                Intent batteryIntent = context.RegisterReceiver(null, new IntentFilter(Intent.ActionBatteryChanged));
                int level = batteryIntent.GetIntExtra(BatteryManager.ExtraLevel, 0);
                int scale = batteryIntent.GetIntExtra(BatteryManager.ExtraScale, 1);
                return (level * 100.0) / scale;
            }
            else
            {
                return 0;
            }
        }

        public void OnProviderDisabled(string provider)
        {
            this.ProviderDisabled(this, new ProviderDisabledEventArgs(provider));
        }

        public void OnProviderEnabled(string provider)
        {
            this.ProviderEnabled(this, new ProviderEnabledEventArgs(provider));
        }

        public void OnStatusChanged(string provider, Availability status, Bundle extras)
        {
            this.StatusChanged(this, new StatusChangedEventArgs(provider, status, extras));
        }

        #endregion

    }
}