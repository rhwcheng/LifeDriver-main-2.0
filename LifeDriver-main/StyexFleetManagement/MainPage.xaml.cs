using Rg.Plugins.Popup.Extensions;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Extensions;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Services;
using StyexFleetManagement.Statics;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace StyexFleetManagement
{
    public partial class MainPage : MasterDetailPage
    {
        public string Test { get; set; }


        public MainPage()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.iOS)
            {
                IsGestureEnabled = false;
            }

            masterPage.ListView.ItemSelected += OnItemSelected;
            masterPage.SettingsClick.Tapped += OnSettingsSelected;
            masterPage.LogoutClick.Tapped += OnLogoutSelected;

            NavigateTo(typeof(FavouriteReportsPage));

            ShowLoginDialog();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (App.PageWidth.NearlyEqual(width) && App.PageHeight.NearlyEqual(height)) return;

            App.PageHeight = height;
            App.PageWidth = width;
        }

        async void ShowLoginDialog()
        {
            var page = new LoginPage();

            await Navigation.PushModalAsync(page);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (!(e.SelectedItem is MasterPageItem item)) return;

            masterPage.ListView.SelectedItem = null;
            IsPresented = false;

            if (item.TargetType == typeof(FlicPage))
            {
                DependencyService.Get<IFlicService>().InitializeFlic();

                return;
            }
            //if (Device.OS == TargetPlatform.Android)
            //{
            //    if (item.TargetType.Equals(typeof(MapPage)))
            //    {
            //        Detail = new NavigationPage((Page)Activator.CreateInstance(typeof(AndroidMapPage))) { BarBackgroundColor = Palette.NavBar };
            //    }
            //    else
            //    {
            //        var page = new NavigationPage((Page)Activator.CreateInstance(item.TargetType)) { BarBackgroundColor = Palette.NavBar };
            //        Detail = page;
            //    }

            //}

            //else
            //{
            Detail = new NavigationPage((Page) Activator.CreateInstance(item.TargetType))
                {BarBackgroundColor = Palette.NavBar};
            //}


        }

        void OnSettingsSelected(object sender, EventArgs e)
        {
            Detail = new NavigationPage((Page) Activator.CreateInstance(typeof(SettingsPageStandalone)))
                {BarBackgroundColor = Palette.NavBar};

            masterPage.ListView.SelectedItem = null;
            IsPresented = false;
        }

        async void OnLogoutSelected(object sender, EventArgs e)
        {
            DependencyService.Get<ICredentialsService>().DeleteCredentials();
            SettingsContent.ClearCache(isLoggingOut: true);

            await Navigation.PushModalAsync(new LoginPage());

            masterPage.ListView.SelectedItem = null;
            IsPresented = false;
        }

        public async void NavigateTo(Type TargetPage, DashboardTileType tileType = DashboardTileType.DISTANCE,
            List<Event> eventData = null)
        {
            if (TargetPage == typeof(SettingsPage))
            {
                //Detail.Navigation.PushAsync(new SettingsPage());
                var page = new SettingsPage();

                await Navigation.PushPopupAsync(page);

                return;
            }

            if (TargetPage == typeof(PersonalLogbookLogPage))
            {
                //Detail.Navigation.PushAsync(new SettingsPage());
                var page = new PersonalLogbookLogPage();

                await Navigation.PushPopupAsync(page);

                return;
            }

            if (TargetPage == typeof(ReportTilePage))
            {
                var page = new ReportTilePage(tileType, eventData);

                await Navigation.PushPopupAsync(page);

                return;
            }

            if (TargetPage == typeof(FlicPage))
            {
                DependencyService.Get<IFlicService>().InitializeFlic();

                return;
            }

            Detail = new NavigationPage((Page) Activator.CreateInstance(TargetPage))
            {
                BarBackgroundColor = Palette.NavBar
            };

            IsPresented = false;
        }
    }
}