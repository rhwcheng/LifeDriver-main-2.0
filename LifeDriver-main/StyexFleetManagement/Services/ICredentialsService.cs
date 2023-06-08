using System.Threading.Tasks;

namespace StyexFleetManagement.Services
{
    public interface ICredentialsService
    {
        string MZoneUserName { get; }
        string MZonePassword { get; }
        Task SaveMZoneCredentialsAsync(string userName, string password);
        bool DoMZoneCredentialsExist();
        string SalusUserName { get; }
        string SalusPassword { get; }
        Task SaveSalusCredentialsAsync(string userName, string password);
        bool DoSalusCredentialsExist();
        void DeleteCredentials();
    }
}