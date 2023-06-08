using System.Threading.Tasks;
using StyexFleetManagement.Salus.Models;

namespace StyexFleetManagement.Salus.Services
{
    public interface IServerDetailsService
    {
        Task<ServerDetailsResponse> GetServerDetails(string deviceI);
    }
}