using System.Threading.Tasks;
using StyexFleetManagement.Salus.Models;

namespace StyexFleetManagement.Salus.Services
{
    public interface IDeviceService
    {
        Task<DeviceSettingsResponse> GetDeviceSettingsAsync(string userId, string deviceId);
    }
}