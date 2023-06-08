using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using StyexFleetManagement.Models;
using Xamarin.Forms;

namespace StyexFleetManagement.Services
{
    public static class AlertsAPI
	{


		private const string URL_ALERT_BY_VEHICLE = "vehicles/{0}/alerts/all/{1}/{2}.json";
        
		public static async Task<List<Alert>> GetAlertsByVehicle(string vehicleId, DateTime startDate, DateTime endDate)
    {

        var startDateStringKey = startDate.ToString("yyyyMMddTHH", CultureInfo.InvariantCulture);
        var endDateStringKey = endDate.ToString("yyyyMMddTHH", CultureInfo.InvariantCulture);

            string key = "ALERTS: " + DateTime.Now.ToString("yyyyMMddTHH") + ":" + vehicleId + ":" + startDateStringKey + "-" + endDateStringKey;


        var startDateString = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDateString = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

        string requestUrl = string.Format(URL_ALERT_BY_VEHICLE, vehicleId, startDateString, endDateString);

        var alerts = await RestService.GetServiceResponseFromCacheAsync<Alert>(key, requestUrl);

        return alerts;
    }


		
    }


}

