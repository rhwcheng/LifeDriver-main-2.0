using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class HeaderCell : ViewCell
    {
        public HeaderCell()
        {
            Height = 35;
            
            
            var title = new Label
            {
                FontSize = Device.GetNamedSize(NamedSize.Small, this),
                TextColor = Color.FromHex("#737273"),
                VerticalOptions = LayoutOptions.Center
            };

            title.SetBinding(Label.TextProperty, "Key");

            View = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                HeightRequest = 35,
                BackgroundColor = Color.Transparent,
                Padding = 5,
                Orientation = StackOrientation.Horizontal,
                Children = { title }
            };
        }
    }
}
