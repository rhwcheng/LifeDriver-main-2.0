using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Akavache;
using StyexFleetManagement.Models;
using Xamarin.Forms;

namespace StyexFleetManagement.Services
{
	public class VehicleAPI
	{
		private const string URL_VEHICLEGROUPS = "vehiclegroups.json";
        private const string URL_VEHICLES = "vehicles.json";
        private const string URL_VEHICLE_GROUP_LIST = "vehiclegroups/{0}/vehicles.json";
        private const string URL_LASTKNOWNLOCATIONS = "vehiclegroups/{0}/lastknownpositions.json";
        private const string URL_LAST_LOCATION = "vehicles/{0}/lastknownposition.json";
        private const string URL_VEHICLE_BY_ID = "vehicle/{0}/vehicle.json";
        private const string URL_TRIPS = "vehicles/{0}/trips.json?ps=20";
		private const string URL_POLL = "vehicles/{0}/poll.json";
		private const string URL_ICON = "vehicles/{0}_128x128.png";
        private const string URL_DRIVERS = "drivers.json";

        private const string URL_EVENTS = "vehiclegroups/{0}/events/{1}/{2}.json?ps={3}";

        public static async Task<LastKnownPositions> GetLastKnownPositions(string vehicleGroupId)
		{
			var requestUrl = string.Format(URL_LASTKNOWNLOCATIONS, vehicleGroupId);
			var response = await RestService.GetServiceResponse<LastKnownPositions>(requestUrl);

			try
			{
				return (LastKnownPositions)response.ResponseToken;
			}
			catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
			}
			/*
			var username = DependencyService.Get<ICredentialsService>().UserName;
			var password = DependencyService.Get<ICredentialsService>().Password;
			HttpClientHandler handler = new HttpClientHandler() { Credentials = new NetworkCredential(username, password) };

			// ... Use HttpClient.            
			HttpClient client = new HttpClient(handler);

			//var byteArray = new UTF8Encoding().GetBytes(username + ":" + password);
			//client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

			HttpResponseMessage response = await client.GetAsync("http://us.mzoneweb.net/api/v2/vehiclegroups/" + vehicleGroupId + "/lastknownpositions.json");

			//HttpContent content = response.Content;


			// Read Json

			if (response.IsSuccessStatusCode)
			{
				string jsonMessage;
				using (Stream responseStream = await response.Content.ReadAsStreamAsync())
				{
					jsonMessage = new StreamReader(responseStream).ReadToEnd();
				}

				var lastKnownPositionsToken = (LastKnownPositions)JsonConvert.DeserializeObject(jsonMessage, typeof(LastKnownPositions));

				return lastKnownPositionsToken;
			}

			else
			{
				return null;
			}*/
		}

        public static async Task<UnitLocation> GetVehicleLastKnownPosition(string vehicleId)
        {
            var requestUrl = string.Format(URL_LAST_LOCATION, vehicleId);
            var response = await RestService.GetServiceResponse<UnitLocation>(requestUrl);

            try
            {
                return (UnitLocation)response.ResponseToken;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }
        }

        internal static async Task<ObservableCollection<Vehicle>> GetAllVehicles(List<VehicleGroup> vehicleGroups)
        {
            ObservableCollection<Vehicle> allVehicles = new ObservableCollection<Vehicle>();
            if (vehicleGroups.Count == 0)
                return allVehicles;
            
            using (var loading = UserDialogs.Instance.Progress("Fetching Vehicles", null, null, true, MaskType.Black))
            {
                var totalCount = vehicleGroups.Count;
                var iter = 0;
                vehicleGroups = vehicleGroups.OrderByDescending(v => v.Description).ToList();
                foreach (var vehicleGroup in vehicleGroups)
                {
                    var progress = (iter * 100) / totalCount;
                    if (progress > 0)
                        loading.PercentComplete = progress;
                    List<VehicleItem> vehicles = await VehicleAPI.GetVehicleInGroupAsync(vehicleGroup.Id);
                    iter++;

                    foreach (var vehicle in vehicles)
                    {
                        if (!allVehicles.Any(x => x.Description == vehicle.Description))
                        {
                            allVehicles.Add(new Vehicle()
                            {
                                VehicleGroupName = vehicleGroup.Description,
                                Id = Guid.Parse(vehicle.Id),
                                Description = vehicle.Description
                            });
                        }
                    }

                }
            }
            return allVehicles;
        }

