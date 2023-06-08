using StyexFleetManagement.Models;
using StyexFleetManagement.Pages;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel.Base;
using System;
using Microsoft.AppCenter.Crashes;
using Xamarin.Forms;

namespace StyexFleetManagement
{
    internal enum LoginProvider
    {
        Salus = 0,
        Mzone = 1
    }

    public partial class LoginPage : ContentPage
    {
        private readonly ICredentialsService _storeService;
        private readonly IAuthenticationService _authenticationService;
        private LoginProvider _loginProvider = LoginProvider.Salus;

        public LoginPage()
        {
            try
            {
                InitializeComponent();

                _storeService = DependencyService.Get<ICredentialsService>();
                _authenticationService = ViewModelLocator.Resolve<IAuthenticationService>();
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, e.Message);
                Crashes.TrackError(e);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            try
            {
                var mZoneCredentialsExist = _storeService.DoMZoneCredentialsExist();
                var salusCredentialsExist = _storeService.DoSalusCredentialsExist();

                if (mZoneCredentialsExist)
                {
                    var username = DependencyService.Get<ICredentialsService>().MZoneUserName;
                    var password = DependencyService.Get<ICredentialsService>().MZonePassword;

                    App.ShowLoading(true);
                    try
                    {
                        var loggedIn = await _authenticationService.LoginMZone(username, password);
                        if (loggedIn)
                        {
                            MessagingCenter.Send(this, "LoggedIn");

                            Type landingPageType;

                            switch (Settings.Current.LandingPage)
                            {
                                case LandingPage.Dashboard:
                                    landingPageType = typeof(DashboardCarousel);
                                    break;
                                case LandingPage.Map:
                                    landingPageType = typeof(MapPage);
                                    break;
                                case LandingPage.Sos:
                                    landingPageType = salusCredentialsExist
                                        ? typeof(SosPage)
                                        : typeof(DashboardCarousel);
                                    break;
                                default:
                                    landingPageType = typeof(DashboardCarousel);
                                    break;
                            }

                            App.MainDetailPage.NavigateTo(landingPageType);
                            await Navigation.PopModalAsync();
                        }
                        else
                        {
                            OnLoginFailure();
                        }
                    }
                    catch
                    {
                        await DisplayAlert("Login failed", "An error occurred during login. Please try again.", "OK");
                    }

                    return;
                }

                if (salusCredentialsExist)
                {
                    var username = DependencyService.Get<ICredentialsService>().SalusUserName;
                    var password = DependencyService.Get<ICredentialsService>().SalusPassword;

                    App.ShowLoading(true);
                    var loggedIn = await _authenticationService.LoginSalus(username, password);
                    if (loggedIn)
                    {
                        MessagingCenter.Send(this, "LoggedIn");
                        App.MainDetailPage.NavigateTo(typeof(SosPage));
                        await Navigation.PopModalAsync();
                    }
                    else
                    {
                        OnLoginFailure();
                    }
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, e.Message);
            }
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private void OnLoginFailure()
        {
            App.ShowLoading(false);
            errorContainer.IsVisible = true;
            messageLabel.Text =
                "Login failed. Please ensure that your username and password are correct and try again.";
            passwordEntry.Text = string.Empty;
        }

        private void OnLoginButtonClicked(object sender, EventArgs e)
        {
            var userName = usernameEntry.Text;
            var password = passwordEntry.Text;

            Settings.Current.Server = Constants.DEFAULT_MZONE_SERVER;

            Login(userName, password);
        }

        private async void Login(string userName, string password)
        {
            var loggedIn = false;
            App.ShowLoading(true);
            if (_loginProvider == LoginProvider.Salus)
            {
                loggedIn = await _authenticationService.LoginSalus(userName, password);
                if (loggedIn)
                {
                    Settings.Current.LandingPage = LandingPage.Sos;
                    App.MainDetailPage.NavigateTo(typeof(SosPage));
                    await Navigation.PopModalAsync();
                }

            }
            else
            {
                var mzoneId = await _authenticationService.TryGetMZoneUserId(userName, password);

                if (mzoneId != null)
                {
                    try
                    {
                        var doCredentialsExist = _storeService.DoMZoneCredentialsExist();
                        if (!doCredentialsExist)
                        {
                            await _storeService.SaveMZoneCredentialsAsync(userName, password);
                        }

                        await _authenticationService.ProcessMZoneLogin(mzoneId.Value);
                        Type landingPageType;

                        switch (Settings.Current.LandingPage)
                        {
                            case LandingPage.Dashboard:
                                landingPageType = typeof(DashboardCarousel);
                                break;
                            case LandingPage.Map:
                                landingPageType = typeof(MapPage);
                                break;
                            default:
                                landingPageType = typeof(DashboardCarousel);
                                break;
                        }

                        loggedIn = true;
                        App.MainDetailPage.NavigateTo(landingPageType);
                        await Navigation.PopModalAsync();
                    }

                    catch (InvalidCastException ex)
                    {
                        App.ShowLoading(false);
                        Serilog.Log.Debug(ex.Message);
                        await DisplayAlert("Login failed", "An error occurred during login. Please try again.", "OK");
                    }

                    return;
                }
            }

            if (!loggedIn)
            {
                OnLoginFailure();
            }
            else
            {

                MessagingCenter.Send(this, "LoggedIn");
            }
        }

        private void ChangeLoginProvider(LoginProvider loginProvider)
        {
            _loginProvider = loginProvider;
            backgroundImage.Source = loginProvider == LoginProvider.Salus ? ImageSource.FromFile("login_background_salus.jpg") : ImageSource.FromFile("login_background_mzone.jpg");
            usernameEntry.TextColor = passwordEntry.TextColor = loginProvider == LoginProvider.Salus ? Color.Black : Color.White;
        }

        private void OnSalusLoginClicked(object sender, EventArgs e)
        {
            ChangeLoginProvider(LoginProvider.Salus);
        }

        private void OnMzoneLoginClicked(object sender, EventArgs e)
        {
            ChangeLoginProvider(LoginProvider.Mzone);
        }
    }
}