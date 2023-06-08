using Xamarin.Forms;

namespace StyexFleetManagement.Models
{
	public class ExtendedCell : ViewCell
	{
		Label startTime, endTime, address, distance;

		public ExtendedCell()
		{
			StackLayout cellWrapper = new StackLayout();
			StackLayout horizontalLayout = new StackLayout();
			StackLayout lowerLayout = new StackLayout();
			startTime = new Label();
			startTime.FontSize = 8;
			address = new Label();
			address.FontSize = 8;
			distance = new Label();
			distance.FontSize = 8;
			endTime = new Label();
			endTime.FontSize = 8;


			//set bindings
			startTime.SetBinding(Label.TextProperty, "time");
			address.SetBinding(Label.TextProperty, "address");
			distance.SetBinding(Image.SourceProperty, "distance");

			//Set properties for desired design
			cellWrapper.BackgroundColor = Color.FromHex("#eee");
			horizontalLayout.Orientation = StackOrientation.Horizontal;
			address.HorizontalOptions = LayoutOptions.EndAndExpand;
			startTime.FontAttributes = FontAttributes.Bold;
			distance.FontSize = 8.0;
			distance.HorizontalOptions = LayoutOptions.EndAndExpand;

			//add views to the view hierarchy
			horizontalLayout.Children.Add(startTime);
			horizontalLayout.Children.Add(address);
			lowerLayout.Children.Add(distance);
			cellWrapper.Children.Add(horizontalLayout);
			cellWrapper.Children.Add(lowerLayout);
			View = cellWrapper;
		}
	}
}

