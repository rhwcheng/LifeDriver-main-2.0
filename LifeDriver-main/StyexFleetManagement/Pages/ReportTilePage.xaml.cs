using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using StyexFleetManagement.CustomControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Models;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
    public partial class ReportTilePage : PopupPage
    {
        private DashboardTile dashboardTile;

        public ReportTilePage(DashboardTileType type, List<Event> eventData)
        {
            InitializeComponent();

            HasSystemPadding = true;

            double sidePadding = 0;
            if (Device.Idiom == TargetIdiom.Phone)
            {
                sidePadding = 10;
            }
            else
            {
                sidePadding = (App.ScreenWidth) / 5;
            }
            Padding = new Thickness(sidePadding, 10, sidePadding, 10);

            var closeGestureRecognizer = new TapGestureRecognizer { Command = new Command(async () => await PopupNavigation.PopAsync()) };
            backButton.GestureRecognizers.Add(closeGestureRecognizer);

            dashboardTile = new DashboardTile(type, eventData, isFullScreen: true) { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, };
            dashboardTile.Init();
            MainStack.Children.Add(dashboardTile);
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

        }


    }
}
