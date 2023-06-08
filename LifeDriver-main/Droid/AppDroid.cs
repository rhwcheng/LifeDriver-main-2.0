using Android.App;
using Android.Content;
using Android.Util;
using StyexFleetManagement.Droid.Services;
using StyexFleetManagement.Models;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel.Base;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StyexFleetManagement.Droid
{
    public class AppDroid
    {
        // events
        public event EventHandler<ServiceConnectedEventArgs> LocationServiceConnected = delegate { };

        // declarations
        protected readonly string logTag = "LocationManager";
        protected static LocationServiceConnection locationServiceConnection;

        // properties

        public static AppDroid Current => current;
        private static AppDroid current;

        public LocationService LocationService
        {
            get
            {
                if (locationServiceConnection.Binder == null)
                    throw new Exception("Service not bound yet");
                // note that we use the ServiceConnection to get the Binder, and the Binder to get the Service here
                return locationServiceConnection.Binder.Service;
            }
        }

        #region Application context

        static AppDroid()
        {
            current = new AppDroid();
        }

        protected AppDroid()
        {
            if (Settings.Current.LocationUpdates)
            {

                // create a new service connection so we can get a binder to the service
                locationServiceConnection = new LocationServiceConnection(null);

                // this event will fire when the Service connectin in the OnServiceConnected call 
                locationServiceConnection.ServiceConnected += (object sender, ServiceConnectedEventArgs e) =>
                {

                    Log.Debug(logTag, "Service Connected");
                    // we will use this event to notify MainActivity when to start updating the UI
                    this.LocationServiceConnected(this, e);
                };
            }
        }

        public static async void StartLocationService(AlarmManager alarmManager, PendingIntent alarmIntent)
        {
            try
            {
                var permissionService = ViewModelLocator.Resolve<IPermissionsService>();
                var hasLocationPermission = await permissionService.GetLocationPermissionAsync();

                if (!hasLocationPermission)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Acr.UserDialogs.UserDialogs.Instance.Alert(
                            "Please ensure that geolocation is enabled and permissions are allowed for Fleet BI to monitor trips.",
                        "Geolocation Disabled", "OK");

                    });
                }

                if (hasLocationPermission)
                {
                    // Starting a service like this is blocking, so we want to do it on a background thread
                    new Task(() =>
                    {

                            // Start our main service
                            Log.Debug("LocationManager", "Calling StartService");
                        Android.App.Application.Context.StartService(new Intent(Android.App.Application.Context, typeof(LocationService)));

                        alarmManager.SetInexactRepeating(AlarmType.ElapsedRealtimeWakeup,
                        15000, 15000, alarmIntent);

                            // bind our service (Android goes and finds the running service by type, and puts a reference
                            // on the binder to that service)
                            // The Intent tells the OS where to find our Service (the Context) and the Type of Service
                            // we're looking for (LocationService)
                            Intent locationServiceIntent = new Intent(Android.App.Application.Context, typeof(LocationService));
                        Log.Debug("LocationManager", "Calling service binding");

                            // Finally, we can bind to the Service using our Intent and the ServiceConnection we
                            // created in a previous step.
                            Android.App.Application.Context.BindService(locationServiceIntent, locationServiceConnection, Bind.AutoCreate);
                    }).Start();

                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, ex.Message);
                //Xamarin.Insights.Report(ex);
            }
        }

        public static void StopLocationService()
        {
            // Check for nulls in case StartLocationService task has not yet completed.
            Log.Debug("LocationManager", "StopLocationService");

            // Unbind from the LocationService; otherwise, StopSelf (below) will not work:
            if (locationServiceConnection != null)
            {
                Log.Debug("LocationManager", "Unbinding from LocationService");
                Android.App.Application.Context.UnbindService(locationServiceConnection);
            }

            // Stop the LocationService:
            if (Current.LocationService != null)
            {
                Log.Debug("LocationManager", "Stopping the LocationService");
                Current.LocationService.StopSelf();
            }
        }


        #endregion

    }
}