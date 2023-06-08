using System;
using Android.Content;
using Android.Support.V4.Content;
using Android.Preferences;
using StyexFleetManagement.Droid.Services;

namespace StyexFleetManagement.Droid
{
    public class AutostartReceiver : WakefulBroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            //ISharedPreferences sharedPreferences = PreferenceManager.GetDefaultSharedPreferences(context);
            //if (sharedPreferences.GetBoolean(MainActivity.KEY_STATUS, false))
            //{
            //    StartWakefulService(context, new Intent(context, typeof(LocationService)));
            //}
            StartWakefulService(context, new Intent(context, typeof(LocationService)));
        }
    }
}
        
