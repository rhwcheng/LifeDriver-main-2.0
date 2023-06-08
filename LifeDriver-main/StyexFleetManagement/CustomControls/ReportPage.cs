using StyexFleetManagement.Abstractions;
using StyexFleetManagement.Pages;
using StyexFleetManagement.Services;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class ReportPage: ContentPage
    {
        private DeviceOrientation orientation;
        private bool forceLandscape;

        public ReportPage() : base()
        {
            forceLandscape = true; //Force landscape by default

        }

        public ReportPage(bool forceLandscape) : base()
        {
            this.forceLandscape = forceLandscape;
        }

        public void NavigateToReportsMainPage(){

            if (forceLandscape && Device.Idiom == TargetIdiom.Phone && Device.RuntimePlatform == Device.Android)
            {
                if (orientation == DeviceOrientation.Landscape)
                    DependencyService.Get<IOrientationService>().Landscape();
                else
                    DependencyService.Get<IOrientationService>().Portrait();
            }
            (App.MainDetailPage).NavigateTo(typeof(ReportsPage));
        }

        protected override void OnAppearing()
        {
            App.ShowLoading(true);
            if (forceLandscape)
            {
                orientation = DependencyService.Get<IDeviceOrientationService>().GetOrientation();
                if (Device.Idiom == TargetIdiom.Phone)
                    DependencyService.Get<IOrientationService>().Landscape();
            }
            base.OnAppearing();
            App.ShowLoading(false);
        }

        protected override bool OnBackButtonPressed()
        {
            NavigateToReportsMainPage();
            return false;
        }

        protected override void OnDisappearing()
        {

            App.ShowLoading(true);
            base.OnDisappearing();
            if (forceLandscape && Device.Idiom == TargetIdiom.Phone) //&& Device.RuntimePlatform == Device.iOS)
            {
                if (orientation == DeviceOrientation.Landscape)
                    DependencyService.Get<IOrientationService>().Landscape();
                else
                    DependencyService.Get<IOrientationService>().Portrait();
            }
            App.ShowLoading(false);
            //if (Device.RuntimePlatform == Device.Android && Device.Idiom == TargetIdiom.Phone)
            //{
            //    ReportsPage page = null;
            //    foreach (var _page in Navigation.NavigationStack){
            //        if (page.GetType() == typeof(ReportsPage)){
            //            page = (ReportsPage)_page;
            //        }
            //    }
            //    if (page != null){
            //        page = (ReportsPage)Activator.CreateInstance(typeof(ReportsPage));
            //    }
            //}
        }
    }
}
