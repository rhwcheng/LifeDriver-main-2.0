using StyexFleetManagement.Salus.Models;
using System.Threading.Tasks;

namespace StyexFleetManagement.Salus.Services
{
    public class ServerDetailsService : BaseSalusService, IServerDetailsService
    {
        public async Task<ServerDetailsResponse> GetServerDetails(string deviceId)
        {
            var timestamp = Utils.Timestamp();
            var key = Utils.GetMd5Key(timestamp + deviceId + Constants.EncryptionKey);

            return await GetAsync<ServerDetailsResponse>("server_port", new ServerDetailsRequest()
            {
                DeviceId = deviceId,
                Timestamp = timestamp,
                Key = key
            });
        }
    }
}