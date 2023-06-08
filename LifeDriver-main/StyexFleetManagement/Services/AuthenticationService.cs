using StyexFleetManagement.Abstractions;
using StyexFleetManagement.Enums;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages;
using StyexFleetManagement.ViewModel.Base;
using StyexFleetManagement.Salus.Models;
using StyexFleetManagement.Salus.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StyexFleetManagement.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ICredentialsService _storeService;

        public AuthenticationService()
        {
            _storeService = DependencyService.Get<ICredentialsService>();
        }

        public async Task ProcessMZoneLogin(Guid mZoneId)
        {
            if (string.IsNullOrEmpty(Settings.Current.Server))
            {
                Settings.Current.Server = Constants.DEFAULT_MZONE_SERVER;
            }

            //Configure Settings
            Settings.Current.MzoneUserId = mZoneId;

            var response = await RestService.GetUserSettings(mZoneId);

            var userSettings = (UserSettings) response.ResponseToken;

            Settings.Current.UserDescription = userSettings.Description;
            Settings.Current.Currency = userSettings.CurrencyCode;
            Settings.Current.DistanceMeasurementUnit = userSettings.UnitOfMeasureDistanceId;
            Settings.Current.FluidMeasurementUnit = userSettings.UnitOfMeasureFluidId;
            Settings.Current.UtcOffset = userSettings.UtcOffset;

            //TODO: Clear all settings?

            Settings.Current.LastLoginDate = DateTime.Now;

            await App.InitializeData();

            MessagingCenter.Send(this, AppEvent.LoggedInMZone.ToString());

            App.ShowLoading(false);
        }


        public async Task<bool> LoginMZone(string username, string password)
        {
            var mzoneId = await TryGetMZoneUserId(username, password);

            if (mzoneId != null)
            {
                try
                {
                    var doCredentialsExist = _storeService.DoMZoneCredentialsExist();
                    if (!doCredentialsExist)
                    {
                        await _storeService.SaveMZoneCredentialsAsync(username, password);
                    }

                    await ProcessMZoneLogin(mzoneId.Value);
                    return true;
                }

                catch (InvalidCastException ex)
                {
                    App.ShowLoading(false);
                    Serilog.Log.Debug(ex.Message);
                    throw;
                }
            }

            return false;
        }

        public async Task<bool> LoginSalus(string username, string password)
        {
            var salusUser = await TryGetSalusUser(username, password);

            if (salusUser == null)
            {
                App.ShowLoading(false);
                return false;
            }

            var doCredentialsExist = _storeService.DoSalusCredentialsExist();
            if (!doCredentialsExist)
            {
                await _storeService.SaveSalusCredentialsAsync(username, password);
            }

            Settings.Current.SalusUser = salusUser;
            Settings.Current.LastLoginDate = DateTime.Now;

            await App.InitializeData();

            MessagingCenter.Send(this, AppEvent.LoggedInSalus.ToString());

            App.ShowLoading(false);
            return true;
        }

        public async Task<Guid?> TryGetMZoneUserId(string username, string password)
        {
            var response = await RestService.GetUsersAsync(username, password);

            if (response.ResponseToken != null && response.ErrorStatus == ServiceError.NO_ERROR)
            {
                return ((User) response.ResponseToken).Id;
            }

            return null;
        }

        public async Task<SalusUser> TryGetSalusUser(string username, string password)
        {
            App.ShowLoading(false);
            var permissionService = ViewModelLocator.Resolve<IPermissionsService>();
            if (!await permissionService.GetPhonePermissionAsync()) return null;
            App.ShowLoading(true);

            var salusUserService = new UserService();

            var result =
                await salusUserService.Login(username, password, DependencyService.Get<IDevice>().GetImei());

            return result?.User;
        }

        public bool IsLoggedIn(AccountType accountType)
        {
            if (accountType == AccountType.MZone)
            {
                return !string.IsNullOrEmpty(DependencyService.Get<ICredentialsService>().MZoneUserName) &&
                       !string.IsNullOrEmpty(DependencyService.Get<ICredentialsService>().MZonePassword);
            }
            else
            {
                return !string.IsNullOrEmpty(DependencyService.Get<ICredentialsService>().SalusUserName) &&
                       !string.IsNullOrEmpty(DependencyService.Get<ICredentialsService>().SalusPassword);
            }
        }
    }
}