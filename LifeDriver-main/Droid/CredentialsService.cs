using StyexFleetManagement.Droid;
using StyexFleetManagement.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(CredentialsService))]

namespace StyexFleetManagement.Droid
{
    public class CredentialsService : ICredentialsService
    {
        public string MZoneUserName => SecureStorage.GetAsync(nameof(MZoneUserName)).Result;
        public string MZonePassword => SecureStorage.GetAsync(nameof(MZonePassword)).Result;
        public string SalusUserName => SecureStorage.GetAsync(nameof(SalusUserName)).Result;
        public string SalusPassword => SecureStorage.GetAsync(nameof(SalusPassword)).Result;

        public async Task SaveMZoneCredentialsAsync(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            await SecureStorage.SetAsync(nameof(MZoneUserName), userName);
            await SecureStorage.SetAsync(nameof(MZonePassword), password);
        }

        public async Task SaveSalusCredentialsAsync(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                throw new ArgumentNullException(nameof(userName));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentNullException(nameof(password));
            }

            await SecureStorage.SetAsync(nameof(SalusUserName), userName);
            await SecureStorage.SetAsync(nameof(SalusPassword), password);
        }

        public void DeleteCredentials()
        {
            SecureStorage.Remove(nameof(MZoneUserName));
            SecureStorage.Remove(nameof(MZonePassword));
            SecureStorage.Remove(nameof(SalusUserName));
            SecureStorage.Remove(nameof(SalusPassword));
        }

        public bool DoMZoneCredentialsExist()
        {
            return !string.IsNullOrEmpty(SecureStorage.GetAsync(nameof(MZoneUserName)).Result);
        }

        public bool DoSalusCredentialsExist()
        {
            return !string.IsNullOrEmpty(SecureStorage.GetAsync(nameof(SalusUserName)).Result);
        }
    }
}