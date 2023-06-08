using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Services;

namespace StyexFleetManagement.Helpers
{
    public static class VehicleHelper
    {
        public static async Task<string> GetVehicleDescriptionFromIdAsync(string vehicleId, string vehicleGroup)
        {

            var vehicles = await VehicleAPI.GetVehicleInGroupAsync(vehicleGroup);
            string vehicleDesciption = vehicles.Where(x => x.Id == vehicleId).First().Description;
            return vehicleDesciption;
        }
    }
}
