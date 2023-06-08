using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Akavache;
using StyexFleetManagement.Models;
using StyexFleetManagement.Statics;
using Xamarin.Forms;

namespace StyexFleetManagement.Services
{
    public class TripsAPI
    {
        private const string URL_POSITIONS = "trips/{0}/positions.json";
        private const string URL_EXCEPTIONS = "trips/{0}/exceptions.json";
        private const string URL_BEHAVIOUR_EXCEPTIONS = "trips/{0}/driverbehaviourexceptions.json";
        private const string URL_EXTENDED_TRIP = "vehiclegroups/{0}/tripsupdatedsince/{1}/{2}.json?i=E&i=P&i=PA&i=T&i=S&i=R&i=D&i=A";
        private const string URL_TRIPS_WITH_STATS = "vehicles/{0}/tripswithstats/{1}/{2}.json";
        private const string URL_UNIT_TRIPS_WITH_STATS = "units/{0}/tripswithstats/{1}/{2}.json";
        private const string URL_AGGREGATED_TRIP_DATA = "vehiclegroups/{0}/trips/{1}/{2}/summary.json";
        private const string SNAP_TO_ROAD_URL = "https://roads.googleapis.com/v1/snapToRoads?path={0}&interpolate=true&key={1}";
        private const string STATIC_ROUTE_MAP_URL = "https://maps.googleapis.com/maps/api/staticmap?markers=color:green%7C|{0}&markers=color:black%7C|{1}&path=color:0x0000ff|weight:5{2}&size=300x150&key={3}";

        public static TimeSpan TIMEOUT = TimeSpan.FromMilliseconds(30000);

        public static int PAGINATION_OVERLAP = 5;

        public static int PAGE_SIZE_LIMIT = 100;

        private static HttpClient GetAuthenticatedClient()
        {
            var username = DependencyService.Get<ICredentialsService>().MZoneUserName;
            var password = DependencyService.Get<ICredentialsService>().MZonePassword;
            HttpClientHandler handler = new HttpClientHandler() { Credentials = new NetworkCredential(username, password) };

            // ... Use HttpClient.            
            HttpClient client = new HttpClient(handler);
            return client;
        }
        public static async Task<List<ExtendedTrip>> GetExtendedTripAsync(string vehicleGroupId, DateTime startDate, DateTime endDate)
        {
            //Format dates
            string s = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            string e = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

            var requestUrl = string.Format(URL_EXTENDED_TRIP, vehicleGroupId, s, e);
            var response = await RestService.GetServiceResponse<ExtendedTripResponse>(requestUrl);

            try
            {
                var responseToken = (ExtendedTripResponse)response.ResponseToken;
                return responseToken.Items;
            }
            catch (InvalidCastException exception)
            {
                Serilog.Log.Debug(exception.Message);
                return null;
            }
        }
        public static async Task<List<Trip>> GetTripsWithStats(string vehicleId, DateTime startDate, DateTime endDate, bool isUnitId = false)
        {
            bool inCache = false;
            //Format dates
            string s = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            string e = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            string cacheKey = "Trips:" + vehicleId + ":" + isUnitId.ToString() + DateTime.Now.ToString("yyyyMMddTHH") + ":" + startDate.ToString("yyyyMMddTHH", CultureInfo.InvariantCulture) + "-" + endDate.ToString("yyyyMMddTHH", CultureInfo.InvariantCulture); ;

            string requestUrl;
            if (isUnitId)
                requestUrl = string.Format(URL_UNIT_TRIPS_WITH_STATS, vehicleId, s, e);
            else
                requestUrl = string.Format(URL_TRIPS_WITH_STATS, vehicleId, s, e);
            
            return await RestService.GetServiceResponseFromCacheAsync<Trip>(cacheKey, requestUrl);
            /*var cache = BlobCache.LocalMachine;
            string cacheKey = "Trips:" + vehicleId + ":" + s + "-" + e;
            List<Trip> trips = new List<Trip>();
            try
            {
                trips = await cache.GetObject<List<Trip>>(cacheKey);
                if (trips != null && trips.Count > 0)
                {
                    inCache = true;
                }
                
            }
            catch (System.Collections.Generic.KeyNotFoundException keyNotFound)
            {
                inCache = false;
            }

            if (!inCache)
            {
                try
                {
                    trips = await GetTripsWithStatsAsync(vehicleId, s, e);
                    await cache.InsertObject<List<Trip>>(cacheKey, trips);

                }
                catch (NullReferenceException error)
                {
                    Xamarin.Insights.Report(error);
                }
            }

            return trips;*/


        }

