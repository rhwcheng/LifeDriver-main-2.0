using StyexFleetManagement.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StyexFleetManagement.CustomControls
{
    public partial class TripViewCell : ViewCell
    {
        public TripViewCell()
        {
            InitializeComponent();
            if (Device.Idiom == TargetIdiom.Phone)
            {
                routePreviewImage.WidthRequest = (Math.Min(App.ScreenWidth, App.ScreenHeight) * 0.9) - 38;
            }
            else
            {
                routePreviewImage.WidthRequest = (Math.Min(App.ScreenWidth, App.ScreenHeight)/1.4) - 38;
            }

            if (Device.OS == TargetPlatform.iOS)
            {
                innerFrame.HasShadow = false;
            }
            routePreviewImage.HeightRequest = routePreviewImage.WidthRequest / 2;
        }

        public Task FetchRoutePreviewTask { get; private set; }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (BindingContext != null)
            {
                var trip = (TripModel)BindingContext;
                
                FetchRoutePreviewTask = SetRoutePreviewImage(trip.Id);
            }
        }
        private async Task SetRoutePreviewImage(int tripId)
        {
            var tripPositions = await TripsAPI.GetTripPositionsAsync(tripId);

            var url = TripsAPI.GetStaticMapImageUrlForRoute(tripPositions.Items);

            routePreviewImage.Source = url;


        }
    }
}