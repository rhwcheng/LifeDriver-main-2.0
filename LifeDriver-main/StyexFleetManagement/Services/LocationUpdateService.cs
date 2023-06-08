using StyexFleetManagement.Resx;
using StyexFleetManagement.ViewModel.Base;
using StyexFleetManagement.Salus.Services;
using System.Linq;
using System.Threading.Tasks;
using StyexFleetManagement.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using EventType = StyexFleetManagement.Salus.Enums.EventType;
using Location = Xamarin.Essentials.Location;

namespace StyexFleetManagement.Services
{
    public class LocationUpdateService : ILocationUpdateService
    {
        public async Task GetLocationAndSendEvent(EventType eventType,
            bool showDialog = false)
        {
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Internet)
            {
                if (showDialog)
                {
                    App.ShowLoading(true, AppResources.sending_event);
                }

                var location = await Geolocation.GetLocationAsync();

                var address = await GetAddressAsync(location);

                switch (eventType)
                {
                    case EventType.AmberAlert:
                    case EventType.GuardianAngel:
                    case EventType.Sos:
                        await SendEventAsync(location, address, eventType);

                        Device.BeginInvokeOnMainThread(() =>
                            Acr.UserDialogs.UserDialogs.Instance.Toast(AppResources.sos_event_sent));
                        break;

                    case EventType.CheckIn:
                        await SendEventAsync(location, address, eventType);
                        Device.BeginInvokeOnMainThread(() =>
                            Acr.UserDialogs.UserDialogs.Instance.Toast(AppResources.checkin_event_sent));
                        break;
                    case EventType.CheckOut:
                        await SendEventAsync(location, address, eventType);
                        Device.BeginInvokeOnMainThread(() =>
                            Acr.UserDialogs.UserDialogs.Instance.Toast(AppResources.checkout_event_sent));
                        break;
                }

                if (showDialog)
                {
                    App.ShowLoading(false);
                }
            }
            else
            {
                if (showDialog)
                {
                    Device.BeginInvokeOnMainThread(() =>
                        Acr.UserDialogs.UserDialogs.Instance.Toast(AppResources.not_connected_to_internet));
                }
            }
        }

        private static async Task<string> GetAddressAsync(Location location)
        {
            var placeMark = (await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude))
                .FirstOrDefault();

            return placeMark != null
                ? $"{placeMark.SubThoroughfare} {placeMark.Thoroughfare}, {placeMark.Locality}, {placeMark.AdminArea}, {placeMark.PostalCode}, {placeMark.CountryName}"
                : string.Empty;
        }


        private static async Task SendCheckInEvent(Location location, string address, EventType eventType)
        {
            var eventService = ViewModelLocator.Resolve<IEventService>();

            await eventService.LogEventAsync(Settings.Current.SalusUser.DeviceId, eventType, location.Latitude, location.Longitude,
                address); //TODO: DEVICE ID

            Device.BeginInvokeOnMainThread(() =>
                Acr.UserDialogs.UserDialogs.Instance.Toast(AppResources.sos_event_sent));
        }

        private static async Task SendEventAsync(Location location, string address, EventType eventType)
        {
            var eventService = ViewModelLocator.Resolve<IEventService>();

            await eventService.LogEventAsync(Settings.Current.SalusUser.DeviceId, eventType, location.Latitude, location.Longitude,
                address);
        }
    }
}