        internal static async Task<List<AggregatedTripData>> GetAggregatedTripDataAsync(string selectedVehicleGroup, DateTime startDate, DateTime endDate)
        {
            bool inCache = false;
            //Format dates
            string s = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            string e = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

            var cache = BlobCache.LocalMachine;
            string cacheKey = "Aggregated_trip_data:" + selectedVehicleGroup + ":" + DateTime.Now.ToString("yyyyMMddTHH") + ":" + startDate.ToString("yyyyMMddTHH") + "-" + endDate.ToString("yyyyMMddTHH");
            List<AggregatedTripData> trips = new List<AggregatedTripData>();
            try
            {
                trips = await cache.GetObject<List<AggregatedTripData>>(cacheKey);
                if (trips != null)
                {
                    inCache = true;
                }

            }
            catch (System.Collections.Generic.KeyNotFoundException keyNotFound)
            {
                inCache = false;
            }

            if (!inCache)
            {
                try
                {
                    trips = await GetAggregatedTripData(selectedVehicleGroup, s, e);
                    await cache.InsertObject<List<AggregatedTripData>>(cacheKey, trips);

                }
                catch (NullReferenceException exception)
                {
                    Serilog.Log.Debug(exception.Message);
                }
            }

            return trips;
        }
        private static async Task<List<AggregatedTripData>> GetAggregatedTripData(string selectedVehicleGroup, string s, string e)
        {
            
            var requestUrl = string.Format(URL_AGGREGATED_TRIP_DATA, selectedVehicleGroup, s, e);
            var response = await RestService.GetServiceResponse<APIResponse<AggregatedTripData>>(requestUrl);

            try
            {
                var responseToken = (APIResponse<AggregatedTripData>)response.ResponseToken;
                return responseToken.Items;
            }
            catch (InvalidCastException exception)
            {
                Serilog.Log.Debug(exception.Message);
                return null;
            }
        }

        private static async Task<List<Trip>> GetTripsWithStatsAsync(string vehicleId, string s, string e)
        {
            var requestUrl = string.Format(URL_TRIPS_WITH_STATS, vehicleId, s, e);
            var response = await RestService.GetServiceResponse<TripsResponse>(requestUrl);

            try
            {
                var responseToken = (TripsResponse)response.ResponseToken;
                return responseToken.Items;
            }
            catch (InvalidCastException exception)
            {
                Serilog.Log.Debug(exception.Message);
                return null;
            }
        }

        public static async Task<TripPositions> GetTripPositionsAsync(int tripId)
        {
            var requestUrl = string.Format(URL_POSITIONS, tripId);
            var response = await RestService.GetServiceResponse<TripPositions>(requestUrl);

            try
            {
                return (TripPositions)response.ResponseToken;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }

            /*var client = GetAuthenticatedClient();

			HttpResponseMessage response = await client.GetAsync("http://us.mzoneweb.net/api/v2/trips/" + tripId + "/positions.json");
			//HttpContent content = response.Content;


			// Read Json

			if (response.IsSuccessStatusCode)
			{
				string jsonMessage;
				using (Stream responseStream = await response.Content.ReadAsStreamAsync())
				{
					jsonMessage = new StreamReader(responseStream).ReadToEnd();
				}

				var tripToken = (TripPositions)JsonConvert.DeserializeObject(jsonMessage, typeof(TripPositions));

				return tripToken;
			}

			else
			{
				return null;
			}*/
        }
        public static async Task<List<SnappedPoint>> SnapTripToRoad(TripPositions trip)
        {
            List<SnappedPoint> snappedPoints = new List<SnappedPoint>();

            int offset = 0;
            while (offset < trip.Items.Count)
            {
                // Calculate which points to include in this request. We can't exceed the API's
                // maximum and we want to ensure some overlap so the API can infer a good location for
                // the first few points in each request.
                if (offset > 0)
                {
                    offset -= PAGINATION_OVERLAP;   // Rewind to include some previous points.
                }
                int lowerBound = offset;
                int upperBound = Math.Min(offset + PAGE_SIZE_LIMIT, trip.Items.Count);

                // Get the data we need for this page.
                List<List<float>> page = trip.Items
                        .GetRange(lowerBound, upperBound - lowerBound);

                // Perform the request. Because we have interpolate=true, we will get extra data points
                // between our originally requested path. To ensure we can concatenate these points, we
                // only start adding once we've hit the first new point (that is, skip the overlap).
                List<SnappedPoint> points = await SnapTripToRoadAsync(page);
                bool passedOverlap = false;
                foreach (SnappedPoint point in points)
                {
                    if (offset == 0 || point.OriginalIndex >= PAGINATION_OVERLAP - 1)
                    {
                        passedOverlap = true;
                    }
                    if (passedOverlap)
                    {
                        snappedPoints.Add(point);
                    }
                }

                offset = upperBound;
            }


            return snappedPoints;
        }

