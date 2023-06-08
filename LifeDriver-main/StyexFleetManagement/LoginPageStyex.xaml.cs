using StyexFleetManagement.Abstractions;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages;
using StyexFleetManagement.Services;
using StyexFleetManagement.Salus.Services;
using System;
using System.Threading.Tasks;
using StyexFleetManagement.ViewModel.Base;
using Xamarin.Forms;

namespace StyexFleetManagement
{
    public partial class LoginPageStyex : ContentPage
    {
        private readonly ICredentialsService _storeService;

        public LoginPageStyex()
        {
            InitializeComponent();

            _storeService = DependencyService.Get<ICredentialsService>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            //if (!_storeService.DoCredentialsExist()) return;

            //App.ShowLoading(true);
            //var username = DependencyService.Get<ICredentialsService>().UserName;
            //var password = DependencyService.Get<ICredentialsService>().Password;

            //Login(username, password);
        }

        private void OnLoginButtonClicked(object sender, EventArgs e)
        {
            var userName = usernameEntry.Text;
            var password = passwordEntry.Text;

            Settings.Current.Server = string.IsNullOrEmpty(serverEntry.Text)
                ? Constants.DEFAULT_MZONE_SERVER
                : serverEntry.Text;

            Login(userName, password);
        }

        private async void Login(string userName, string password)
        {
            App.ShowLoading(true);
            var mzoneId = await TryGetMZoneUserId(userName, password);

            if (mzoneId != null)
            {
                try
                {
                    var doCredentialsExist = _storeService.DoMZoneCredentialsExist();
                    if (!doCredentialsExist)
                    {
                        await _storeService.SaveMZoneCredentialsAsync(userName, password);
                    }

                    await ProcessMZoneLogin(mzoneId.Value);

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

                    App.MainDetailPage.NavigateTo(landingPageType);

                    int numModals = Application.Current.MainPage.Navigation.ModalStack.Count;
                    for (int currModal = 0; currModal < numModals; currModal++)
                    {
                        await Application.Current.MainPage.Navigation.PopModalAsync();
                    }
                }

                catch (InvalidCastException ex)
                {
                    App.ShowLoading(false);
                    Serilog.Log.Debug(ex.Message);
                    await DisplayAlert("Login failed", "An error occurred during login. Please try again.", "OK");
                }

                return;
            }

            App.ShowLoading(false);
            errorContainer.IsVisible = true;
            messageLabel.Text =
                "Login failed. Please ensure that your username and password are correct and try again.";
            passwordEntry.Text = string.Empty;
        }

        private async Task ProcessMZoneLogin(Guid mzoneId)
        {
            if (string.IsNullOrEmpty(Settings.Current.Server))
            {
                Settings.Current.Server = Constants.DEFAULT_MZONE_SERVER;
            }

            //Configure Settings
            Settings.Current.MzoneUserId = mzoneId;

            var response = await RestService.GetUserSettings(mzoneId);

            var userSettings = (UserSettings) response.ResponseToken;

            Settings.Current.UserDescription = userSettings.Description;
            Settings.Current.Currency = userSettings.CurrencyCode;
            Settings.Current.DistanceMeasurementUnit = userSettings.UnitOfMeasureDistanceId;
            Settings.Current.FluidMeasurementUnit = userSettings.UnitOfMeasureFluidId;
            Settings.Current.UtcOffset = userSettings.UtcOffset;

            //TODO: Clear all settings?

            Settings.Current.LastLoginDate = DateTime.Now;

            await App.InitializeData();

            MessagingCenter.Send(this, "LoggedIn");

            App.ShowLoading(false);

        }

        private static async Task<Guid?> TryGetMZoneUserId(string username, string password)
        {
            var response = await RestService.GetUsersAsync(username, password);

            if (response.ResponseToken != null && response.ErrorStatus == ServiceError.NO_ERROR)
            {
                return ((User) response.ResponseToken).Id;
            }

            return null;
        }

        private async Task<string> TryGetSalusUserId(string username, string password)
        {
            App.ShowLoading(false);
            var permissionService = ViewModelLocator.Resolve<IPermissionsService>();
            if (!await permissionService.GetPhonePermissionAsync()) return null;
            App.ShowLoading(true);

            var salusUserService = new UserService();

            return (await salusUserService.Login(username, password, DependencyService.Get<IDevice>().GetImei()))
                .User.DeviceId;
        }
    }
}