        private static async Task<byte[]> GetVehicleImage(string vehicleId)
        {
            
            var requestUrl = string.Format(URL_ICON, vehicleId);
            var response = await RestService.GetImageResponse(requestUrl);

            return response;
        }

        public static async Task<UnitLocation> GetLastKnownLocationById(string vehicleId)
        {

            var requestUrl = string.Format(URL_LAST_LOCATION, vehicleId);
            var response = await RestService.GetServiceResponse<UnitLocation>(requestUrl);

            return (UnitLocation)response.ResponseToken;
        }

        public static async Task<byte[]> GetVehicleImage(string vehicleId, bool fromCache=true)
        {
            if (fromCache)
            {
                var image = await BlobCache.LocalMachine.GetOrFetchObject($"{"ICON"}: {vehicleId}", async () => await GetVehicleImage(vehicleId));

                return image;
            }
            else
                return await GetVehicleImage(vehicleId);
        }

        public static async Task<VehicleTrips> GetTripsAsync(Guid vehicleGroupId)
		{
			var requestUrl = string.Format(URL_TRIPS, vehicleGroupId);
			var response = await RestService.GetServiceResponse<VehicleTrips>(requestUrl);

			try
			{
				return (VehicleTrips)response.ResponseToken;
			}
			catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
			}
		}

        public static async Task<List<VehicleItem>> GetVehiclesAsync(bool fromCache)
        {
            var cache = BlobCache.LocalMachine;
            List<VehicleItem> vehicles = new List<VehicleItem>();
            try
            {
                vehicles = await cache.GetObject<List<VehicleItem>>("Vehicles");
            }
            catch (System.Collections.Generic.KeyNotFoundException keyNotFound)
            {
                try
                {
                    vehicles = await GetVehiclesAsync();
                    await cache.InsertObject<List<VehicleItem>>("Vehicles", vehicles);

                }
                catch (NullReferenceException e)
                {
                    Serilog.Log.Debug(e.Message);
                }
            }

            return vehicles;
        }
        public static async Task<List<VehicleItem>> GetVehicleInGroupAsync(string vehicleGroup)
        {
            var cache = BlobCache.LocalMachine;
            string key = vehicleGroup + ":Vehicles";
            List<VehicleItem> vehicles = new List<VehicleItem>();
            try
            {
                vehicles = await cache.GetObject<List<VehicleItem>>(key);
            }
            catch (System.Collections.Generic.KeyNotFoundException keyNotFound)
            {
                try
                {
                    var requestUrl = string.Format(URL_VEHICLE_GROUP_LIST, vehicleGroup);
                    var response = await RestService.GetServiceResponse<VehiclesResponse>(requestUrl);

                    try
                    {
                        vehicles = (List<VehicleItem>)((VehiclesResponse)response.ResponseToken).Items;
                    }
                    catch (InvalidCastException e)
                    {
                        Serilog.Log.Debug(e.Message);
                        return null;
                    }
                    await cache.InsertObject<List<VehicleItem>>(key, vehicles);

                }
                catch (NullReferenceException e)
                {
                    Serilog.Log.Debug(e.Message);
                }
            }

            return vehicles;
        }
        public static async Task<List<Driver>> GetDriversAsync()
        {
            var key = "Drivers";
            var requestUrl = URL_DRIVERS;

            return await GetServiceResponseAsync<Driver>(key, requestUrl);

        }

        public static async Task<ExtendedVehicle> GetVehicleById(string vehicleId)
        {
            var key = "Extended_Vehicle:"+vehicleId;

            var cache = BlobCache.LocalMachine;
            bool inCache = false;

            ExtendedVehicle vehicle;
            try
            {
                vehicle = await cache.GetObject<ExtendedVehicle>(key);

                if (vehicle != null)
                {
                    inCache = true;
                    return vehicle;
                }
                else
                    inCache = false;

            }
            catch (System.Collections.Generic.KeyNotFoundException keyNotFound)
            {
                inCache = false;
            }

            if (!inCache)
            {
                try
                {
                    var requestUrl = string.Format(URL_VEHICLE_BY_ID, vehicleId);
                    var response = await RestService.GetServiceResponse<ExtendedVehicle>(requestUrl);

                    try
                    {
                        vehicle = (ExtendedVehicle)(response.ResponseToken);
                    }
                    catch (InvalidCastException e)
                    {
                        Serilog.Log.Debug(e.Message);
                        return null;
                    }
                    await cache.InsertObject<ExtendedVehicle>(key, vehicle);
                    return vehicle;

                }
                catch (NullReferenceException e)
                {
                    Serilog.Log.Debug(e.Message);
                }
            }


            return null;

        }
        public static async Task<List<T>> GetServiceResponseAsync<T>(string key, string requestUrl)
        {
            var cache = BlobCache.LocalMachine;
            bool inCache = false;

            List<T> items = new List<T>();
            try
            {
                items = await cache.GetObject<List<T>>(key);

                if (items != null)
                {
                    inCache = true;
                    return items;
                }
                else
                    inCache = false;

            }
            catch (System.Collections.Generic.KeyNotFoundException keyNotFound)
            {
                inCache = false;
            }

            if (!inCache)
            {
                try
                {
                    var response = await RestService.GetServiceResponse<APIResponse<T>>(requestUrl);

                    try
                    {
                        items = (List<T>)((APIResponse<T>)response.ResponseToken).Items;
                    }
                    catch (InvalidCastException e)
                    {
                        Serilog.Log.Debug(e.Message);
                        return null;
                    }
                    await cache.InsertObject<List<T>>(key, items);
                    return items;

                }
                catch (NullReferenceException e)
                {
                    Serilog.Log.Debug(e.Message);
                }
            }
            

            return null;
        }


        public static async Task<List<VehicleItem>> GetVehicles()
        {
            var username = DependencyService.Get<ICredentialsService>().MZoneUserName;
            List<VehicleItem> vehicles = await BlobCache.LocalMachine.GetOrFetchObject($"{"VEHICLES:"}: {username}", async () => await GetVehiclesAsync());

            return vehicles;
            
        }
        public static async Task<List<VehicleItem>> GetVehiclesAsync()
        {
            var key = "AllVehicles";
            var requestUrl = URL_VEHICLES;
            var response = await RestService.GetServiceResponseFromCacheAsync<VehicleItem>(key,requestUrl);

            return response;
           
        }

        public static async Task<List<Event>> GetLastEventAsync(string vehicleGroup, int page = 0)
        {
            //TODO: FINISH
            var startDate = DateTime.Now.AddDays(-3).ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDate = DateTime.Now.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var requestUrl = string.Format(URL_EVENTS, vehicleGroup, startDate, endDate, page);
            var response = await RestService.GetServiceResponse<Events>(requestUrl);

            try
            {
                Events responseToken = (Events)response.ResponseToken;
                if (!(responseToken.TotalResults > responseToken.PageSize))
                    return responseToken.Items;
                else
                {
                    decimal count = (decimal)responseToken.TotalResults / responseToken.PageSize;
                    var pages = Math.Ceiling(count);

                    for (int i = 1; i < 5; i++)
                    {
                        var url = string.Format(URL_EVENTS, vehicleGroup, startDate, endDate, i);
                        var resp = await RestService.GetServiceResponse<Events>(url);
                        responseToken.Items.AddRange(((Events)resp.ResponseToken).Items);
                    }
                    return responseToken.Items;
                }
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }
        }


        }
}

