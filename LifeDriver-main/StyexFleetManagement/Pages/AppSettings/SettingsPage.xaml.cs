using System;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages.AppSettings
{
    public partial class SettingsPage : PopupPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            SetUpPadding();
            SetUpGestureRecognizers();
        }

        private void SetUpPadding()
        {
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



        }

        private void SetUpGestureRecognizers()
        {
            var closeGestureRecognizer = new TapGestureRecognizer();
            closeGestureRecognizer.Tapped += CloseGestureRecognizer_Tapped;

            backButton.GestureRecognizers.Add(closeGestureRecognizer);
        }

        private async void CloseGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await PopupNavigation.PopAsync();
        }
    }
}
