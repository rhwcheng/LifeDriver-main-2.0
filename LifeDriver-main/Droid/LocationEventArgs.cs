using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Locations;

namespace StyexFleetManagement.Droid
{
    public class LocationEventArgs : LocationChangedEventArgs
    {
        public string DeviceId { get; set; }
        public Location Location { get; set; }
        public double BatteryLevel { get; set; }
        public bool IsPeriodicUpdate { get; set; }

        public LocationEventArgs(Location location) : base(location)
        {
        }

        public LocationEventArgs(string deviceId, Location location, double batteryLevel) : base(location)
        {
            this.DeviceId = deviceId;
            this.Location = location;
            this.BatteryLevel  = batteryLevel;
        }

    }
}