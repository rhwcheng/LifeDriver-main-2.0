using System.Threading.Tasks;

namespace StyexFleetManagement.Services
{
    public interface IPermissionsService
    {
        Task<bool> GetPhonePermissionAsync();
        Task<bool> GetLocationPermissionAsync();
    }
}