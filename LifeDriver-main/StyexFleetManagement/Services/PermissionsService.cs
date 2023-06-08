using System.Threading.Tasks;
using Xamarin.Essentials;

namespace StyexFleetManagement.Services
{
    public class PermissionsService : IPermissionsService
    {
        public async Task<bool> GetPhonePermissionAsync()
        {
            var status = await CheckAndRequestPermissionAsync(new Permissions.Phone());
            return status == PermissionStatus.Granted;
        }

        public async Task<bool> GetLocationPermissionAsync()
        {
            var status = await CheckAndRequestPermissionAsync(new Permissions.LocationWhenInUse());
            return status == PermissionStatus.Granted;
        }

        private async Task<PermissionStatus> CheckAndRequestPermissionAsync<T>(T permission)
            where T : Permissions.BasePermission
        {
            var status = await permission.CheckStatusAsync();
            if (status != PermissionStatus.Granted)
            {
                status = await permission.RequestAsync();
            }

            return status;
        }
    }
}