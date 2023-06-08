using System;
using System.Threading.Tasks;
using StyexFleetManagement.Enums;

namespace StyexFleetManagement.Services
{
    public interface IAuthenticationService
    {
        Task<bool> LoginMZone(string username, string password);
        Task<bool> LoginSalus(string username, string password);
        Task<Guid?> TryGetMZoneUserId(string userName, string password);
        Task ProcessMZoneLogin(Guid mZoneId);
        bool IsLoggedIn(AccountType accountType);
    }
}