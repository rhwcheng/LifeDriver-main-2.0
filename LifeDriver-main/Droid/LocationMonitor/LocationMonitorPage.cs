using System;
using Android.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

//[assembly: ExportRenderer(typeof(LocationMonitorPage), typeof(StyexFleetManagement.Droid.LocationMonitorPage))]
namespace StyexFleetManagement.Droid.LocationMonitor
{
    public class LocationMonitorPage : PageRenderer
    {
        readonly string logTag = "LocationMonitorPage";
        private Activity activity;
        private Android.Views.View view;

        //Labels
        TextView latText;
        TextView longText;
        TextView altText;
        TextView speedText;
        TextView bearText;
        TextView accText;

        protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
            {
                return;
            }

            try
            {
                SetupUserInterface();
                SetupEventHandlers();
                AddView(view);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(@"          ERROR: ", ex.Message);
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            view.Measure(msw, msh);
            view.Layout(0, 0, r - l, b - t);
        }

        void SetupUserInterface()
        {
            activity = this.Context as Activity;
            view = activity.LayoutInflater.Inflate(Resource.Layout.LocationMonitor, this, false);

            latText = view.FindViewById<TextView>(Resource.Id.lat);
            longText = view.FindViewById<TextView>(Resource.Id.longx);
            altText = view.FindViewById<TextView>(Resource.Id.alt);
            speedText = view.FindViewById<TextView>(Resource.Id.speed);
            bearText = view.FindViewById<TextView>(Resource.Id.bear);
            accText = view.FindViewById<TextView>(Resource.Id.acc);

            altText.Text = "altitude";
            speedText.Text = "speed";
            bearText.Text = "bearing";
            accText.Text = "accuracy";

        }


        private void SetupEventHandlers()
        {
            // notifies us of location changes from the system
            AppDroid.Current.LocationService.LocationChanged += HandleLocationChanged;

        }

        ///<summary>
        /// Updates UI with location data
        /// </summary>
        public void HandleLocationChanged(object sender, LocationEventArgs e)
        {
            Android.Locations.Location location = e.Location;
            Log.Debug(logTag, "Foreground updating");
            latText.Text = $"Latitude: {location.Latitude}";
            longText.Text = $"Longitude: {location.Longitude}";
            altText.Text = $"Altitude: {location.Altitude}";
            speedText.Text = $"Speed: {location.Speed}";
            accText.Text = $"Accuracy: {location.Accuracy}";
            bearText.Text = $"Bearing: {location.Bearing}";
        }
    }
}