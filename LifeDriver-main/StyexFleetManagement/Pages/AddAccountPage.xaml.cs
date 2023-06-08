using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using StyexFleetManagement.Enums;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel.Base;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StyexFleetManagement.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddAccountPage : PopupPage
    {
        private readonly AccountType _accountType;
        private readonly IAuthenticationService _authenticationService;

        public AddAccountPage(AccountType accountType)
        {
            InitializeComponent();
            SetUpPadding();
            SetUpGestureRecognizers();

            _accountType = accountType;
            _authenticationService = ViewModelLocator.Resolve<IAuthenticationService>();

            if (accountType != AccountType.MZone)
            {
                serverEntry.IsVisible = false;
            }
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
            await PopupNavigation.Instance.PopAsync();
        }

        private async void OnProceedClicked(object sender, EventArgs e)
        {
            bool loggedIn;
            if (_accountType == AccountType.MZone)
            {
                loggedIn = await _authenticationService.LoginMZone(usernameEntry.Text, passwordEntry.Text);
            }
            else
            {
                loggedIn = await _authenticationService.LoginSalus(usernameEntry.Text, passwordEntry.Text);
            }

            if (loggedIn)
            {
                await PopupNavigation.Instance.PopAsync();
            }
            else
            {
                App.ShowLoading(false);
                errorContainer.IsVisible = true;
                messageLabel.Text =
                    "Login failed. Please ensure that your username and password are correct and try again.";
                passwordEntry.Text = string.Empty;
            }
        }
    }
}