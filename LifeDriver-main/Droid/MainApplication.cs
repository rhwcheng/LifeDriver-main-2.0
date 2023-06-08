using Android.App;
using Android.Runtime;
using Shiny;
using System;

namespace StyexFleetManagement.Droid
{
    [Application]
    public class MainApplication : Application
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }


        public override void OnCreate()
        {
            base.OnCreate();
            this.ShinyOnCreate(new Startup());
            Xamarin.Essentials.Platform.Init(this);
        }
    }
}