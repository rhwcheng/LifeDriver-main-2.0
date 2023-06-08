using StyexFleetManagement.Salus.Enums;
using StyexFleetManagement.Salus.Models;
using System.Threading.Tasks;

namespace StyexFleetManagement.Salus.Services
{
    public interface IEventService
    {
        Task<StatusMessage> LogEventAsync(string deviceId, EventType eventType, double latitude, double longitude,
            string address);
    }
}