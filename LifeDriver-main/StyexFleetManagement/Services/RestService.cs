using Akavache;
using FFImageLoading;
using FFImageLoading.Config;
using FFImageLoading.Forms;
using Newtonsoft.Json;
using StyexFleetManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StyexFleetManagement.Services
{
    public static class RestService
    {
        public static TimeSpan TIMEOUT = TimeSpan.FromMilliseconds(30000);

        //Users
        private const string URL_USERS_SELF = "users/self.json";
        private const string URL_USERS_SETTINGS = "users/{0}.json";

        //Trips
        private const string URL_POSITIONS = "trips/{0}/positions.json";
        private const string URL_EXCEPTIONS = "trips/{0}/exceptions.json";
        private const string URL_TRIPSUMMARY = "vehiclegroups/{0}/trips/{1}/{2}/summary.json";

        //Utilization
        // {0}: vehicleGroupId, {1}: startDate, {2}: endDate
        private const string URL_DRIVING_SUMMARY = "vehiclegroups/{0}/drivingtotal/{1}/{2}.json";
        private const string URL_UTILIZATION_SUMMARY = "vehiclegroups/{0}/percentofutilization/{1}/{2}.json";
        private const string URL_TIME_PROFILE_UTILIZATION = "vehiclegroups/{0}/timeprofileutilization/{1}/{2}.json";
        private const string URL_OVERTIME_UTILIZATION = "vehiclegroups/{0}/timeprofilesummary/{1}/{2}.json";

        //Vehicle
        private const string URL_VEHICLEGROUPS = "vehiclegroups.json";
        private const string URL_LASTKNOWNLOCATIONS = "vehiclegroups/{0}/lastknownpositions.json";
        private const string URL_TRIPS = "vehicles/{0}/trips.json?ps=20";
        private const string URL_POLL = "vehicles/{0}/poll.json";
        private const string URL_ICON = "vehicles/{0}_32x32.png";

        //Fuel API
        // {0}: vehicleGroupId, {1}: startDate, {2}: endDate, {3}: group by
        private const string URL_FUELSUMMARY = "vehiclegroups/{0}/fuelsummary/{1}/{2}.json";
        private const string URL_FUELVOLUME = "vehiclegroups/{0}/fuelvolume/{3}/{1}/{2}.json";
        private const string URL_FUELCONSUMPTION = "vehiclegroups/{0}/fuelconsumption/{3}/{1}/{2}.json";
        private const string URL_FUELCOST = "vehiclegroups/{0}/fuelcost/{3}/{1}/{2}.json";

        public static async Task<Response> GetServiceResponse<T>(string requestUrl)
        {
            var serviceResponse = new Response();

            if (App.Singleton.HttpClient == null)
            {
                var username = DependencyService.Get<ICredentialsService>().MZoneUserName;
                var password = DependencyService.Get<ICredentialsService>().MZonePassword;

                var handler = new HttpClientHandler() {Credentials = new NetworkCredential(username, password)};

                App.Singleton.HttpClient = new HttpClient(handler)
                {
                    Timeout = TIMEOUT
                };
            }

            var client = App.Singleton.HttpClient;

            var url = Constants.SERVICE_URL_PREFIX + Settings.Current.Server + Constants.SERVICE_API_URL + requestUrl;

            try
            {
                var response = await client.GetAsync(url);
                // Read Json

                if (response.IsSuccessStatusCode)
                {
                    serviceResponse.ErrorStatus = ServiceError.NO_ERROR;

                    string jsonMessage;
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        jsonMessage = new StreamReader(responseStream).ReadToEnd();
                    }

                    var settings = new JsonSerializerSettings();
                    settings.DateParseHandling = DateParseHandling.DateTimeOffset;
                    settings.NullValueHandling = NullValueHandling.Ignore;
                    settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    settings.MissingMemberHandling = MissingMemberHandling.Ignore;

                    var token = (T) JsonConvert.DeserializeObject(jsonMessage, typeof(T), settings);

                    //var property = token.Items.GetType().GetRuntimeProperties().FirstOrDefault(p => string.Equals(p.Name, "DateTimeOffset", StringComparison.OrdinalIgnoreCase)); ;

                    //if (property != null)
                    //{
                    //    var dateTime = ((DateTimeOffset)property.GetValue(token, null));
                    //    dateTime = new DateTimeOffset(dateTime.DateTime, TimeSpan.FromHours(2));
                    //    property.SetValue(token, dateTime, null);
                    //}

                    serviceResponse.ResponseToken = (T) token;
                    if (token.Equals(null))
                    {
                        serviceResponse.ErrorStatus = ServiceError.NO_DATA;
                    }

                    //client.Dispose();
                    return serviceResponse;
                }

                else
                {
                    serviceResponse.ErrorStatus = (ServiceError) response.StatusCode;
                    //client.Dispose();
                    return serviceResponse;
                }
            }
            catch (TaskCanceledException ex)
            {
                //Xamarin.Insights.Report(ex);
                //client.Dispose();
                if (!ex.CancellationToken.IsCancellationRequested)
                {
                    serviceResponse.ErrorStatus = ServiceError.SERVER_CONNECTION_ERROR;
                    return serviceResponse;
                }
                else
                {
                    serviceResponse.ErrorStatus = ServiceError.UNKNOWN_SERVICE_ERROR;
                    return serviceResponse;
                }
            }
            catch (HttpRequestException e)
            {
                Serilog.Log.Debug(e.Message);
                //client.Dispose();
                if (e.Message.Equals("Unauthorized"))
                {
                    serviceResponse.ErrorStatus = ServiceError.AUTHENTICATION_ERROR;
                }
                else
                {
                    serviceResponse.ErrorStatus = ServiceError.UNKNOWN_SERVICE_ERROR;
                }

                return serviceResponse;
            }
            catch (NullReferenceException e)
            {
                Serilog.Log.Debug(e.Message);
                return serviceResponse;
            }
            catch (Exception e)
            {
                Serilog.Log.Debug(e.Message);
                //client.Dispose();
                serviceResponse.ErrorStatus = ServiceError.SERVER_CONNECTION_ERROR;
                System.Diagnostics.Debug.WriteLine("Exception at RestClientService.executeRequest(): " + e.Message);
                return serviceResponse;
            }
        }

        public static async Task<List<T>> GetServiceResponseFromCacheAsync<T>(string key, string requestUrl,
            bool enablePagination = true, string alternateKey = null)
        {
            var cache = BlobCache.LocalMachine;
            var inCache = false;

            key += enablePagination.ToString();

            var items = new List<T>();
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
            catch (NullReferenceException e)
            {
                await cache.InvalidateObject<List<T>>(key);
                inCache = false;
            }

            if (!inCache)
            {
                var repsonseItems = new List<T>();
                var pg = 0;
                var ps = 1000;
                try
                {
                    do
                    {
                        string url;
                        if (requestUrl.Substring(Math.Max(0, requestUrl.Length - 4)) == "json")
                            url = $"{requestUrl}?pg={pg.ToString()}";
                        else
                            url = $"{requestUrl}&pg={pg.ToString()}";

                        var response = await RestService.GetServiceResponse<APIResponse<T>>(url);

                        try
                        {
                            repsonseItems = (List<T>) ((APIResponse<T>) response.ResponseToken).Items;
                            items.AddRange(repsonseItems);
                            pg += 1;
                            if (!enablePagination)
                                break;
                        }
                        catch (InvalidCastException e)
                        {
                            Serilog.Log.Debug(e.Message);
                            return null;
                        }
                    } while (repsonseItems.Count == 1000);

                    if (items != null && items.Count > 0)
                    {
                        await cache.InsertObject<List<T>>(key, items);
                        if (alternateKey != null)
                            await cache.InsertObject<List<T>>(alternateKey, items);
                    }

                    return items;
                }
                catch (NullReferenceException e)
                {
                    Serilog.Log.Debug(e.Message);
                }
            }


            return null;
        }


        public static async Task<Response> GetHttpResponse<T>(string requestUrl)
        {
            var serviceResponse = new Response();


            var handler = new HttpClientHandler();

            var client = new HttpClient(handler);
            client.Timeout = TIMEOUT;


            try
            {
                var response = await client.GetAsync(requestUrl);
                // Read Json

                if (response.IsSuccessStatusCode)
                {
                    serviceResponse.ErrorStatus = ServiceError.NO_ERROR;

                    string jsonMessage;
                    using (var responseStream = await response.Content.ReadAsStreamAsync())
                    {
                        jsonMessage = new StreamReader(responseStream).ReadToEnd();
                    }

                    var settings = new JsonSerializerSettings();
                    settings.DateParseHandling = DateParseHandling.DateTimeOffset;
                    settings.DateFormatString = "yyyy-MM-ddTH:mm:sszzz";

                    var token = (T) JsonConvert.DeserializeObject(jsonMessage, typeof(T), settings);
                    serviceResponse.ResponseToken = (T) token;
                    if (token.Equals(null))
                    {
                        serviceResponse.ErrorStatus = ServiceError.NO_DATA;
                    }

                    return serviceResponse;
                }

                else
                {
                    serviceResponse.ErrorStatus = (ServiceError) response.StatusCode;
                    return serviceResponse;
                }
            }
            catch (TaskCanceledException ex)
            {
                //Xamarin.Insights.Report(ex);
                //client.Dispose();
                if (!ex.CancellationToken.IsCancellationRequested)
                {
                    serviceResponse.ErrorStatus = ServiceError.SERVER_CONNECTION_ERROR;
                    return serviceResponse;
                }
                else
                {
                    serviceResponse.ErrorStatus = ServiceError.UNKNOWN_SERVICE_ERROR;
                    return serviceResponse;
                }
            }
            catch (HttpRequestException e)
            {
                Serilog.Log.Debug(e.Message);
                //client.Dispose();
                if (e.Message.Equals("Unauthorized"))
                {
                    serviceResponse.ErrorStatus = ServiceError.AUTHENTICATION_ERROR;
                }
                else
                {
                    serviceResponse.ErrorStatus = ServiceError.UNKNOWN_SERVICE_ERROR;
                }

                return serviceResponse;
            }
            catch (Exception e)
            {
                Serilog.Log.Debug(e.Message);
                //client.Dispose();
                serviceResponse.ErrorStatus = ServiceError.SERVER_CONNECTION_ERROR;
                System.Diagnostics.Debug.WriteLine("Exception at RestClientService.executeRequest(): " + e.Message);
                return serviceResponse;
            }
        }

        public static async Task<byte[]> GetImageResponse(string requestUrl)
        {
            var serviceResponse = new Response();

            var username = DependencyService.Get<ICredentialsService>().MZoneUserName;
            var password = DependencyService.Get<ICredentialsService>().MZonePassword;

            var handler = new HttpClientHandler() {Credentials = new NetworkCredential(username, password)};

            // ... Use HttpClient.            
            var client = new HttpClient(handler);
            client.Timeout = TIMEOUT;

            var url = Constants.SERVICE_URL_PREFIX + Settings.Current.Server + Constants.SERVICE_API_URL + requestUrl;

            try
            {
                var response = await client.GetAsync(url);
                // Read Json

                if (response.IsSuccessStatusCode)
                {
                    var imageByteArray = await response.Content.ReadAsByteArrayAsync();

                    var token = imageByteArray;

                    return token;
                }

                else
                {
                    return null;
                }
            }
            catch (TaskCanceledException ex)
            {
                //Xamarin.Insights.Report(ex);
                //client.Dispose();
                return null;
            }
            catch (HttpRequestException e)
            {
                Serilog.Log.Debug(e.Message);
                //client.Dispose();
                return null;
            }
            catch (Exception e)
            {
                Serilog.Log.Debug(e.Message);
                //client.Dispose();
                return null;
            }
        }


        public static async Task<Response> GetUserSettings(Guid id)
        {
            var requestUrl = string.Format(URL_USERS_SETTINGS, id);
            var response = await GetServiceResponse<UserSettings>(requestUrl);

            return response;
        }


        public static async Task<Response> GetUsersAsync(string username, string password)
        {
            var serviceResponse = new Response();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                serviceResponse.ErrorStatus = ServiceError.MISSING_CREDENTIALS_ERROR;
                return serviceResponse;
            }

            using (var client = new HttpClient())
            {
                client.Timeout = TIMEOUT;

                //HttpContent content = response.Content;
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get,
                        $"https://{Settings.Current.Server}/api/v3/users/self.json");
                    request.Headers.Accept.Clear();
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(new UTF8Encoding().GetBytes($"{username}:{password}")));
                    var response = await client.SendAsync(request, CancellationToken.None);

                    if (response.IsSuccessStatusCode)
                    {
                        serviceResponse.ErrorStatus = ServiceError.NO_ERROR;

                        string jsonMessage;
                        using (var responseStream = await response.Content.ReadAsStreamAsync())
                        {
                            jsonMessage = new StreamReader(responseStream).ReadToEnd();
                        }

                        var userToken = (User) JsonConvert.DeserializeObject(jsonMessage, typeof(User));
                        serviceResponse.ResponseToken = (User) userToken;
                        if (userToken.Equals(null))
                        {
                            serviceResponse.ErrorStatus = ServiceError.NO_DATA;
                        }

                        return serviceResponse;
                    }

                    else
                    {
                        serviceResponse.ErrorStatus = (ServiceError) response.StatusCode;
                        return serviceResponse;
                    }
                }
                catch (TaskCanceledException ex)
                {
                    //Xamarin.Insights.Report(ex);
                    //client.Dispose();
                    if (!ex.CancellationToken.IsCancellationRequested)
                    {
                        serviceResponse.ErrorStatus = ServiceError.SERVER_CONNECTION_ERROR;
                        return serviceResponse;
                    }
                    else
                    {
                        serviceResponse.ErrorStatus = ServiceError.UNKNOWN_SERVICE_ERROR;
                        return serviceResponse;
                    }
                }
                catch (HttpRequestException e)
                {
                    Serilog.Log.Debug(e.Message);
                    //client.Dispose();
                    if (e.Message.Equals("Unauthorized"))
                    {
                        serviceResponse.ErrorStatus = ServiceError.AUTHENTICATION_ERROR;
                    }
                    else
                    {
                        serviceResponse.ErrorStatus = ServiceError.UNKNOWN_SERVICE_ERROR;
                    }

                    return serviceResponse;
                }
                catch (Exception e)
                {
                    Serilog.Log.Debug(e.Message);
                    //client.Dispose();
                    serviceResponse.ErrorStatus = ServiceError.SERVER_CONNECTION_ERROR;
                    System.Diagnostics.Debug.WriteLine("Exception at RestClientService.executeRequest(): " + e.Message);
                    return serviceResponse;
                }
            }
        }

        public static async Task<VehicleGroupCollection> GetVehicleGroupsAsync()
        {
            var requestUrl = URL_VEHICLEGROUPS;
            var response = await GetServiceResponse<VehicleGroupCollection>(requestUrl);

            try
            {
                return (VehicleGroupCollection) response.ResponseToken;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }

            /*var username = DependencyService.Get<ICredentialsService>().UserName;
			var password = DependencyService.Get<ICredentialsService>().Password;
			HttpClientHandler handler = new HttpClientHandler() { Credentials = new NetworkCredential(username, password) };

			// ... Use HttpClient.            
			HttpClient client = new HttpClient(handler);

			//var byteArray = new UTF8Encoding().GetBytes(username + ":" + password);
			//client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

			HttpResponseMessage response = await client.GetAsync("http://us.mzoneweb.net/api/v2/vehiclegroups.json");

			//HttpContent content = response.Content;


			// Read Json

			if (response.IsSuccessStatusCode)
			{
				string jsonMessage;
				using (Stream responseStream = await response.Content.ReadAsStreamAsync())
				{
					jsonMessage = new StreamReader(responseStream).ReadToEnd();
				}

				var vehicleGroupCollection = JsonConvert.DeserializeObject<VehicleGroupCollection>(jsonMessage);

				return vehicleGroupCollection;
			}

			else
			{
				return null;
			}*/
        }


        public static async Task<TimeUtilization> GetUtilizationDataAsync(string vehicleGroupId, string startDate,
            string endDate)
        {
            var requestUrl = string.Format(URL_TIME_PROFILE_UTILIZATION, vehicleGroupId, startDate, endDate);
            var response = await GetServiceResponse<TimeUtilization>(requestUrl);

            try
            {
                return (TimeUtilization) response.ResponseToken;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }

            /*var username = DependencyService.Get<ICredentialsService>().UserName;
			var password = DependencyService.Get<ICredentialsService>().Password;
			HttpClientHandler handler = new HttpClientHandler() { Credentials = new NetworkCredential(username, password) };

			// ... Use HttpClient.            
			HttpClient client = new HttpClient(handler);

			//var byteArray = new UTF8Encoding().GetBytes(username + ":" + password);
			//client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

			HttpResponseMessage response = await client.GetAsync("http://us.mzoneweb.net/api/v2/vehiclegroups/" + vehicleGroupId + "/timeprofileutilization/" + startDate + "/" + endDate + ".json");

			//HttpContent content = response.Content;


			// Read Json

			if (response.IsSuccessStatusCode)
			{
				string jsonMessage;
				using (Stream responseStream = await response.Content.ReadAsStreamAsync())
				{
					jsonMessage = new StreamReader(responseStream).ReadToEnd();
				}

				var utilizationToken = (TimeUtilization)JsonConvert.DeserializeObject(jsonMessage, typeof(TimeUtilization));

				return utilizationToken;
			}

			else
			{
				return null;
			}*/
        }

        public static async Task<OvertimeUtilization> GetOvertimeUtilizationDataAsync(string vehicleGroupId,
            string startDate, string endDate)
        {
            var requestUrl = string.Format(URL_OVERTIME_UTILIZATION, vehicleGroupId, startDate, endDate);
            var response = await GetServiceResponse<OvertimeUtilization>(requestUrl);

            try
            {
                return (OvertimeUtilization) response.ResponseToken;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }

            /*var username = DependencyService.Get<ICredentialsService>().UserName;
			var password = DependencyService.Get<ICredentialsService>().Password;
			HttpClientHandler handler = new HttpClientHandler() { Credentials = new NetworkCredential(username, password) };

			// ... Use HttpClient.            
			HttpClient client = new HttpClient(handler);

			//var byteArray = new UTF8Encoding().GetBytes(username + ":" + password);
			//client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

			HttpResponseMessage response = await client.GetAsync("http://us.mzoneweb.net/api/v2/vehiclegroups/" + vehicleGroupId + "/timeprofilesummary/" + startDate + "/" + endDate + ".json");

			//HttpContent content = response.Content;


			// Read Json

			if (response.IsSuccessStatusCode)
			{
				string jsonMessage;
				using (Stream responseStream = await response.Content.ReadAsStreamAsync())
				{
					jsonMessage = new StreamReader(responseStream).ReadToEnd();
				}

				var overtimeUtilizationToken = (OvertimeUtilization)JsonConvert.DeserializeObject(jsonMessage, typeof(OvertimeUtilization));

				return overtimeUtilizationToken;
			}

			else
			{
				return null;
			}*/
        }

        public static async Task<FuelSummary> GetFuelSummaryDataAsync(string vehicleGroupId, string startDate,
            string endDate)
        {
            var requestUrl = string.Format(URL_FUELSUMMARY, vehicleGroupId, startDate, endDate);
            var response = await GetServiceResponse<FuelSummary>(requestUrl);

            try
            {
                return (FuelSummary) response.ResponseToken;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }

            /*var username = DependencyService.Get<ICredentialsService>().UserName;
			var password = DependencyService.Get<ICredentialsService>().Password;
			HttpClientHandler handler = new HttpClientHandler() { Credentials = new NetworkCredential(username, password) };

			// ... Use HttpClient.            
			HttpClient client = new HttpClient(handler);

			//var byteArray = new UTF8Encoding().GetBytes(username + ":" + password);
			//client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

			HttpResponseMessage response = await client.GetAsync("http://us.mzoneweb.net/api/v2/vehiclegroups/" + vehicleGroupId + "/fuelsummary/" + startDate + "/" + endDate + ".json");

			//HttpContent content = response.Content;


			// Read Json

			if (response.IsSuccessStatusCode)
			{
				string jsonMessage;
				using (Stream responseStream = await response.Content.ReadAsStreamAsync())
				{
					jsonMessage = new StreamReader(responseStream).ReadToEnd();
				}

				var fuelSummaryoken = (FuelSummary)JsonConvert.DeserializeObject(jsonMessage, typeof(FuelSummary));

				return fuelSummaryoken;
			}

			else
			{
				return null;
			}*/
        }


        public static async Task<DrivingSummary> GetDrivingSummaryAsync(string vehicleGroupId, string startDate,
            string endDate)
        {
            var requestUrl = string.Format(URL_DRIVING_SUMMARY, vehicleGroupId, startDate, endDate);
            var response = await GetServiceResponse<DrivingSummary>(requestUrl);

            try
            {
                return (DrivingSummary) response.ResponseToken;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }

            /*var username = DependencyService.Get<ICredentialsService>().UserName;
			var password = DependencyService.Get<ICredentialsService>().Password;
			HttpClientHandler handler = new HttpClientHandler() { Credentials = new NetworkCredential(username, password) };

			// ... Use HttpClient.            
			HttpClient client = new HttpClient(handler);

			//var byteArray = new UTF8Encoding().GetBytes(username + ":" + password);
			//client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

			HttpResponseMessage response = await client.GetAsync("http://us.mzoneweb.net/api/v2/vehiclegroups/" + vehicleGroupId + "/drivingtotal/" + startDate + "/" + endDate + ".json");

			//HttpContent content = response.Content;


			// Read Json

			if (response.IsSuccessStatusCode)
			{
				string jsonMessage;
				using (Stream responseStream = await response.Content.ReadAsStreamAsync())
				{
					jsonMessage = new StreamReader(responseStream).ReadToEnd();
				}

				var drivingSummaryToken = (DrivingSummary)JsonConvert.DeserializeObject(jsonMessage, typeof(DrivingSummary));

				return drivingSummaryToken;
			}

			else
			{
				return null;
			}*/
        }


        public static async Task<FuelConsumption> GetFuelConsumptionAsync(string vehicleGroupId, string startDate,
            string endDate, string groupBy)
        {
            var requestUrl = string.Format(URL_FUELCONSUMPTION, vehicleGroupId, startDate, endDate, groupBy);
            var response = await GetServiceResponse<FuelConsumption>(requestUrl);

            try
            {
                return (FuelConsumption) response.ResponseToken;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }

            /*var username = DependencyService.Get<ICredentialsService>().UserName;
			var password = DependencyService.Get<ICredentialsService>().Password;
			HttpClientHandler handler = new HttpClientHandler() { Credentials = new NetworkCredential(username, password) };

			// ... Use HttpClient.            
			HttpClient client = new HttpClient(handler);

			//var byteArray = new UTF8Encoding().GetBytes(username + ":" + password);
			//client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

			HttpResponseMessage response = await client.GetAsync("http://us.mzoneweb.net/api/v2/vehiclegroups/" + vehicleGroupId + "/fuelconsumption/" + groupBy + "/" + startDate + "/" + endDate + ".json");

			//HttpContent content = response.Content;


			// Read Json

			if (response.IsSuccessStatusCode)
			{
				string jsonMessage;
				using (Stream responseStream = await response.Content.ReadAsStreamAsync())
				{
					jsonMessage = new StreamReader(responseStream).ReadToEnd();
				}

				var fuelConsumptionToken = (FuelConsumption)JsonConvert.DeserializeObject(jsonMessage, typeof(FuelConsumption));

				return fuelConsumptionToken;
			}

			else
			{
				return null;
			}*/
        }

        public static async Task<FuelEntryVolume> GetFuelVolumeAsync(string vehicleGroupId, string startDate,
            string endDate, string groupBy)
        {
            var requestUrl = string.Format(URL_FUELVOLUME, vehicleGroupId, startDate, endDate, groupBy);
            var response = await GetServiceResponse<FuelEntryVolume>(requestUrl);

            try
            {
                return (FuelEntryVolume) response.ResponseToken;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }

            /*var username = DependencyService.Get<ICredentialsService>().UserName;
			var password = DependencyService.Get<ICredentialsService>().Password;
			HttpClientHandler handler = new HttpClientHandler() { Credentials = new NetworkCredential(username, password) };

			// ... Use HttpClient.            
			HttpClient client = new HttpClient(handler);

			//var byteArray = new UTF8Encoding().GetBytes(username + ":" + password);
			//client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

			HttpResponseMessage response = await client.GetAsync("http://us.mzoneweb.net/api/v2/vehiclegroups/" + vehicleGroupId + "/fuelvolume/" + groupBy + "/" + startDate + "/" + endDate + ".json");

			//HttpContent content = response.Content;


			// Read Json

			if (response.IsSuccessStatusCode)
			{
				string jsonMessage;
				using (Stream responseStream = await response.Content.ReadAsStreamAsync())
				{
					jsonMessage = new StreamReader(responseStream).ReadToEnd();
				}

				var fuelVolumeToken = (FuelEntryVolume)JsonConvert.DeserializeObject(jsonMessage, typeof(FuelEntryVolume));

				return fuelVolumeToken;
			}

			else
			{
				return null;
			}*/
        }

        public static async Task<FuelEntryCost> GetFuelCostAsync(string vehicleGroupId, string startDate,
            string endDate, string groupBy)
        {
            var requestUrl = string.Format(URL_FUELCOST, vehicleGroupId, startDate, endDate, groupBy);
            var response = await GetServiceResponse<FuelEntryCost>(requestUrl);

            try
            {
                return (FuelEntryCost) response.ResponseToken;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }

            /*var username = DependencyService.Get<ICredentialsService>().UserName;
			var password = DependencyService.Get<ICredentialsService>().Password;
			HttpClientHandler handler = new HttpClientHandler() { Credentials = new NetworkCredential(username, password) };

			// ... Use HttpClient.            
			HttpClient client = new HttpClient(handler);

			//var byteArray = new UTF8Encoding().GetBytes(username + ":" + password);
			//client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

			HttpResponseMessage response = await client.GetAsync("http://us.mzoneweb.net/api/v2/vehiclegroups/" + vehicleGroupId + "/fuelcost/" + groupBy + "/" + startDate + "/" + endDate + ".json");

			//HttpContent content = response.Content;


			// Read Json

			if (response.IsSuccessStatusCode)
			{
				string jsonMessage;
				using (Stream responseStream = await response.Content.ReadAsStreamAsync())
				{
					jsonMessage = new StreamReader(responseStream).ReadToEnd();
				}

				var fuelCostToken = (FuelEntryCost)JsonConvert.DeserializeObject(jsonMessage, typeof(FuelEntryCost));

				return fuelCostToken;
			}

			else
			{
				return null;
			}*/
        }


        public static async Task<Trip> GetTripDataAsync(string vehicleGroupId, string startDate, string endDate)
        {
            var requestUrl = string.Format(URL_TRIPSUMMARY, vehicleGroupId, startDate, endDate);
            var response = await GetServiceResponse<Trip>(requestUrl);

            try
            {
                return (Trip) response.ResponseToken;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }

            /*var username = DependencyService.Get<ICredentialsService>().UserName;
			var password = DependencyService.Get<ICredentialsService>().Password;
			HttpClientHandler handler = new HttpClientHandler() { Credentials = new NetworkCredential(username, password) };

			// ... Use HttpClient.            
			HttpClient client = new HttpClient(handler);

			//var byteArray = new UTF8Encoding().GetBytes(username + ":" + password);
			//client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

			HttpResponseMessage response = await client.GetAsync("http://us.mzoneweb.net/api/v2/vehiclegroups/" + vehicleGroupId + "/trips/" + startDate + "/" + endDate + "/summary.json");

			//HttpContent content = response.Content;


			// Read Json

			if (response.IsSuccessStatusCode)
			{
				string jsonMessage;
				using (Stream responseStream = await response.Content.ReadAsStreamAsync())
				{
					jsonMessage = new StreamReader(responseStream).ReadToEnd();
				}

				var tripToken = (Trip)JsonConvert.DeserializeObject(jsonMessage, typeof(Trip));

				return tripToken;
			}

			else
			{
				return null;
			}*/
        }


        public static CachedImage GetIcon(Guid vehicleId)
        {
            var username = DependencyService.Get<ICredentialsService>().MZoneUserName;
            var password = DependencyService.Get<ICredentialsService>().MZonePassword;
            var handler = new HttpClientHandler() {Credentials = new NetworkCredential(username, password)};

            // ... Use HttpClient.            
            var client = new HttpClient(handler);

            var config = new Configuration();
            config.HttpClient = client;

            ImageService.Instance.Initialize(config);

            var image = new CachedImage
            {
                CacheDuration = TimeSpan.FromDays(30),
                Source = "http://us.mzoneweb.net/api/v2/vehicles/" + vehicleId + "_32x32.png",
                WidthRequest = 10
            };

            return image;
        }
    }
}