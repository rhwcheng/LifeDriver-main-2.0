//using Plugin.Geolocator;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using Xamarin.Forms;

//namespace StyexFleetManagement.Services
//{
//    public class LocationTask
//    {
//        public async Task StartListening()
//        {
//            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 10, true, new Plugin.Geolocator.Abstractions.ListenerSettings
//            {
//                ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
//                AllowBackgroundUpdates = true,
//                DeferLocationUpdates = true,
//                DeferralDistanceMeters = 1,
//                DeferralTime = TimeSpan.FromSeconds(1),
//                ListenForSignificantChanges = true,
//                PauseLocationUpdatesAutomatically = false
//            });

//            CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
//        }

//        private void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
//        {
//            Device.BeginInvokeOnMainThread(() =>
//            {
//                var location = e.Position;
//                MessagingCenter.Send<LocationUpdateMessage>(new LocationUpdateMessage(location), "LocationUpdate");
//            });
//        }
//    }
//}
