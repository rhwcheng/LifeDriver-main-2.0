using System.Threading.Tasks;
using StyexFleetManagement.Salus.Enums;

namespace StyexFleetManagement.Services
{
    public interface ILocationUpdateService
    {
        Task GetLocationAndSendEvent(EventType eventType, bool showDialog = false);
    }
}