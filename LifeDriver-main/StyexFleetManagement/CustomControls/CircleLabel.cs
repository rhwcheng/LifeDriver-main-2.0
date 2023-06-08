using System;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
	public class CircleLabel : Frame
    {
        public CircleLabel()
        {
            CornerRadius = 34;
            HeightRequest = 68;
            WidthRequest = 68;
            HorizontalOptions = LayoutOptions.Start;
            VerticalOptions = LayoutOptions.Start;
            Margin = 0;
            Padding = 0;
            IsClippedToBounds = true;
            OutlineColor = Color.Red;
            BackgroundColor = Color.Red;
        }
    }

    public class InnerCircleLabel : Frame
    {
        public InnerCircleLabel()
        {
            CornerRadius = 31;
            HeightRequest = 62;
            WidthRequest = 62;
            HorizontalOptions = LayoutOptions.Center;
            VerticalOptions = LayoutOptions.Center;
            Margin = 0;
            Padding = 0;
            IsClippedToBounds = true;
            BackgroundColor = Color.White;
                                       

        }

    }
}
