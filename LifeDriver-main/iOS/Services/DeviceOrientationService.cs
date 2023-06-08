using StyexFleetManagement.iOS.Services;
using StyexFleetManagement.Services;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(DeviceOrientationService))]
namespace StyexFleetManagement.iOS.Services
{
    public class DeviceOrientationService : IDeviceOrientationService
    {
        public StyexFleetManagement.Services.DeviceOrientation GetOrientation()
        {
              UIInterfaceOrientation orientation = UIApplication.SharedApplication.StatusBarOrientation;

            bool isPortrait = orientation == UIInterfaceOrientation.Portrait ||
                orientation == UIInterfaceOrientation.PortraitUpsideDown;
            return isPortrait ? StyexFleetManagement.Services.DeviceOrientation.Portrait : StyexFleetManagement.Services.DeviceOrientation.Landscape;
        }
    }
}