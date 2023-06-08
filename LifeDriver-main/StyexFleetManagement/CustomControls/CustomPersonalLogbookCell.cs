using System;
using System.Globalization;
using System.Threading.Tasks;
using FFImageLoading.Forms;
using StyexFleetManagement.Helpers;
using StyexFleetManagement.Models;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class CustomPersonalLogbookCell : ViewCell
    {
        Label startTimeLabel, endTimeLabel, distanceLabel, durationLabel;

        TapGestureRecognizer plotGesture;

        StackLayout cellWrapper, startHorizontalLayout, endHorizontalLayout, lowerLayout;

        ActivityIndicator syncIndicator;

        CachedImage routePreviewImage;

        public static Color selectedColour = Color.FromHex("#A5D6A7");
        public static Color unselectedColour = Color.FromHex("#fff");

        public Task FetchRoutePreviewTask { get; private set; }

        public CustomPersonalLogbookCell()
        {
            cellWrapper = new StackLayout { Spacing = 0, Padding = 0, VerticalOptions = LayoutOptions.FillAndExpand };
            startHorizontalLayout = new StackLayout();
            endHorizontalLayout = new StackLayout();
            lowerLayout = new StackLayout();

           

            var firstSeperator = new BoxView { HeightRequest = 0.8, BackgroundColor = Color.Gray, HorizontalOptions = LayoutOptions.FillAndExpand };
            var secondSeperator = new BoxView { HeightRequest = 0.8, BackgroundColor = Color.Gray, HorizontalOptions = LayoutOptions.FillAndExpand };

            endTimeLabel = new Label { TextColor = Color.FromHex("#585858") };
            startTimeLabel = new Label { TextColor = Color.FromHex("#585858") };
            distanceLabel = new Label { FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#787E7E"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Start };
            durationLabel = new Label { FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#787E7E"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Start };

            //set bindings
            /*startTimeLabel.SetBinding(Label.TextProperty, "Time");
            endTimeLabel.SetBinding(Label.TextProperty, "EndTime");
            distanceLabel.SetBinding(Label.TextProperty, "Distance");
            numberOfExceptionsLabel.SetBinding(Label.TextProperty, "NumberOfExceptions");
            cellWrapper.SetBinding(StackLayout.BackgroundColorProperty, new Binding("IsSelected", BindingMode.Default, new BackgroundColourConverter(), null));
            */


            var plotButtonStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center
            };

            syncIndicator = new ActivityIndicator
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false,
                IsEnabled = true,
            };
            //plotButtonStack.Children.Add(plotButton);
            //plotButtonStack.Children.Add(syncIndicator);

            //Get map route image
            routePreviewImage = new CachedImage()
            {
                HorizontalOptions = LayoutOptions.Center,
                Margin = 0,
                Aspect = Aspect.AspectFill,
                CacheDuration = TimeSpan.FromDays(90),
                DownsampleToViewSize = true,
                RetryCount = 3,
                RetryDelay = 250
                //LoadingPlaceholder = "ic_loading.png",
                //ErrorPlaceholder = "error.png"
            };


            if (Device.Idiom == TargetIdiom.Phone)
            {
                routePreviewImage.WidthRequest = (Math.Min(App.ScreenWidth, App.ScreenHeight) * 0.9) - 38;
            }
            else
            {
                routePreviewImage.WidthRequest = (Math.Min(App.ScreenWidth, App.ScreenHeight) / 2) - 38;
            }
            routePreviewImage.HeightRequest = routePreviewImage.WidthRequest / 2;

            //add views to the view hierarchy
            /*startHorizontalLayout.Children.Add(startTime);
			startHorizontalLayout.Children.Add(startAddress);
			endHorizontalLayout.Children.Add(endTime);
			endHorizontalLayout.Children.Add(endAddress);
			lowerLayout.Children.Add(distance);
			cellWrapper.Children.Add(new Label { Text="Start", FontSize=10});
			cellWrapper.Children.Add(startHorizontalLayout);
			cellWrapper.Children.Add(new Label { Text = "End", FontSize=10 });
			cellWrapper.Children.Add(endHorizontalLayout);
			cellWrapper.Children.Add(lowerLayout);
			cellWrapper.Children.Add(plotButtonStack);
            cellWrapper.Children.Add(routePreviewImage);*/
            var timeStack = new StackLayout
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Orientation = StackOrientation.Horizontal,
                Padding = 4,
                Children =
                {
                    startTimeLabel,
                    new Label { Text = AppResources.to_label.ToLower(), TextColor=Color.Gray, FontSize = startTimeLabel.FontSize-1},
                    endTimeLabel
                }
            };

            detailStack = new Grid
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.FromHex("#E0E4E7"),
                Padding = 4

            };
            detailStack.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            detailStack.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            detailStack.Children.Add(new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Children =
                        {
                            new Label { Text = AppResources.duration_short_title, TextColor = Color.FromHex("#787E7E"), VerticalTextAlignment=TextAlignment.Start, HorizontalTextAlignment = TextAlignment.Center },
                            durationLabel
                        }
            }, 0, 0);
            detailStack.Children.Add(new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Children =
                        {
                            new Label { Text = AppResources.distance_short_title, TextColor = Color.FromHex("#787E7E"), VerticalTextAlignment=TextAlignment.Start, HorizontalTextAlignment = TextAlignment.Center},
                            distanceLabel
                        }
            }, 1, 0);

            cellWrapper.Children.Add(plotButtonStack);
            cellWrapper.Children.Add(routePreviewImage);
            cellWrapper.Children.Add(secondSeperator);
            cellWrapper.Children.Add(timeStack);
            cellWrapper.Children.Add(detailStack);


            cellWrapper.GestureRecognizers.Add(plotGesture);

            var innerFrame = new Frame { Padding = 0, HasShadow = true, BackgroundColor = Color.Transparent };
            innerFrame.Content = cellWrapper;

            var outerFrame = new Frame { Padding = new Thickness(5, 10, 5, 10), HasShadow = false, BackgroundColor = Color.Transparent };
            outerFrame.Content = innerFrame;


            View = outerFrame;



        }
        
        public void SetBackgroundSelected()
        {
            this.cellWrapper.BackgroundColor = selectedColour;
        }

        public static readonly BindableProperty TripIdProperty = BindableProperty.Create("TripId", typeof(int), typeof(CustomCell), -1);
        public static readonly BindableProperty TimeProperty = BindableProperty.Create("StartLocalTimestamp", typeof(DateTimeOffset), typeof(CustomCell), DateTimeOffset.Now);
        public static readonly BindableProperty EndTimeProperty = BindableProperty.Create("EndLocalTimestamp", typeof(DateTimeOffset), typeof(CustomCell), DateTimeOffset.Now);
        //public static readonly BindableProperty StartAddressProperty = BindableProperty.Create("StartAddress", typeof(string), typeof(CustomCell), "StartAddress");
        //public static readonly BindableProperty EndAddressProperty = BindableProperty.Create("EndAddress", typeof(string), typeof(CustomCell), "EndAddress");
        public static readonly BindableProperty DistanceProperty = BindableProperty.Create("Distance", typeof(string), typeof(CustomCell), "Distance");
        //public static readonly BindableProperty RouteCoordinatesProperty = BindableProperty.Create("RouteCoordinates", typeof(List<List<float>>), typeof(CustomCell), new List<List<float>>());
        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create("IsSelected", typeof(bool), typeof(CustomCell), false);
        private Label exceptionTitleLabel;
        private Grid detailStack;

        public int TripId
        {
            get => (int)GetValue(TripIdProperty);
            set => SetValue(TripIdProperty, value);
        }
        public DateTimeOffset Time
        {
            get => (DateTimeOffset)GetValue(TimeProperty);
            set => SetValue(TimeProperty, value);
        }

        public DateTimeOffset EndTime
        {
            get => (DateTimeOffset)GetValue(EndTimeProperty);
            set => SetValue(EndTimeProperty, value);
        }

        /*public string StartAddress
        {
            get { return (string)GetValue(StartAddressProperty); }
            set { SetValue(StartAddressProperty, value); }
        }
        public List<List<float>> RouteCoordinates
        {
            get { return (List<List<float>>)GetValue(RouteCoordinatesProperty); }
            set { SetValue(RouteCoordinatesProperty, value); }
        }
        public string EndAddress
        {
            get { return (string)GetValue(EndAddressProperty); }
            set { SetValue(EndAddressProperty, value); }
        }*/


        public string Distance
        {
            get => (string)GetValue(DistanceProperty);
            set => SetValue(DistanceProperty, value);
        }
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set
            {
                SetValue(IsSelectedProperty, value);
                if (value)
                {
                    cellWrapper.BackgroundColor = selectedColour;
                }
                else
                {
                    cellWrapper.BackgroundColor = unselectedColour;
                }
            }
        }


        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                try
                {
                    var trip = (Trip)BindingContext;
                    startTimeLabel.Text = trip.StartLocalTimestamp.LocalDateTime.ToString("HH:mm tt");
                    endTimeLabel.Text = trip.EndLocalTimestamp.LocalDateTime.ToString("HH:mm tt");
                    //startAddress.Text = StartAddress;
                    //endAddress.Text = EndAddress;
                    decimal distance = Math.Round(decimal.Parse(Distance, CultureInfo.InvariantCulture), 2);
                    distanceLabel.FormattedText = FormatHelper.FormatDistance(distance.ToString());

                    TimeSpan duration = EndTime - Time;
                    durationLabel.Text = FormatHelper.ToShortForm(duration);

                    if (IsSelected)
                        cellWrapper.BackgroundColor = selectedColour;
                    else
                        cellWrapper.BackgroundColor = unselectedColour;

                    FetchRoutePreviewTask = SetRoutePreviewImage(TripId);
                }
                catch (Exception e)
                {

                }
                
                

            }
        }

        private async Task SetRoutePreviewImage(int tripId)
        {
            var tripPositions = await TripsAPI.GetTripPositionsAsync(tripId);

            var url = TripsAPI.GetStaticMapImageUrlForRoute(tripPositions.Items);

            routePreviewImage.Source = url;


        }

    }
    
}

