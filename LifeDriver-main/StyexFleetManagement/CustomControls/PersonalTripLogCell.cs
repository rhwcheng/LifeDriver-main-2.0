using FFImageLoading.Forms;
using StyexFleetManagement.Resx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class PersonalTripLogCell : ViewCell
    {
        public PersonalTripLogCell()
        {
            //instantiate each of our views
            var cellWrapper = new StackLayout { Spacing = 0, Padding = 0, VerticalOptions = LayoutOptions.FillAndExpand };
            var startHorizontalLayout = new StackLayout();
            var endHorizontalLayout = new StackLayout();
            var lowerLayout = new StackLayout();
            var firstSeperator = new BoxView { HeightRequest = 0.8, BackgroundColor = Color.Gray, HorizontalOptions = LayoutOptions.FillAndExpand };
            var secondSeperator = new BoxView { HeightRequest = 0.8, BackgroundColor = Color.Gray, HorizontalOptions = LayoutOptions.FillAndExpand };
            var endTimeLabel = new Label { TextColor = Color.FromHex("#585858") };
            var startTimeLabel = new Label { TextColor = Color.FromHex("#585858") };
            var distanceLabel = new Label { FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#787E7E"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Start };
            var durationLabel = new Label { FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#787E7E"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Start };

            var previewImage = new CachedImage()
            {
                HorizontalOptions = LayoutOptions.Center,
                Margin = 0,
                Aspect = Aspect.AspectFill,
                CacheDuration = TimeSpan.FromDays(90),
                DownsampleToViewSize = true,
                RetryCount = 3,
                RetryDelay = 250
            };
            //if (Device.Idiom == TargetIdiom.Phone)
            //{
            //    previewImage.WidthRequest = (Math.Min(App.ScreenWidth, App.ScreenHeight) * 0.9) - 38;
            //}
            //else
            //{
            //    previewImage.WidthRequest = (Math.Min(App.ScreenWidth, App.ScreenHeight) / 2) - 38;
            //}
            previewImage.WidthRequest = (Math.Min(App.ScreenWidth, App.ScreenHeight) * 0.9);
            previewImage.HeightRequest = previewImage.WidthRequest / 2;

            //set bindings
            distanceLabel.SetBinding(Label.TextProperty, "DistanceString");
            durationLabel.SetBinding(Label.TextProperty, "DurationString");
            previewImage.SetBinding(CachedImage.SourceProperty, "Url");
            startTimeLabel.SetBinding(Label.TextProperty, "StartTime");
            endTimeLabel.SetBinding(Label.TextProperty, "EndTime");

            //add views to the view hierarchy
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

            var detailStack = new Grid
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

            cellWrapper.Children.Add(previewImage);
            cellWrapper.Children.Add(secondSeperator);
            cellWrapper.Children.Add(timeStack);
            cellWrapper.Children.Add(detailStack);


            var innerFrame = new Frame { Padding = 0, HasShadow = true, BackgroundColor = Color.Transparent };
            innerFrame.Content = cellWrapper;

            var outerFrame = new Frame { Padding = new Thickness(5, 10, 5, 10), HasShadow = false, BackgroundColor = Color.Transparent };
            outerFrame.Content = innerFrame;


            View = outerFrame;
        }
    }
}
