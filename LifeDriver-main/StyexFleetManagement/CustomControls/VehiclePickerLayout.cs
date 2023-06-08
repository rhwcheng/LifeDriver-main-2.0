using FFImageLoading.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class VehiclePickerLayout : StackLayout
    {
        public VehiclePickerLayout() : base()
        {
            Orientation = StackOrientation.Horizontal;

            var vehicleImage = new CachedImage { Source = "ic_truck", Aspect = Aspect.AspectFit };
            var vehiclePicker = new VehiclePicker();

            Children.Add(vehicleImage);
            Children.Add(vehiclePicker);
        }
    }
}
