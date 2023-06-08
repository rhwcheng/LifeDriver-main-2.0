using System;
using FFImageLoading.Forms;
using Xamarin.Forms;

namespace StyexFleetManagement.Models
{
	public class VehicleListViewCell : ViewCell
	{
		Label title, subtitle;
		CachedImage image;

		public VehicleListViewCell()
		{
			//instantiate each of our views
			image = new CachedImage()
			{
				WidthRequest = 50,
				HeightRequest = 50,
				DownsampleHeight = 50,
				DownsampleUseDipUnits = true,
				Aspect = Aspect.AspectFill,
				CacheDuration = TimeSpan.FromDays(30),
				RetryCount = 3,
				RetryDelay = 500,
				LoadingPlaceholder = "loading.png",
			};

			StackLayout cellWrapper = new StackLayout();
			StackLayout horizontalLayout = new StackLayout();
			title = new Label();
			subtitle = new Label();

			//Set properties for desired design
			cellWrapper.BackgroundColor = Color.FromHex("#eee");
			horizontalLayout.Orientation = StackOrientation.Horizontal;
			subtitle.HorizontalOptions = LayoutOptions.EndAndExpand;
			title.TextColor = Color.FromHex("#f35e20");
			subtitle.TextColor = Color.FromHex("503026");

			//add views to the view hierarchy
			horizontalLayout.Children.Add(image);
			horizontalLayout.Children.Add(title);
			horizontalLayout.Children.Add(subtitle);
			cellWrapper.Children.Add(horizontalLayout);
			View = cellWrapper;
		}


		public static readonly BindableProperty TitleProperty = BindableProperty.Create("Title", typeof(string), typeof(VehicleListViewCell), "Title");
		public static readonly BindableProperty SubtitleProperty = BindableProperty.Create("Subtitle", typeof(string), typeof(VehicleListViewCell), "Subtitle");
		public static readonly BindableProperty ImageProperty = BindableProperty.Create("Icon", typeof(CachedImage), typeof(VehicleListViewCell));

		public string Title
		{
			get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

		public string Subtitle
		{
			get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

		public CachedImage Icon
		{
			get => (CachedImage)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

		protected override void OnBindingContextChanged()
		{
			base.OnBindingContextChanged();

			if (BindingContext != null)
			{
				title.Text = Title;
				subtitle.Text = Subtitle;
				if (Icon != null)
				{
					image.Source = Icon.Source;
				}
			}
		}




	}


}

