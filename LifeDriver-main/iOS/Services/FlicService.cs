//using Acr.UserDialogs;
//using FlicLib;
//using Foundation;
//using StyexFleetManagement.iOS.Services;
//using StyexFleetManagement.Resx;
//using StyexFleetManagement.Services;
//using System;
//using StyexFleetManagement.ViewModel.Base;
//using UniversalBeacon.Library.Core.Interfaces;
//using Xamarin.Forms;

//[assembly: Dependency(typeof(FlicService))]

//namespace StyexFleetManagement.iOS.Services
//{
//    public class FlicService : IFlicService
//    {
//        private const string FlicAppId = "1264e4b994-e04a-1a8d-4ca1-c162c54a4a";
//        private const string FlicAppSecret = "22fcd9ad43-044d-ddf7-56b4-3652a7b086";

//        public static void Init()
//        {
//            ViewModelLocator.Register<SCLFlicManager, SCLFlicManager>(new SCLFlicManager(new SCLFlicManagerDelegate(), FlicAppId, FlicAppSecret, true, true));
//        }

//        public void InitializeFlic()
//        {
//            var t = ViewModelLocator.Resolve<SCLFlicManager>();
//            UserDialogs.Instance.Toast(AppResources.flic_not_installed);
//        }

//        public IntPtr Handle { get; }
//        public void Dispose()
//        {
//            throw new NotImplementedException();
//        }

//        public void FlicManagerDidGrabFlicButton(SCLFlicManager manager, SCLFlicButton button, NSError error)
//        {
//            throw new NotImplementedException();
//        }
//    }

//    public class FlicManagerDelegate : SCLFlicManagerDelegate
//    {
//    }

//    public class FlicButtonDelegate : SCLFlicButtonDelegate
//    {
//    }
//}