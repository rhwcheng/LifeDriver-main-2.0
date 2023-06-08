using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.MProfiler.Api;
using StyexFleetManagement.MProfiler.Entities;

namespace StyexFleetManagement.Services
{
    public static class MProfiler
    {
        public async static Task<bool> SendLogbookTrip(byte[] trip, uint templateId, string unitId)
        {
            try
            {


                //Logger.Info(CultureInfo.InvariantCulture, "NonTripPosition Event Data: {0}", Utils.AsHexString(eventData));

                var apiClient = new ThirdPartyGatewayApiClient();
                var messageBatch = new MessageBatch();


                messageBatch.AddMessage(new Message
                {
                    UnitId = unitId,
                    TemplateId = templateId,
                    Data = trip
                });

                SendMessagesResult result = await apiClient.SendMessages(messageBatch);
                if (result.Success)
                {
                    bool allMessagesSent = result.Results.All(v => v.Result.Equals("OK", StringComparison.OrdinalIgnoreCase));
                    if (allMessagesSent)
                    {
                        return true;
                        //Logger.Info(CultureInfo.InvariantCulture, "All messages have been sent: {0}", result);
                    }
                    else
                    {
                        return false;
                        //Logger.Warn(CultureInfo.InvariantCulture, "Not all messages have been sent: {0}", result);
                    }
                }
                else
                {
                    return false;
                    //Logger.Warn(CultureInfo.InvariantCulture, "Messages were not sent: {0}", result);
                }
            }
            catch (Exception e)
            {
                return false;
            }

        }
    }
}
