using System;
using StyexFleetManagement.Salus.Enums;
using StyexFleetManagement.Salus.Extensions;
using StyexFleetManagement.Salus.Models;
using System.Threading.Tasks;

namespace StyexFleetManagement.Salus.Services
{
    public class UserService : BaseSalusService
    {
        public async Task<LoginResponse> Login(string username, string password, string imei)
        {
            if (string.IsNullOrEmpty(imei))
            {
                imei = "imei_not_allowed";
            }

            var timestamp = Utils.Timestamp();
            var key = Utils.GetMd5Key(timestamp + username + password + Constants.EncryptionKey);

            //TODO: FCM token

            try
            {
                var result = await PostAsync<LoginResponse>("login", new LoginRequest()
                {
                    UserId = username,
                    Password = password,
                    Imei = imei,
                    DeviceToken = "", //TODO
                    DeviceType = Utils.DeviceType(),
                    Key = key,
                    Timestamp = timestamp,
                    LanguageCode = Language.English.ToDescriptionString()
                });
                return result;
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, e.Message);
                return null;
            }
        }
    }
}