using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Models;

namespace StyexFleetManagement.Services
{
    public static class DTCEventAPI
    {
        private const string URL_DTC_BY_VEHICLE = "vehicles/{0}/dtc/{1}/{2}.json";
        private const string URL_DTC_BY_VEHICLEGROUP = "vehiclegroups/{0}/dtc/lastknown.json";

        public static async Task<List<DTCEvent>> GetDTCHistoryForVehicle(string vehicleId, DateTime startDate, DateTime endDate)
        {
            var startDateString = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDateString = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

            var key = "DTC_EVENT: " + vehicleId + startDateString.Substring(0,11) + endDateString.Substring(0, 11);

            var requestUrl = string.Format(URL_DTC_BY_VEHICLE, vehicleId, startDateString, endDateString);
            var response = await RestService.GetServiceResponseFromCacheAsync<DTCEvent>(key, requestUrl);

            try
            {
                return response;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }

        }

        public static async Task<List<DTCEvent>> GetLastDTCForVehicleGroup(string vehicleGroupId)
        {
            var datetring = DateTime.Now.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

            var key = "DTC_EVENT: " + vehicleGroupId + datetring.Substring(0, 11);

            var requestUrl = string.Format(URL_DTC_BY_VEHICLEGROUP, vehicleGroupId);
            var response = await RestService.GetServiceResponseFromCacheAsync<DTCEvent>(key, requestUrl);

            try
            {
                return response;
            }

            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }

        }
    }
}
