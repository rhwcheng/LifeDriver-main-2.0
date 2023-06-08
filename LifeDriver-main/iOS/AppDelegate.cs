
using System;
using System.Threading.Tasks;
using FFImageLoading;
using FFImageLoading.Forms.Platform;
using FFImageLoading.Svg.Forms;
using Foundation;
using ImageCircle.Forms.Plugin.iOS;
using StyexFleetManagement.iOS.Services;
using StyexFleetManagement.Messages;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Statics;
using Syncfusion.SfChart.XForms.iOS.Renderers;
using Syncfusion.SfDataGrid.XForms.iOS;
using Syncfusion.SfPicker.XForms.iOS;
using Syncfusion.XForms.iOS.ProgressBar;
using UIKit;
using UserNotifications;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace StyexFleetManagement.iOS
{
    [Register ("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
        #region declarations and properties
        public static LocationManager Manager = null;
        private iOSTimerTask timerTask;
        #endregion
        public override bool FinishedLaunching (UIApplication app, NSDictionary options)
        {
            try
            {
                Rg.Plugins.Popup.Popup.Init();

                global::Xamarin.Forms.Forms.Init();
                Xamarin.FormsGoogleMaps.Init("AIzaSyD9mK6C16e0tlS1e-yoHUAP69JS6ZAbQmg");
                var ignore = typeof(SvgCachedImage);

                CachedImageRenderer.Init();

                //App.Init(new CustomMapRenderer());

                // TODO: Map

                App.ScreenWidth = UIScreen.MainScreen.Bounds.Width;
                App.ScreenHeight = UIScreen.MainScreen.Bounds.Height;

                new SfChartRenderer();
                SfChartRenderer.Init();
                SfDataGridRenderer.Init();
                SfCircularProgressBarRenderer.Init();

                ImageCircleRenderer.Init();
                SfPickerRenderer.Init();

                //FlicService.Init(); //TODO: Fix

                LoadApplication(new App());

                var thai = new System.Globalization.ThaiBuddhistCalendar();


                // Apply OS-specific color theming
                ConfigureApplicationTheming();


                MessagingCenter.Subscribe<LoginPage>(this, "LoggedIn", (vm) => LoggedIn("LoggedIn"));

                MessagingCenter.Subscribe<StartLongRunningTaskMessage>(this, "StartLongRunningTaskMessage", async message =>
                {
                    timerTask = new iOSTimerTask();
                    await timerTask.Start();
                });

                MessagingCenter.Subscribe<StopLongRunningTaskMessage>(this, "StopLongRunningTaskMessage", message =>
                {
                    timerTask.Stop();
                });

                MessagingCenter.Subscribe<SettingsContent>(this, "LocationUpdatesPreferenceChanged", message =>
                {
                    UpdateLocationManager();
                });

                // Request notification permissions from the user
                UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) => {
                    // Handle approval
                });

                return base.FinishedLaunching(app, options);
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, e.Message);
                throw;
            }
        }


        private void LoggedIn(string v)
        {
            UpdateLocationManager();
        }

        private void UpdateLocationManager()
        {
            //Begin generating location updates in the location manager

            Manager = new LocationManager();
            Manager.StartLocationUpdates();
        }

        void ConfigureApplicationTheming()
		{
			UINavigationBar.Appearance.TintColor = UIColor.White;
			UINavigationBar.Appearance.BarTintColor = Palette._001.ToUIColor();
			UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes { ForegroundColor = UIColor.White };
			UIBarButtonItem.Appearance.SetTitleTextAttributes(new UITextAttributes { TextColor = UIColor.Black }, UIControlState.Normal);

			UITabBar.Appearance.TintColor = UIColor.White;
			UITabBar.Appearance.BarTintColor = UIColor.White;
			UITabBar.Appearance.SelectedImageTintColor = Palette._003.ToUIColor();
			UITabBarItem.Appearance.SetTitleTextAttributes(new UITextAttributes { TextColor = Palette._003.ToUIColor() }, UIControlState.Selected);

			UIProgressView.Appearance.ProgressTintColor = Palette._003.ToUIColor();
		}
        

    }
}

