using System;
using Android.App;
using Android.Content;
using Android.Support.V4.Content;
using Android.Widget;
using IO.Flic.Lib;
using Java.Lang;
using StyexFleetManagement.Droid.Services;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel.Base;
using StyexFleetManagement.Salus.Enums;
using Xamarin.Forms;
using Exception = System.Exception;

[assembly: Dependency(typeof(FlicService))]

namespace StyexFleetManagement.Droid.Services
{
    public class FlicService : IFlicService
    {
        private const string FlicAppId = "9fade055-7a1d-4f51-b498-677ed1c77016";
        private const string FlicAppSecret = "ddca8f8c-27a2-4094-99e6-9e9a8c1a1bed";
        private const string FlicAppName = "AvatarApp";

        public static void Init()
        {
            FlicManager.SetAppCredentials(FlicAppId, FlicAppSecret, FlicAppName);
        }

        public void InitializeFlic()
        {
            try
            {
                FlicManager.GetInstance(Android.App.Application.Context, new FlicManagerGetInstanceInitializedCallback());
            }
            catch (FlicAppNotInstalledException err)
            {
                Toast.MakeText(Android.App.Application.Context, AppResources.flic_not_installed, ToastLength.Short)
                    .Show();
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, e.Message);
            }
        }

        public static void RegisterBroadcastReceiver()
        {
            LocalBroadcastManager.GetInstance(Android.App.Application.Context).RegisterReceiver(new FlicBroadcastReceiver(), new IntentFilter(Salus.Constants.FlicEvent));
        }
    }

    internal class FlicManagerGetInstanceInitializedCallback : Java.Lang.Object, IFlicManagerInitializedCallback
    {
        public void OnInitialized(FlicManager manager)
        {
            manager.InitiateGrabButton(MainActivity.Instance);
        }
    }

    internal class FlicManagerResultInitializedCallback : Java.Lang.Object, IFlicManagerInitializedCallback
    {
        private readonly int _requestCode;
        private readonly int _resultCode;
        private readonly Intent _data;

        public FlicManagerResultInitializedCallback(int requestCode, int resultCode, Intent data)
        {
            _requestCode = requestCode;
            _resultCode = resultCode;
            _data = data;
        }

        public void OnInitialized(FlicManager manager)
        {
            var button = manager.CompleteGrabButton(_requestCode, _resultCode, _data);
            if (button == null) return;

            button.RegisterListenForBroadcast(FlicBroadcastReceiverFlags.ClickOrDoubleClick | FlicBroadcastReceiverFlags.Removed);
            Toast.MakeText(Android.App.Application.Context, string.Format(AppResources.flic_not_installed, button.Name), ToastLength.Short)
                .Show();
        }
    }

    [BroadcastReceiver(Enabled = true)]
    internal class FlicBroadcastReceiver : BroadcastReceiver
    {

        public override async void OnReceive(Context context, Intent intent)
        {
            LocalBroadcastManager.GetInstance(Android.App.Application.Context).UnregisterReceiver(this);
            await ViewModelLocator.Resolve<ILocationUpdateService>().GetLocationAndSendEvent(EventType.Sos);
            FlicService.RegisterBroadcastReceiver();
        }
    }
}