using System;
using Android.App;
using StyexFleetManagement.Abstractions;
using StyexFleetManagement.Droid.Services;
using Xamarin.Forms;
using Android.Content.PM;

[assembly: Xamarin.Forms.Dependency(typeof(OrientationService))]
namespace StyexFleetManagement.Droid.Services
{
    public class OrientationService : IOrientationService
    {
        public void Landscape()
        {
            ((Activity)Forms.Context).RequestedOrientation = ScreenOrientation.Landscape;
        }

        public void Portrait()
        {
            ((Activity)Forms.Context).RequestedOrientation = ScreenOrientation.Portrait;
        }
    }
}