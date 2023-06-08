using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class VehicleListCell : ViewCell
    {
        Label vehicleLabel;
        Image vehiclePin;

        public VehicleListCell()
        {
            vehicleLabel = new Label { VerticalTextAlignment = TextAlignment.Center };
            vehiclePin = new Image { Source = ImageFilename, HorizontalOptions = LayoutOptions.EndAndExpand };

            var mainStack = new StackLayout
            {
                Padding = new Thickness(20, 0, 0, 0),
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    vehicleLabel,
                    vehiclePin
                }
            };

            View = mainStack;
        }
        public static readonly BindableProperty VehicleNameProperty =
          BindableProperty.Create("VehicleName", typeof(string), typeof(VehicleListCell), "1");

        public string VehicleName
        {
            get => (string)GetValue(VehicleNameProperty);
            set => SetValue(VehicleNameProperty, value);
        }
        
        public static readonly BindableProperty ImageFilenameProperty =
          BindableProperty.Create("ImageFilename", typeof(string), typeof(VehicleListCell), "");

        public string ImageFilename
        {
            get => (string)GetValue(ImageFilenameProperty);
            set => SetValue(ImageFilenameProperty, value);
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                vehicleLabel.Text = VehicleName;
                vehiclePin.Source = ImageFilename;

            }
        }
    }
}