        internal static Task<AggregatedTripData> GetAggregatedTripDataAsync(string selectedVehicleGroup, object startDate, object endDate)
        {
            throw new NotImplementedException();
        }

        public static string GetStaticMapImageUrlForRoute(List<List<float>> tripPositions)
        {
            string positionArray = "";

            if (tripPositions == null || tripPositions.Count == 0)
                return string.Empty;



            if (tripPositions.Count > 250)
            {
                var tempPositions = tripPositions;
                tripPositions = new List<List<float>>();
                int multiple = (int)Math.Ceiling(tempPositions.Count / (float)250);

                tripPositions.Add(tempPositions[0]);
                for (int i = 1; i < tempPositions.Count - 1; i += multiple)
                {
                    tripPositions.Add(tempPositions[i]);
                }
                tripPositions.Add(tempPositions[tempPositions.Count - 1]);
            }

            foreach (List<float> position in tripPositions)
            {
                positionArray +=
                    $"|{position[1].ToString(CultureInfo.InvariantCulture)},{position[0].ToString(CultureInfo.InvariantCulture)}";

            }



            string startLocation =
                $"{tripPositions[0][1].ToString(CultureInfo.InvariantCulture)},{tripPositions[0][0].ToString(CultureInfo.InvariantCulture)}";
            string endLocation =
                $"{tripPositions[tripPositions.Count - 1][1].ToString(CultureInfo.InvariantCulture)},{tripPositions[tripPositions.Count - 1][0].ToString(CultureInfo.InvariantCulture)}";

            var requestUrl = string.Format(STATIC_ROUTE_MAP_URL, startLocation, endLocation, positionArray, Keys.GOOGLE_STATIC_IMAGE_API_KEY);
            return requestUrl;

        }
        private static async Task<List<SnappedPoint>> SnapTripToRoadAsync(List<List<float>> tripPositions)
        {
            string positionArray = "";
            bool isFirstIndex = true;

            foreach (List<float> position in tripPositions)
            {
                if (!isFirstIndex)
                {
                    positionArray +=
                        $"|{position[1].ToString(CultureInfo.InvariantCulture)},{position[0].ToString(CultureInfo.InvariantCulture)}";
                }
                else
                {
                    positionArray +=
                        $"{position[1].ToString(CultureInfo.InvariantCulture)},{position[0].ToString(CultureInfo.InvariantCulture)}";
                    isFirstIndex = false;
                }

            }

            var requestUrl = string.Format(SNAP_TO_ROAD_URL, positionArray, Keys.GOOGLE_ROAD_API_KEY);
            var response = await RestService.GetHttpResponse<SnappedPointResponse>(requestUrl);

            try
            {
                return ((SnappedPointResponse)response.ResponseToken).SnappedPoints;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }
        }
        public static async Task<TripFleetExceptionList> GetTripFleetExceptions(int tripId)
        {
            var requestUrl = string.Format(URL_EXCEPTIONS, tripId);

            Debug.WriteLine("Getting trip fleet exception list");

            var response = await RestService.GetServiceResponse<TripFleetExceptionList>(requestUrl);

            try
            {
                return (TripFleetExceptionList)response.ResponseToken;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }
        }


        public static async Task<List<TripException>> GetTripExceptionsAsync(int tripId, EventTypeGroup eventGroup)
        {

            var key = "TRIP_EXCEPTIONS:" + eventGroup.ToString() + " " + tripId.ToString();

            string requestUrl;
            switch (eventGroup)
            {
                case EventTypeGroup.UBIExceptions:
                    requestUrl = string.Format(URL_BEHAVIOUR_EXCEPTIONS, tripId);
                    break;
                default:
                case EventTypeGroup.FleetExceptions:
                    requestUrl = string.Format(URL_EXCEPTIONS, tripId);
                    break;
            }

            var exceptions = await RestService.GetServiceResponseFromCacheAsync<TripException>(key, requestUrl);

            return exceptions;
        }

        public static async Task<TripUBIExceptionList> GetTripUBIExceptionsAsync(int tripId)
        {
            var requestUrl = string.Format(URL_BEHAVIOUR_EXCEPTIONS, tripId);
            var response = await RestService.GetServiceResponse<TripUBIExceptionList>(requestUrl);

            try
            {
                return (TripUBIExceptionList)response.ResponseToken;
            }
            catch (InvalidCastException e)
            {
                Serilog.Log.Debug(e.Message);
                return null;
            }
        }
    }
}

