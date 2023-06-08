using System.Globalization;
using System.Threading.Tasks;
using StyexFleetManagement.Salus.Enums;
using StyexFleetManagement.Salus.Extensions;
using StyexFleetManagement.Salus.Models;

namespace StyexFleetManagement.Salus.Services
{
    public class EventService : BaseSalusService, IEventService
    {

        public async Task<StatusMessage> LogEventAsync(string deviceId, EventType eventType, double latitude, double longitude, string address)
        {
            var timestamp = Utils.Timestamp();
            var key = Utils.GetMd5Key(timestamp + deviceId + Constants.EncryptionKey);

            return await PostAsync<LoginResponse>("log_event", new LogEventRequest()
            {
                DeviceId = deviceId,
                EventType = eventType.ToDescriptionString(),
                Latitude = latitude.ToString(CultureInfo.InvariantCulture),
                Longitude = longitude.ToString(CultureInfo.InvariantCulture),
                Address = address,
                Key = key,
                Timestamp = timestamp,
                LanguageCode = Language.English.ToDescriptionString()
            });
        }
    }
}