using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using StyexFleetManagement.Abstractions;
using StyexFleetManagement.iOS.Services;

[assembly: Xamarin.Forms.Dependency(typeof(OrientationService))]
namespace StyexFleetManagement.iOS.Services
{
    public class OrientationService : IOrientationService
    {
        public void Landscape()
        {
            UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)UIInterfaceOrientation.LandscapeLeft), new NSString("orientation"));
        }

        public void Portrait()
        {
            UIDevice.CurrentDevice.SetValueForKey(new NSNumber((int)UIInterfaceOrientation.Portrait), new NSString("orientation"));
        }
    }
}