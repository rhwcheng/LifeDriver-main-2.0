using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Models;

namespace StyexFleetManagement.Services
{
    public static class FuelAPI
    {
        //Fuel API
        // {0}: vehicleGroupId, {1}: startDate, {2}: endDate, {3}: group by
        private const string URL_FUELSUMMARY = "vehiclegroups/{0}/fuelsummary/{1}/{2}.json";
        private const string URL_FUELVOLUME = "vehiclegroups/{0}/fuelvolume/{3}/{1}/{2}.json";
        private const string URL_FUELCONSUMPTION = "vehiclegroups/{0}/fuelconsumption/{3}/{1}/{2}.json";
        private const string URL_FUELCOST = "vehiclegroups/{0}/fuelcost/{3}/{1}/{2}.json";
        private const string URL_MONTHLYFUELCONSUMPTION = "vehicles/{0}/monthlyfuelconsumption.json";

        public static async Task<List<ConsumptionItem>> GetFuelConsumptionAsync(string vehicleGroupId, DateTime startDate, DateTime endDate, string groupBy)
        {
            var key = "FUEL_CONSUMPTION: " + vehicleGroupId + startDate + endDate;

            var startDateString = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDateString = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

            var requestUrl = string.Format(URL_FUELCONSUMPTION, vehicleGroupId, startDateString, endDateString, groupBy);
            var response = await RestService.GetServiceResponseFromCacheAsync<ConsumptionItem>(key, requestUrl);

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
        public static async Task<List<MonthlyFuelConsumption>> GetMonthlyFuelConsumptionAsync(string vehicleGroupId)
        {
            var key = "MONTHLY_FUEL_CONSUMPTION: " + vehicleGroupId + DateTime.Now.ToString("yyyyMMddHH");
            
            var requestUrl = string.Format(URL_MONTHLYFUELCONSUMPTION, vehicleGroupId);
            var response = await RestService.GetServiceResponseFromCacheAsync<MonthlyFuelConsumption>(key, requestUrl);

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
        public static async Task<List<ConsumptionItem>> GetFuelVolumeAsync(string vehicleGroupId, DateTime startDate, DateTime endDate, string groupBy)
        {

            var startDateString = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDateString = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

            var key = "FUEL_VOLUME: " + vehicleGroupId + startDateString.Substring(0,11) + endDateString.Substring(0, 11);

            var requestUrl = string.Format(URL_FUELVOLUME, vehicleGroupId, startDateString, endDateString, groupBy);
            var response = await RestService.GetServiceResponseFromCacheAsync<ConsumptionItem>(key, requestUrl);

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
        public static async Task<List<ConsumptionItem>> GetFuelCostAsync(string vehicleGroupId, DateTime startDate, DateTime endDate, string groupBy)
        {

            var startDateString = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDateString = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

            var key = "FUEL_COST: " + vehicleGroupId + startDateString.Substring(0, 11) + endDateString.Substring(0, 11);

            var requestUrl = string.Format(URL_FUELCOST, vehicleGroupId, startDateString, endDateString, groupBy);
            var response = await RestService.GetServiceResponseFromCacheAsync<ConsumptionItem>(key, requestUrl);

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
