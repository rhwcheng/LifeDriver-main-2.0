using MoreLinq;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Models;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.Statics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using StyexFleetManagement.Pages.AppSettings;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
    public class LocationMonitorPage : ContentPage
    {
        private ListView tripLog;
        private new ObservableCollection<ObservableGroupCollection<string, LogbookTrip>> groupedTripList;

        private List<Trip> tripList;
        private StackLayout mainStack;
        private Label noRecordsLabel;
        private ActivityIndicator syncIndicator;
        private DateTime startDate;
        private DateTime endDate;
        private Label tripsLabel;
        private StackLayout tripsLabelStack;
        private Label lastPositionLabel;
        private Label lastAddressLabel;

        public LocationMonitorPage()
        {
            var now = DateTime.Now;
            startDate = new DateTime(now.AddDays(-7).Year, now.AddDays(-7).Month, now.AddDays(-7).Day);
            endDate = now;

            tripList = new List<Trip>();
            groupedTripList = new ObservableCollection<ObservableGroupCollection<string, LogbookTrip>>();

            tripLog = new ListView { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, IsPullToRefreshEnabled = true, RefreshCommand = RefreshCommand};
            
            this.Title = AppResources.trip_logbook_title;
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


            SetupUI();
            GetTripData();
            //PopulateListview();


            //locationTask = StartListening();

        }
        //async Task StartListening()
        //{
        //    await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(5), 10, true, new Plugin.Geolocator.Abstractions.ListenerSettings
        //    {
        //        ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
        //        AllowBackgroundUpdates = true,
        //        DeferLocationUpdates = true,
        //        DeferralDistanceMeters = 1,
        //        DeferralTime = TimeSpan.FromSeconds(1),
        //        ListenForSignificantChanges = true,
        //        PauseLocationUpdatesAutomatically = false
        //    });

        //    CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
        //}
        //private void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        //{
        //    Device.BeginInvokeOnMainThread(() =>
        //    {
        //        var test = e.Position;
        //        listenLabel.Text = "Full: Lat: " + test.Latitude.ToString() + " Long: " + test.Longitude.ToString();
        //        listenLabel.Text += "\n" + $"Time: {test.Timestamp.ToString()}";
        //        listenLabel.Text += "\n" + $"Heading: {test.Heading.ToString()}";
        //        listenLabel.Text += "\n" + $"Speed: {test.Speed.ToString()}";
        //        listenLabel.Text += "\n" + $"Accuracy: {test.Accuracy.ToString()}";
        //        listenLabel.Text += "\n" + $"Altitude: {test.Altitude.ToString()}";
        //        listenLabel.Text += "\n" + $"AltitudeAccuracy: {test.AltitudeAccuracy.ToString()}";
        //    });
        //}
        //private async Task MonitorLocation()
        //{
        //    while (true)
        //    {
        //        try
        //        {
        //            var locator = CrossGeolocator.Current;
        //            locator.DesiredAccuracy = 50;

        //            var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(10));
        //            if (position == null)
        //                return;

        //            Debug.WriteLine(Constants.LOG_TAG+"Position Status: {0}", position.Timestamp);
        //            Debug.WriteLine(Constants.LOG_TAG + "Position Latitude: {0}", position.Latitude);
        //            Debug.WriteLine(Constants.LOG_TAG + "Position Longitude: {0}", position.Longitude);
        //        }
        //        catch (Exception ex)
        //        {
        //            Debug.WriteLine(Constants.LOG_TAG + "Unable to get location, may need to increase timeout: " + ex);
        //        }
        //    }
        //}
        async void GetTripData()
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
        //private async void PopulateListview()
        //{

        //    //var allTrips = await BlobCache.LocalMachine.GetAllObjects<LogbookTrip>();
        //    var username = DependencyService.Get<ICredentialsService>().UserName;
        //    var tripsResponse = await StyexFleetManagement.Services.Firebase.GetData<byte[]>(username);

        //    if (tripsResponse == null || tripsResponse.Count == 0)
        //        return;
        //    var allTrips = new System.Collections.Generic.List<LogbookTrip>();
        //    var points = new System.Collections.Generic.List<EventHeader>();
        //    foreach (var trip in tripsResponse)
        //    {
                
        //        using (var stream = new MemoryStream(trip.Object))
        //        {
        //            try
        //            {
        //                var msgOut = (TripStartup)Serializer.Deserialize<TripStartup>(stream);
        //                points.Add(msgOut.Header);
        //            }
        //            catch (Exception e)
        //            {
        //                try
        //                {
        //                    var msgOut1 = Serializer.Deserialize<TachographData>(stream);
        //                    points.Add(msgOut1.Header);
        //                }
        //                catch (Exception es)
        //                {
        //                    try
        //                    {
        //                        var msgOut2 = Serializer.Deserialize<PeriodicPosition>(stream);
        //                        points.Add(msgOut2.Header);
        //                    }
        //                    catch (Exception ess)
        //                    {
        //                        try
        //                        {
        //                            var msgOut3 = Serializer.Deserialize<TripShutdown>(stream);
        //                            points.Add(msgOut3.Header);
        //                        }
        //                        catch (Exception esss)
        //                        {
        //                            Debug.WriteLine(e);
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        //object processedTrip = new object();
        //        //using (var stream = new MemoryStream(trip.Object))
        //        //{
        //        //    switch ((TripEventType)Convert.ToInt32(msgOut.TemplateId))
        //        //    {
        //        //        case (TripEventType.TripStart):
        //        //            processedTrip = Serializer.Deserialize<TripStartup>(stream);
        //        //            break;
        //        //        case (TripEventType.TripEnd):
        //        //            processedTrip = Serializer.Deserialize<TripShutdown>(stream);
        //        //            break;
        //        //        case (TripEventType.Tachograph):
        //        //            processedTrip = Serializer.Deserialize<TachographData>(stream);
        //        //            break;
        //        //        case (TripEventType.PeriodicPosition):
        //        //            processedTrip = Serializer.Deserialize<PeriodicPosition>(stream);
        //        //            break;
        //        //    }
        //        //}
        //        //points.Add(processedTrip);


        //    }


        //    points = points.OrderBy(x => x.UtcTimestampSeconds).ToList();

        //    EventHeader previousPoint = new EventHeader();
        //    LogbookTrip newTrip = new LogbookTrip();
        //    int count = 0;
        //    foreach (var point in points)
        //    {
        //        try
        //        {
        //            count += 1;
        //            if (previousPoint.Latitude == 0 || point.UtcTimestampSeconds - previousPoint.UtcTimestampSeconds > TimeSpan.FromMinutes(5).TotalSeconds)
        //            {
        //                if (newTrip != null && newTrip.Coordinates != null && newTrip.Coordinates.Count > 0)
        //                {
        //                    newTrip.EndLocalTimestamp = UnixTimeStampToDateTime(Convert.ToDouble(previousPoint.UtcTimestampSeconds));
        //                    allTrips.Add(newTrip);
        //                }

        //                newTrip = new LogbookTrip { StartLocalTimestamp = UnixTimeStampToDateTime(Convert.ToDouble(point.UtcTimestampSeconds)) };
        //                newTrip.Coordinates.Add(new Coordinate(point.Latitude, point.Longitude));
        //            }
        //            else
        //            {
        //                if (newTrip != default(LogbookTrip))
        //                {

        //                    newTrip.Coordinates.Add(new Coordinate(point.Latitude, point.Longitude));
        //                }
        //            }
        //            previousPoint = point;
        //            if (count == points.Count)
        //            {

        //                newTrip.EndLocalTimestamp = UnixTimeStampToDateTime(Convert.ToDouble(previousPoint.UtcTimestampSeconds));
        //                allTrips.Add(newTrip);
        //            }

        //        }
        //        catch (Exception e)
        //        {
        //            Debug.WriteLine(e);
        //        }
        //    }



        //    if (allTrips != null && allTrips.Count() > 0)
        //    {
        //        allTrips = allTrips.OrderByDescending(x => x.StartLocalTimestamp).ToList();
        //        var locator = CrossGeolocator.Current;
        //        foreach (var trip in allTrips)
        //        {
        //            if (trip.Coordinates == null || trip.Coordinates.Count == 0)
        //                continue;
        //            //try
        //            //{
        //            //    //Start
        //            //    var position = new Plugin.Geolocator.Abstractions.Position { Latitude = trip.Coordinates.First().Latitude, Longitude = trip.Coordinates.First().Longitude };
        //            //    var addresses = await locator.GetAddressesForPositionAsync(position);
        //            //    var address = addresses.FirstOrDefault();
        //            //    trip.StartLocation = address.Thoroughfare;

        //            //    //End
        //            //    var positionEnd = new Plugin.Geolocator.Abstractions.Position { Latitude = trip.Coordinates.Last().Latitude, Longitude = trip.Coordinates.Last().Longitude };
        //            //    var addressesEnd = await locator.GetAddressesForPositionAsync(positionEnd);
        //            //    var addressEnd = addressesEnd.FirstOrDefault();
        //            //    trip.EndLocation = addressEnd.Thoroughfare;


        //            //}
        //            //catch (Exception ex)
        //            //{
        //            //    Debug.WriteLine("Unable to get address: " + ex);
        //            //}

        //            //Get map route image
        //            var tripPositions = new List<List<float>>();

        //            foreach (var coord in trip.Coordinates)
        //            {
        //                tripPositions.Add(new List<float> { Convert.ToSingle(coord.Longitude), Convert.ToSingle(coord.Latitude) });
        //            }
        //            var url = TripsAPI.GetStaticMapImageUrlForRoute(tripPositions);
        //            trip.Url = url;

        //            //var stack = new StackLayout();
        //            //stack.Children.Add(new Label { Text = trip.StartLocation });
        //            //stack.Children.Add(new Label { Text = trip.EndLocation });




        //            ////stack.Children.Add(previewImage);

        //            //mainStack.Children.Add(stack);

        //            //AddTripCell(trip);
        //            tripList.Add(trip);
        //        }
        //        //tripLog.ItemsSource = tripList;
        //        var groupedList = tripList.GroupBy(p => p.EndLocalTimestamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.MonthDayPattern))
        //                         .Select(p => new ObservableGroupCollection<string, LogbookTrip>(p));

        //        foreach (ObservableGroupCollection<string, LogbookTrip> group in groupedList)
        //        {
        //            groupedTripList.Add(group);
        //        }

        //    }
        //}
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        //private void AddTripCell(LogbookTrip trip)
        //{
        //    var cellWrapper = new StackLayout { Spacing = 0, Padding = 0, VerticalOptions = LayoutOptions.FillAndExpand };
        //    var startHorizontalLayout = new StackLayout();
        //    var endHorizontalLayout = new StackLayout();
        //    var lowerLayout = new StackLayout();

        //    var firstSeperator = new BoxView { HeightRequest = 0.8, BackgroundColor = Color.Gray, HorizontalOptions = LayoutOptions.FillAndExpand };
        //    var secondSeperator = new BoxView { HeightRequest = 0.8, BackgroundColor = Color.Gray, HorizontalOptions = LayoutOptions.FillAndExpand };

        //    var endTimeLabel = new Label { TextColor = Color.FromHex("#585858"), Text = trip.EndLocalTimestamp.ToString("HH:mm tt") };
        //    var startTimeLabel = new Label { TextColor = Color.FromHex("#585858"), Text = trip.StartLocalTimestamp.ToString("HH:mm tt") };
        //    var distanceLabel = new Label { FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#787E7E"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Start};
        //    var durationLabel = new Label { FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#787E7E"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Start, Text = FormatHelper.ToShortForm(trip.EndLocalTimestamp-trip.StartLocalTimestamp) };

        //    double distance = Math.Round(trip.Distance, 2);
        //    distanceLabel.FormattedText = FormatHelper.FormatDistance(distance.ToString());

        //    //Get map route image
        //    var tripPositions = new List<List<float>>();

        //    foreach (var coord in trip.Coordinates)
        //    {
        //        tripPositions.Add(new List<float> { Convert.ToSingle(coord.Longitude), Convert.ToSingle(coord.Latitude) });
        //    }
        //    //Image
        //    var previewImage = new CachedImage()
        //    {
        //        HorizontalOptions = LayoutOptions.Center,
        //        Margin = 0,
        //        Aspect = Aspect.AspectFill,
        //        CacheDuration = TimeSpan.FromDays(90),
        //        DownsampleToViewSize = true,
        //        RetryCount = 3,
        //        RetryDelay = 250,
        //        TransparencyEnabled = false,
        //        //LoadingPlaceholder = "ic_loading.png",
        //        //ErrorPlaceholder = "error.png"
        //    };


        //    if (Device.Idiom == TargetIdiom.Phone)
        //    {
        //        previewImage.WidthRequest = (Math.Min(App.ScreenWidth, App.ScreenHeight) * 0.9) - 38;
        //    }
        //    else
        //    {
        //        previewImage.WidthRequest = (Math.Min(App.ScreenWidth, App.ScreenHeight) / 2) - 38;
        //    }
        //    previewImage.HeightRequest = previewImage.WidthRequest / 2;

        //    var url = TripsAPI.GetStaticMapImageUrlForRoute(tripPositions);

        //    previewImage.Source = url;


        //    var timeStack = new StackLayout
        //    {
        //        HorizontalOptions = LayoutOptions.CenterAndExpand,
        //        Orientation = StackOrientation.Horizontal,
        //        Padding = 4,
        //        Children =
        //        {
        //            startTimeLabel,
        //            new Label { Text = AppResources.to_label.ToLower(), TextColor=Color.Gray, FontSize = startTimeLabel.FontSize-1},
        //            endTimeLabel
        //        }
        //    };

        //    var detailStack = new Grid
        //    {
        //        HorizontalOptions = LayoutOptions.FillAndExpand,
        //        BackgroundColor = Color.FromHex("#E0E4E7"),
        //        Padding = 4

        //    };
        //    detailStack.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        //    detailStack.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        //    detailStack.Children.Add(new StackLayout
        //    {
        //        Orientation = StackOrientation.Vertical,
        //        HorizontalOptions = LayoutOptions.CenterAndExpand,
        //        Children =
        //                {
        //                    new Label { Text = AppResources.duration_short_title, TextColor = Color.FromHex("#787E7E"), VerticalTextAlignment=TextAlignment.Start, HorizontalTextAlignment = TextAlignment.Center },
        //                    durationLabel
        //                }
        //    }, 0, 0);
        //    detailStack.Children.Add(new StackLayout
        //    {
        //        Orientation = StackOrientation.Vertical,
        //        HorizontalOptions = LayoutOptions.CenterAndExpand,
        //        Children =
        //                {
        //                    new Label { Text = AppResources.distance_short_title, TextColor = Color.FromHex("#787E7E"), VerticalTextAlignment=TextAlignment.Start, HorizontalTextAlignment = TextAlignment.Center},
        //                    distanceLabel
        //                }
        //    }, 1, 0);

        //    cellWrapper.Children.Add(previewImage);
        //    cellWrapper.Children.Add(secondSeperator);
        //    cellWrapper.Children.Add(timeStack);
        //    cellWrapper.Children.Add(detailStack);


        //    var innerFrame = new Frame { Padding = 0, HasShadow = true, BackgroundColor = Color.Transparent };
        //    innerFrame.Content = cellWrapper;

        //    var outerFrame = new Frame { Padding = new Thickness(5, 10, 5, 10), HasShadow = false, BackgroundColor = Color.Transparent };
        //    outerFrame.Content = innerFrame;


        //    mainStack.Children.Add(outerFrame);

        //}
        private async void UpdateLocation()
        {
            try
            {
                var lastLocationResponse = await EventAPI.GetEventsById(Settings.Current.DeviceId, (int)TripEventType.PolledPosition, startDate, endDate, isUnitId: true);

                if (lastLocationResponse == null || lastLocationResponse.Count == 0)
                {
                    lastPositionLabel.Text = "N/A";
                    return;
                }

                var lastLocation = lastLocationResponse.OrderByDescending(x => x.LocalTimestamp).FirstOrDefault();

                if (lastLocation != null)
                    lastPositionLabel.Text = $"{lastLocation.Position[0]}, {lastLocation.Position[1]}";

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
                    // Feature not supported on device
                }
                catch (Exception ex)
                {
                    // Handle exception that may have occurred in geocoding
                }

            }
            catch (NullReferenceException e)
            {
                
            }
            
        }
        private void SetupUI()
        {
            

            noRecordsLabel = new Label { Text = AppResources.label_no_trips, TextColor = Color.Gray, HorizontalOptions = LayoutOptions.CenterAndExpand, IsVisible = false };

            mainStack = new StackLayout { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, Padding = 10 };

            var customCell = new DataTemplate(typeof(CustomPersonalLogbookCell));
            customCell.SetBinding(CustomPersonalLogbookCell.TimeProperty, "StartLocalTimestamp.DateTime");
            customCell.SetBinding(CustomPersonalLogbookCell.EndTimeProperty, "EndLocalTimestamp.DateTime");
            customCell.SetBinding(CustomPersonalLogbookCell.DistanceProperty, "Distance");
            customCell.SetBinding(CustomPersonalLogbookCell.TripIdProperty, "Id");

            //tripLog = new ListView { VerticalOptions = LayoutOptions.FillAndExpand };
            tripLog.SeparatorVisibility = SeparatorVisibility.None;
            tripLog.SeparatorColor = Color.Transparent;
            tripLog.GroupHeaderTemplate = new DataTemplate(typeof(HeaderCell));
            ////tripLog.ItemSelected += OnTripSelection;
            tripLog.IsGroupingEnabled = true;
            tripLog.HasUnevenRows = true;
            tripLog.BackgroundColor = Color.Transparent;
            tripLog.ItemTemplate = customCell;
            syncIndicator = new ActivityIndicator
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false,
                IsEnabled = true,
            };

            tripsLabel = new Label
            {
                Text = AppResources.last_trips,
                FontSize = 16
            };

            var tripsLabelStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };
            tripsLabelStack.Children.Add(syncIndicator);
            tripsLabelStack.Children.Add(tripsLabel);

            lastPositionLabel = new Label { Text = AppResources.loading, TextColor = Color.White, FontSize = 13, Margin = new Thickness(8,0,0,0) };
            lastAddressLabel = new Label { TextColor = Color.White, FontSize = 13, Margin = new Thickness(8, 0, 0, 0) };

            var lastPositionStack = new StackLayout { HorizontalOptions = LayoutOptions.FillAndExpand, BackgroundColor = Color.Transparent, Padding = 0 };
            lastPositionStack.Children.Add(new Label { Text = AppResources.last_location, FontAttributes = FontAttributes.Bold, TextColor = Color.White, FontSize = 18 });
            lastPositionStack.Children.Add(lastPositionLabel);
            lastPositionStack.Children.Add(lastAddressLabel);

            UpdateLocation();

            //mainStack.Children.Add(tripLog);
            mainStack.Children.Add(new Frame { Content = lastPositionStack, HasShadow = true, CornerRadius = 0, Padding = 10, BackgroundColor = Palette.MainAccent.MultiplyAlpha(0.8) });
            mainStack.Children.Add(tripsLabelStack);
            mainStack.Children.Add(noRecordsLabel);
            mainStack.Children.Add(tripLog);

            Content = mainStack;

        }
        
        public Command RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    tripLog.IsRefreshing = true;

                    await RefreshTripList();

                    tripLog.IsRefreshing = false;
                });
            }
        }

        private async Task RefreshTripList()
        {
            noRecordsLabel.IsVisible = false;

            using (var scope = new ActivityIndicatorScope(syncIndicator, true))
            {
                if (tripList == null)
                    tripList = await TripsAPI.GetTripsWithStats(Settings.Current.DeviceId, startDate, endDate);

                if (tripList == null || tripList.Count == 0)
                {
                    noRecordsLabel.IsVisible = true;
                    return;
                }

                


                //var trips = await TripsAPI.GetExtendedTripAsync(selectedGroup, startDatepicker.Date, endDatepicker.Date);

                tripLog.ItemsSource = null;

                //Get number of exceptions
                //get earliest trip date
                var earliestTrip = tripList.MinBy(x => x.StartLocalTimestamp);
                var latestTrip = tripList.MaxBy(x => x.EndLocalTimestamp);
               

                var groupedData =
                            tripList.Where(p => p.Distance > 0.1)
                                 .OrderByDescending(p => p.EndLocalTimestamp)
                                 .GroupBy(p => p.EndLocalTimestamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.MonthDayPattern))
                                 .Select(p => new ObservableGroupCollection<string, Trip>(p))
                                 .ToList();

                if (groupedData.Count > 0)
                {
                    noRecordsLabel.IsVisible = false;
                }
                else
                    noRecordsLabel.IsVisible = true;

                Device.BeginInvokeOnMainThread(() => 
                {
                    tripLog.ItemsSource = groupedData;

                    tripLog.BindingContext = new ObservableCollection<ObservableGroupCollection<string, Trip>>(groupedData);
                });
                

                //tripsListView.GroupDisplayBinding = new Binding("Key");


            }
        }
    }
}