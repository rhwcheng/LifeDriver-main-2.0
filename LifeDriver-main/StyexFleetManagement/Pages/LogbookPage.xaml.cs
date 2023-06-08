using MoreLinq;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Models;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.Statics;
using StyexFleetManagement.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using StyexFleetManagement.Pages.AppSettings;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
    public partial class LogbookPage : ContentPage
    {
        private ObservableCollection<GroupedTripModel> grouped { get; set; }
        private DateTime startDate;
        private DateTime endDate;
        private List<Trip> tripList;

        public LogbookPage()
        {
            InitializeComponent();
            grouped = new ObservableCollection<GroupedTripModel>();
            Title = AppResources.trip_logbook_title_short;

            tripLog.IsPullToRefreshEnabled = true;
            tripLog.RefreshCommand = RefreshCommand;

            var now = DateTime.Now;
            startDate = new DateTime(now.AddDays(-7).Year, now.AddDays(-7).Month, now.AddDays(-7).Day);
            endDate = now;
            frame.BackgroundColor = Palette.MainAccent.MultiplyAlpha(0.8);


            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });
            ToolbarItems.Add(new ToolbarItem
            {
                Order = ToolbarItemOrder.Secondary,
                Text = "Status",
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(PersonalLogbookLogPage)))
            });

            SetupAdminNote();

            UpdateLocation();
            GetTripData();

        }

        private void SetupAdminNote()
        {
            var browser = new ExtendedWebView();
            browser.HorizontalOptions = LayoutOptions.FillAndExpand;
            browser.VerticalOptions = LayoutOptions.FillAndExpand;
            browser.HeightRequest = 20;

            var source = new HtmlWebViewSource();
            var text = "<html>" +
                    "<body  style=\"text-align: justify; height: 100%\">" +
                    AppResources.personal_logbook_location_message + "</br></br>" +
                    $"<strong>{AppResources.personal_logbook_phone_registration_title}</strong>" + "</br></br>" +
                    AppResources.personal_logbook_phone_registration_steps + "</br></br>" +
                    $"<small>{AppResources.personal_logbook_phone_note}</small>" +
                    "</body>" +
                    "</html>";
            source.Html = text;
            browser.Source = source;
            webViewLayout.Children.Add(browser);
        }

        async Task GetTripData()
        {
            tripLog.ItemsSource = null;
            try
            {
                using (var scope = new ActivityIndicatorScope(syncIndicator, true))
                {
                    tripList = await TripsAPI.GetTripsWithStats(Settings.Current.DeviceId, startDate, endDate, true);

                    RefreshTripList();
                    /*var groupedData =
                        tripList.Items.Where(p => p.Distance > 0.1)
                             .OrderByDescending(p => p.EndLocalTimestamp)
                             .GroupBy(p => p.EndLocalTimestamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.MonthDayPattern))
                             .Select(p => new ObservableGroupCollection<string, VehicleTrip>(p))
                             .ToList();

                    tripsListView.ItemsSource = groupedData;

                    tripsListView.BindingContext = new ObservableCollection<ObservableGroupCollection<string, VehicleTrip>>(groupedData);

                    tripsListView.GroupDisplayBinding = new Binding("Key");*/
                }
            }
            catch (NullReferenceException e)
            {
                
                await DisplayAlert(AppResources.error_label, AppResources.error_trip_information, AppResources.button_ok);
            }

        }
        private async Task UpdateLocation()
        {
            try
            {
                var lastLocationResponse = await EventAPI.GetEventsById(Settings.Current.DeviceId, (int)TripEventType.PolledPosition, startDate, endDate.AddHours(1), isUnitId: true);

                if (lastLocationResponse == null || lastLocationResponse.Count == 0)
                {
                    lastPositionLabel.Text = "N/A";
                    return;
                }

                var lastLocation = lastLocationResponse.OrderByDescending(x => x.LocalTimestamp).FirstOrDefault();

                if (lastLocation != null)
                    lastPositionLabel.Text =
                        $"{lastLocation.Position[0].ToString()}, {lastLocation.Position[1].ToString()}";

                try
                {
                    var latitude = lastLocation.Position[1];
                    var longitude = lastLocation.Position[0];

                    var placemarks = await Geocoding.GetPlacemarksAsync(latitude, longitude);
                    var placemark = placemarks?.FirstOrDefault();
                    if (placemark != null)
                        lastAddressLabel.Text =
                            $"{placemark.SubThoroughfare} {placemark.Thoroughfare}, {placemark.SubLocality}";
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    Serilog.Log.Debug(fnsEx.Message);
                }
                catch (Exception ex)
                {
                    Serilog.Log.Debug(ex.Message);
                }

            }
            catch (NullReferenceException e)
            {
                
            }

        }

        public Command RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    tripLog.IsRefreshing = true;
                    endDate = DateTime.Now;
                    await UpdateLocation();
                    await GetTripData();

                    tripLog.IsRefreshing = false;
                });
            }
        }

        private async Task RefreshTripList()
        {
            noRecordsLabel.IsVisible = false;
            tripLog.IsVisible = true;
            using (var scope = new ActivityIndicatorScope(syncIndicator, true))
            {
                if (tripList == null)
                    tripList = await TripsAPI.GetTripsWithStats(Settings.Current.DeviceId, startDate, endDate);

                if (tripList == null || tripList.Count == 0)
                {
                    noRecordsLabel.IsVisible = true;
                    tripLog.IsVisible = false;
                    return;
                }

                tripLog.ItemsSource = null;

                var earliestTrip = tripList.MinBy(x => x.StartLocalTimestamp);
                var latestTrip = tripList.MaxBy(x => x.EndLocalTimestamp);


                var groupedData =
                            tripList.Where(p => p.Distance > 0.1)
                                 .OrderByDescending(p => p.EndLocalTimestamp)
                                 .GroupBy(p => p.EndLocalTimestamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.MonthDayPattern));

                foreach (var group in groupedData)
                {
                    var dayGroup = new GroupedTripModel { Date = group.Key };
                    var trips = group.ToList();
                    foreach (var trip in trips)
                    {
                        var model = new TripModel
                        {
                            EndLocalTimestamp = trip.EndLocalTimestamp,
                            Distance = trip.Distance,
                            EndLocation = trip.EndLocation,
                            Id = trip.Id,
                            StartLocation = trip.StartLocation,
                            StartLocalTimestamp = trip.StartLocalTimestamp,
                            UnitId = trip.UnitId
                        };

                        dayGroup.Add(model);
                    }
                    grouped.Add(dayGroup);
                }

                if (grouped.Count > 0)
                {
                    noRecordsLabel.IsVisible = false;
                }
                else
                    noRecordsLabel.IsVisible = true;

                Device.BeginInvokeOnMainThread(() =>
                {
                    tripLog.ItemsSource = grouped;

                });


            }

        }
    }
}