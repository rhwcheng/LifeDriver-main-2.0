using Android.OS;
using StyexFleetManagement.Abstractions;
using StyexFleetManagement.Droid.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(DeviceService))]

namespace StyexFleetManagement.Droid.Services
{
    public class DeviceService : IDevice
    {
        public string GetImei()
        {
            try
            {
                var telephonyManager =
                    (Android.Telephony.TelephonyManager) Android.App.Application.Context.GetSystemService(Android
                        .Content
                        .Context.TelephonyService);
#pragma warning disable CS0618 // Type or member is obsolete
                return Build.VERSION.SdkInt >= BuildVersionCodes.O
                    ? telephonyManager.GetImei(0)
                    : telephonyManager.DeviceId;
#pragma warning restore CS0618 // Type or member is obsolete
            }
            catch (Java.Lang.SecurityException)
            {
                return null;
            }
        }
    }
}