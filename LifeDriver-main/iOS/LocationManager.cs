using Acr.UserDialogs;
using Akavache;
using CoreLocation;
using Foundation;
using Newtonsoft.Json;
using StyexFleetManagement.Models;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace StyexFleetManagement.iOS
{
    public class LocationManager : ILocationManager
    {
        //nint _taskId;
        //CancellationTokenSource _cts;

        //public async Task Start()
        //{
        //    //_cts = new CancellationTokenSource();

        //    _taskId = UIApplication.SharedApplication.BeginBackgroundTask("LocationTask", OnExpiration);

        //    try
        //    {
        //        //INVOKE THE SHARED CODE
        //        var task = new LocationTask();
        //        await task.StartListening();

        //    }
        //    catch (OperationCanceledException)
        //    {
        //    }
        //    //finally
        //    //{
        //    //    if (_cts.IsCancellationRequested)
        //    //    {
        //    //        var message = new CancelledMessage();
        //    //        Device.BeginInvokeOnMainThread(
        //    //            () => MessagingCenter.Send(message, "CancelledMessage")
        //    //        );
        //    //    }
        //    //}

        //    //UIApplication.SharedApplication.EndBackgroundTask(_taskId);
        //}

        //public void Stop()
        //{
        //    _cts.Cancel();
        //}

        //void OnExpiration()
        //{
        //    _cts.Cancel();
        //}



        protected CLLocationManager locMgr;
        private bool loggingTrip;
        private Stopwatch timer;
        private List<Position> coordinates;
        private LogbookTrip currentTrip;
        private readonly TimeSpan tripStopThreshold = TimeSpan.FromMinutes(3);
        private Coordinate previousCoord;
        private readonly TimeSpan updateInterval = TimeSpan.FromMinutes(1);

        // event for the location changing
        public event EventHandler<LocationUpdatedEventArgs> LocationUpdated = delegate { };

        public LocationManager()
        {
            if (Settings.Current.LocationUpdates)
            {
                this.locMgr = new CLLocationManager();

                this.locMgr.PausesLocationUpdatesAutomatically = false;

                // iOS 8 has additional permissions requirements
                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
                {
                    locMgr.RequestAlwaysAuthorization(); // works in background
                                                         //locMgr.RequestWhenInUseAuthorization (); // only in foreground
                }

                if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
                {
                    locMgr.AllowsBackgroundLocationUpdates = true;
                }
                LocationUpdated += HandleLocationChanged;

                loggingTrip = false;
                timer = new System.Diagnostics.Stopwatch();
                Task timerTask = Task.Run(async () => TimerTask());
                coordinates = new List<Position>();
            }

        }
        //private async void RunUpdateLoop()
        //{
        //    await Task.Delay(updateInterval);
        //    while (true)
        //    {
        //        try
        //        {
        //            if (locMgr != null)
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

        // create a location manager to get system location updates to the application
        public CLLocationManager LocMgr
        {
            get { return this.locMgr; }
        }

        private Task TimerTask()
        {
            //nint taskID = UIApplication.SharedApplication.BeginBackgroundTask(() => { });


            while (true)
            {
                if (loggingTrip == true && timer.ElapsedMilliseconds > tripStopThreshold.TotalMilliseconds)
                {
                    StopLoggingTrip();
                }
                Thread.Sleep(1000 * 10);
            }

            //UIApplication.SharedApplication.EndBackgroundTask(taskID);
        }

        private void StartLoggingTrip()
        {
            ToastConfig cfg = new ToastConfig(AppResources.logging_started);
            cfg.Duration = TimeSpan.FromSeconds(0.8);
            UserDialogs.Instance.Toast(cfg);

            //Start logging trip
            loggingTrip = true;
            currentTrip = new LogbookTrip();
            currentTrip.StartLocalTimestamp = DateTimeOffset.Now;
        }

        private void StopLoggingTrip()
        {
            ToastConfig cfg = new ToastConfig(AppResources.logging_stopped);
            cfg.Duration = TimeSpan.FromSeconds(0.8);
            UserDialogs.Instance.Toast(cfg);



            //stop timer and reset
            timer.Stop();
            timer.Reset();

            //Stop logging trip and store trip details

            currentTrip.EndLocalTimestamp = DateTimeOffset.Now;


            string json = JsonConvert.SerializeObject(currentTrip, Formatting.Indented);
            currentTrip.SerializedString = json;

            BlobCache.LocalMachine.InsertObject<LogbookTrip>(currentTrip.Id.ToString(), currentTrip);


            loggingTrip = false;
        }

        public void StartLocationUpdates()
        {
            if (!Settings.Current.LocationUpdates)
            {
                return;
            }
            // We need the user's permission for our app to use the GPS in iOS. This is done either by the user accepting
            // the popover when the app is first launched, or by changing the permissions for the app in Settings

            if (CLLocationManager.LocationServicesEnabled)
            {

                LocMgr.DesiredAccuracy = 5; // sets the accuracy that we want in meters

                // Location updates are handled differently pre-iOS 6. If we want to support older versions of iOS,
                // we want to do perform this check and let our LocationManager know how to handle location updates.

                if (UIDevice.CurrentDevice.CheckSystemVersion(6, 0))
                {

                    LocMgr.LocationsUpdated += (object sender, CLLocationsUpdatedEventArgs e) =>
                    {
                        // fire our custom Location Updated event
                        this.LocationUpdated(this, new LocationUpdatedEventArgs(e.Locations[e.Locations.Length - 1]));
                    };

                }
                else
                {

                    // this won't be called on iOS 6 (deprecated). We will get a warning here when we build.
                    LocMgr.UpdatedLocation += (object sender, CLLocationUpdatedEventArgs e) =>
                    {
                        this.LocationUpdated(this, new LocationUpdatedEventArgs(e.NewLocation));
                    };
                }

                // Start our location updates
                LocMgr.StartUpdatingLocation();

                // Get some output from our manager in case of failure
                LocMgr.Failed += (object sender, NSErrorEventArgs e) =>
                {
                    Console.WriteLine(e.Error);
                };

            }
            else
            {

                //Let the user know that they need to enable LocationServices
                Console.WriteLine("Location services not enabled, please enable this in your Settings");

            }
        }

        //This will keep going in the background and the foreground
        public void HandleLocationChanged(object sender, LocationUpdatedEventArgs e)
        {
            var args = new LocationArgs { Latitude = e.Location.Coordinate.Latitude, Longitude = e.Location.Coordinate.Longitude, Speed = e.Location.Speed, Bearing = e.Location.Course, Altitude = e.Location.Altitude, /*BatteryLevel = e.BatteryLevel,*/ Time = e.Location.Timestamp.ToDateTime().Ticks, Accuracy = Convert.ToSingle(e.Location.HorizontalAccuracy) };
            MessagingCenter.Send<ILocationManager, LocationArgs>((ILocationManager)sender, "LocationUpdated", args);

            //CLLocation location = e.Location;

            //Console.WriteLine("Altitude: " + location.Altitude + " meters");
            //Console.WriteLine("Longitude: " + location.Coordinate.Longitude);
            //Console.WriteLine("Latitude: " + location.Coordinate.Latitude);
            //Console.WriteLine("Course: " + location.Course);
            //Console.WriteLine("Speed: " + location.Speed);

            //if (loggingTrip == false && location.Speed > 10)
            //{
            //    StartLoggingTrip();
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
            //    UserDialogs.Instance.Toast(cfg);

            //    //stop timer and reset
            //    timer.Stop();
            //    timer.Reset();
            //}
            //if (loggingTrip == true && location.Speed < 5 && !timer.IsRunning)
            //{
            //    ToastConfig cfg = new ToastConfig("Start timer");
            //    cfg.Duration = TimeSpan.FromSeconds(0.8);
            //    UserDialogs.Instance.Toast(cfg);

            //    timer.Start();
            //}

            //if (loggingTrip == true)
            //{
            //    //Add co-ordinate
            //    var coord = new Coordinate(location.Coordinate.Latitude, location.Coordinate.Longitude);

            //    if (previousCoord != null && previousCoord.Latitude == coord.Latitude && previousCoord.Longitude == coord.Longitude)
            //        return;

            //    if (previousCoord != null)
            //    {
            //        //Calculate distance
            //        currentTrip.Distance += DistanceCalculation.GeoCodeCalc.CalcDistance(previousCoord.Latitude, previousCoord.Longitude, coord.Latitude, coord.Longitude);
            //    }
            //    currentTrip.Coordinates.Add(coord);

            //    previousCoord = coord;

            //    ToastConfig cfg = new ToastConfig("Logged: " + coord.ToString());
            //    cfg.Duration = TimeSpan.FromSeconds(0.8);
            //    UserDialogs.Instance.Toast(cfg);
            //}

        }

    }
}
