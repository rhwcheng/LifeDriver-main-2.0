using StyexFleetManagement.Salus.Models;
using System.Threading.Tasks;

namespace StyexFleetManagement.Salus.Services
{
    public class DeviceService : BaseSalusService, IDeviceService
    {
        public async Task<DeviceSettingsResponse> GetDeviceSettingsAsync(string userId, string deviceId)
        {
            var timestamp = Utils.Timestamp();
            var key = Utils.GetMd5Key(timestamp + userId + deviceId + Constants.EncryptionKey);

            return await GetAsync<DeviceSettingsResponse>("device_setting", new DeviceSettingsRequest()
            {
                UserId = userId,
                DeviceId = deviceId,
                Timestamp = timestamp,
                Key = key
            });
        }
    }
}