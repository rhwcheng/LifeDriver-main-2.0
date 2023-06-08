using Akavache;
using Serilog;
using StyexFleetManagement.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyexFleetManagement.Services
{
    public class EventAPI
    {

        private const string URL_EVENTS = "vehiclegroups/{0}/events/{1}/{2}.json?ps={3}";
        private const string URL_LATEST_EVENTS = "vehiclegroups/{0}/eventtypegroups/{1}/events/{2}/after.json";
        private const string URL_VEHICLE_EVENTS_BETWEEN_DATES = "vehicles/{0}/eventtypegroups/{1}/events/{2}/{3}.json";
        private const string URL_VEHICLE_EVENTS_FOR_TRIP = "vehicles/{0}/events/{1}/{2}.json";
        private const string URL_STATUS_EVENTS_BY_ARRAY_BETWEEN_DATES = "vehicles/{0}/events/{1}/{2}/bytypearray.json?{3}";
        private const string URL_STATUS_EVENTS_BY_ARRAY_BETWEEN_DATES_UNITID = "units/{0}/events/{1}/{2}/bytypearray.json?{3}";
        private const string URL_DRIVER_EVENT = "units/{0}/eventtypes/228/events/{1}/{2}.json";
        private const string URL_EVENT_BY_ID = "vehicles/{0}/eventtypes/{1}/events/{2}/{3}.json";
        private const string URL_UNIT_EVENT_BY_ID = "units/{0}/eventtypes/{1}/events/{2}/{3}.json";
        private const string URL_EVENT_BY_VEHICLEGROUP = "vehiclegroups/{0}/events/{1}/{2}.json";

        public static async Task<List<Event>> GetLatestEvents(string vehicleGroup, bool cacheOnly = false)
        {
            string key = "EVENTS: " + vehicleGroup;
            var cache = BlobCache.LocalMachine;
            bool cacheEmpty = false;
            List<Event> cachedEvents = new List<Event>();

            string lastEventId = "";

            try
            {
                cachedEvents = await cache.GetObject<List<Event>>(key);

                if (cachedEvents.Count > 0)
                {
                    lastEventId = cachedEvents.Last().Id.ToString();
                }
                else
                    cacheEmpty = true;


                if (cacheOnly)
                    return cachedEvents;
            }
            catch (KeyNotFoundException keyNotFound)
            {
                cacheEmpty = true;
                if (cacheOnly)
                    return null;
            }

            if (cacheEmpty)
            {
                var lastEvent = await GetEventFromThreeDays(vehicleGroup, 1);

                if (lastEvent == null)
                {
                    return null;
                }
                else
                    lastEventId = lastEvent.Id.ToString();
            }

            var requestUrl = string.Format(URL_LATEST_EVENTS, vehicleGroup, Constants.ALL_EVENT_TYPES, lastEventId);
            var response = await RestService.GetServiceResponse<List<Event>>(requestUrl);

            try
            {
                List<Event> eventList = new List<Event>();
                if (response.ErrorStatus == ServiceError.NO_ERROR)
                    eventList = (List<Event>)response.ResponseToken;
                else
                    return cachedEvents;

                int count = eventList.Count;
                while (!(count < 1000))
                {
                    var nextRequestUrl = string.Format(URL_LATEST_EVENTS, vehicleGroup, Constants.ALL_EVENT_TYPES, eventList.Last().Id.ToString());
                    var nextResponse = await RestService.GetServiceResponse<List<Event>>(nextRequestUrl);
                    var nextEventList = (List<Event>)nextResponse.ResponseToken;
                    count = nextEventList.Count;
                    eventList = eventList.Concat(nextEventList).ToList();
                }

                eventList = eventList.Where(x => x.EventTypeId == (int)EventType.CONSOLIDATED_FUEL_DATA ||
                x.EventTypeId == (int)EventType.DEVICE_UNPLUGGED ||
                x.EventTypeId == (int)EventType.IDLE ||
                x.EventTypeId == (int)EventType.MAINS_LOW ||
                x.EventTypeId == (int)EventType.TRIP_SHUTDOWN ||
                x.EventTypeId == (int)EventType.TRIP_STARTUP).ToList();

                if (!cacheEmpty)
                {
                    var combinedList = cachedEvents.Concat(eventList).ToList();

                    await cache.InsertObject<List<Event>>(key, combinedList, TimeSpan.FromDays(3));

                    return combinedList;
                }
                else
                {
                    await cache.InsertObject<List<Event>>(key, eventList, TimeSpan.FromDays(3));

                    return eventList;
                }

            }
            catch (InvalidCastException e)
            {
                Log.Debug(e.Message);
                return null;
            }

        }

        public static async Task<List<Event>> GetEventsByVehicleGroup(string selectedVehicleGroup, DateTime startDate, DateTime endDate)
        {
            var startDateString = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDateString = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

            string key;

            var requestUrl = string.Format(URL_EVENT_BY_VEHICLEGROUP, selectedVehicleGroup, startDateString, endDateString);

            List<Event> items = new List<Event>();
            List<Event> repsonseItems = new List<Event>();
            int pg = 0;
            int ps = 1000;

            do
            {
                string url;
                if (requestUrl.Substring(Math.Max(0, requestUrl.Length - 4)) == "json")
                    url = $"{requestUrl}?pg={pg.ToString()}";
                else
                    url = $"{requestUrl}&pg={pg.ToString()}";

                var response = await RestService.GetServiceResponse<APIResponse<Event>>(url);

                try
                {
                    repsonseItems = (List<Event>)((APIResponse<Event>)response.ResponseToken).Items;
                    items.AddRange(repsonseItems);
                    Debug.WriteLine("Events by vehicle group. Page: " + pg.ToString());
                    pg += 1;
                }
                catch (InvalidCastException e)
                {
                    Log.Debug(e.Message);
                    return null;
                }
            } while (repsonseItems.Count == 1000);

            return repsonseItems;
        }

        public static async Task<List<DriverEvent>> GetDriverEvents(string vehicleId, DateTime startDate, DateTime endDate)
        {
            string key = "DRIVER_EVENTS: " + vehicleId + ":" + startDate + "-" + endDate;
            var cache = BlobCache.LocalMachine;
            bool cacheEmpty = false;
            List<DriverEvent> cachedEvents = new List<DriverEvent>();

            try
            {
                cachedEvents = await cache.GetObject<List<DriverEvent>>(key);

                if (cachedEvents == null)
                {
                    await cache.InvalidateObject<List<DriverEvent>>(key);
                    cacheEmpty = true;
                }

            }
            catch (KeyNotFoundException keyNotFound)
            {
                cacheEmpty = true;
            }

            if (cacheEmpty)
            {
                var startDateFormatted = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
                var endDateFormatted = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
                var requestUrl = string.Format(URL_DRIVER_EVENT, vehicleId, startDateFormatted, endDateFormatted);

                try
                {
                    var response = await RestService.GetServiceResponse<APIResponse<DriverEvent>>(requestUrl);
                    APIResponse<DriverEvent> responseToken = (APIResponse<DriverEvent>)response.ResponseToken;
                    var driverEvents = responseToken.Items;
                    await cache.InsertObject(key, driverEvents);
                    return driverEvents;
                }
                catch (NullReferenceException ne)
                {
                    return null;
                }
                catch (InvalidCastException e)
                {
                    Log.Debug(e.Message);
                    return null;
                }
            }

            else
                return cachedEvents;

        }
        public static async Task<List<Event>> GetLatestEventsFromCache(string vehicleGroup)
        {
            string key = "EVENTS: " + vehicleGroup;
            var cache = BlobCache.LocalMachine;
            bool cacheEmpty = false;
            List<Event> cachedEvents = new List<Event>();

            string lastEventId = "";

            try
            {
                cachedEvents = await cache.GetObject<List<Event>>(key);

                if (cachedEvents.Count == 0)
                    cacheEmpty = true;
            }
            catch (KeyNotFoundException keyNotFound)
            {
                cacheEmpty = true;
            }

            if (cacheEmpty)
            {
                return null;
            }

            else
                return cachedEvents;

        }

        public static async Task<List<Event>> GetEventsForTrip(string vehicleId, DateTime tripStartDate, DateTime tripEndDate)
        {
            string key = "TRIP EVENTS: " + vehicleId + ":" + tripStartDate + "-" + tripEndDate;

            var startDate = tripStartDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDate = tripEndDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

            var requestUrl = string.Format(URL_VEHICLE_EVENTS_FOR_TRIP, vehicleId, startDate, endDate);

            var events = await RestService.GetServiceResponseFromCacheAsync<Event>(key, requestUrl);

            return events;
            /*var cache = BlobCache.LocalMachine;
            bool cacheEmpty = false;
            List<Event> cachedEvents = new List<Event>();
            

            try
            {
                cachedEvents = await cache.GetObject<List<Event>>(key);

                if (cachedEvents.Count == 0)
                    cacheEmpty = true;
            }
            catch (KeyNotFoundException keyNotFound)
            {
                cacheEmpty = true;
            }

            if (cacheEmpty)
            {
                return await FetchEventsForTrip(vehicleId, trip);
            }

            else
                return cachedEvents;*/

        }

        public static async Task<List<Event>> GetEventsById(string vehicleId, int eventId, DateTime startDate, DateTime endDate, bool exactDate = false, bool isUnitId = false)
        {

            var startDateString = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDateString = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

            string key;
            if (exactDate)
                key = "EVENTS: " + DateTime.Now.ToString("yyyyMMddTHH") + ":" + vehicleId + "_" + eventId.ToString() + ":" + startDateString + "-" + endDateString;
            else
                key = "EVENTS: " + DateTime.Now.ToString("yyyyMMddTHH") + ":" + vehicleId + "_" + eventId.ToString() + ":" + startDateString + "-" + endDateString;

            string requestUrl;
            if (isUnitId)
                requestUrl = string.Format(URL_UNIT_EVENT_BY_ID, vehicleId, eventId.ToString(), startDateString, endDateString);
            else
                requestUrl = string.Format(URL_EVENT_BY_ID, vehicleId, eventId.ToString(), startDateString, endDateString);

            var events = await RestService.GetServiceResponseFromCacheAsync<Event>(key, requestUrl);

            return events;
        }


        private static async Task<List<Event>> FetchEventsForTrip(string vehicleId, Trip trip)
        {
            var startDate = trip.StartLocalTimestamp.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDate = trip.StartLocalTimestamp.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var requestUrl = string.Format(URL_VEHICLE_EVENTS_FOR_TRIP, vehicleId, startDate, endDate);
            var response = await RestService.GetServiceResponse<Events>(requestUrl);

            try
            {
                Events responseToken = (Events)response.ResponseToken;

                return responseToken.Items;
            }
            catch (InvalidCastException e)
            {
                Log.Debug(e.Message);
                return null;
            }

        }
        public static async Task<Event> GetEventFromThreeDays(string vehicleGroup, int ps = 1)
        {
            var startDate = DateTime.Now.AddDays(-3).ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDate = DateTime.Now.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var requestUrl = string.Format(URL_EVENTS, vehicleGroup, startDate, endDate, ps);
            var response = await RestService.GetServiceResponse<Events>(requestUrl);

            try
            {
                Events responseToken = (Events)response.ResponseToken;

                return responseToken.Items.FirstOrDefault();
                //if (!(responseToken.TotalResults > responseToken.PageSize))
                //    return responseToken.Items;
                //else
                //{
                //    decimal count = (decimal)responseToken.TotalResults / responseToken.PageSize;
                //    var pages = Math.Ceiling(count);

                //    for (int i = 1; i < 5; i++)
                //    {
                //        var url = String.Format(URL_EVENTS, vehicleGroup, startDate, endDate, i);
                //        var resp = await RestService.GetServiceResponse<Events>(url);
                //        responseToken.Items.AddRange(((Events)resp.ResponseToken).Items);
                //    }
                //    return responseToken.Items;
                //}
            }
            catch (InvalidCastException e)
            {
                Log.Debug(e.Message);
                return null;
            }
        }

        private static async Task<List<Event>> GetVehicleExceptionEventsBetweenDates(string vehicleId, EventType eventType, DateTime startDate, DateTime endDate)
        {
            var startDateString = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDateString = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

            string requestUrl = "";
            switch (eventType)
            {
                case (EventType.FLEET_EXCEPTION):
                    requestUrl = string.Format(URL_VEHICLE_EVENTS_BETWEEN_DATES, vehicleId, Constants.DRIVER_EXCEPTION_EVENTGROUP_ID, startDateString, endDateString);
                    break;
                default:
                    requestUrl = string.Format(URL_VEHICLE_EVENTS_BETWEEN_DATES, vehicleId, Constants.DRIVER_BEHAVIOUR_EVENTGROUP_ID, startDateString, endDateString);
                    break;

            }

            var response = await RestService.GetServiceResponse<Events>(requestUrl);

            try
            {
                Events responseToken = (Events)response.ResponseToken;

                return responseToken.Items;
            }
            catch (InvalidCastException e)
            {
                Log.Debug(e.Message);
                return null;
            }

        }

        public static async Task<List<Event>> GetVehicleExceptionEventsBetweenDates(string vehicleId, EventType eventType, DateTime startDate, DateTime endDate, bool fromCache)
        {
            var cache = BlobCache.LocalMachine;

            if (fromCache)
            {
                string key = eventType.ToString() + vehicleId + startDate.ToString() + endDate.ToString();
                var result = cache.GetOrFetchObject<List<Event>>(key,
                    () => GetVehicleExceptionEventsBetweenDates(vehicleId, eventType, startDate, endDate));



                try
                {
                    List<Event> fleetExceptions = await result.FirstAsync();
                    //fleetExceptions = fleetExceptions.Where(f => f.LocalTimestamp >= startDate && f.LocalTimestamp <= endDate).ToList();
                    return fleetExceptions;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            else
            {
                var result = await GetVehicleExceptionEventsBetweenDates(vehicleId, eventType, startDate, endDate);

                return result;
            }




        }
        public static async Task<List<Event>> GetEventsByArrayAsync(string vehicleId, DateTime startDate, DateTime endDate, EventTypeGroup eventGroup = EventTypeGroup.TripStartStop, bool includeTripStartStop = false, bool includeTachographData = false, bool includePeriodicPosition = false, bool enablePagination = true, bool isUnitId = false)
        {

            var startDateString = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDateString = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var now = DateTime.Now.ToString("yyyyMMddTHHmmss");

            string key = "EVENTS(" + now.Substring(0, now.Length - 3) + ":" + eventGroup.ToString() + ")" + ": " + vehicleId + ":" + startDateString.Substring(0, startDateString.Length - 3) + "-" + endDateString.Substring(0, endDateString.Length - 3);

            string requestUrl;
            string eventArray;

            switch (eventGroup)
            {
                case EventTypeGroup.FleetExceptions:
                    eventArray = "et=153&et=140&et=144&et=152&et=155&et=236&et=150";
                    break;
                case EventTypeGroup.UBIExceptions:
                    eventArray = "et=54&et=58&et=61&et=56&et=55&et=52&et=51&et=63&et=53&et=59&et=60&et=50&et=57&et=62&et=96&et=95";
                    break;
                case EventTypeGroup.AllExceptions:
                    eventArray = "et=153&et=140&et=144&et=152&et=155&et=236&et=150&et=54&et=58&et=61&et=56&et=55&et=52&et=51&et=63&et=53&et=59&et=60&et=50&et=57&et=62&et=96&et=95";
                    break;
                default:
                case EventTypeGroup.TripStartStop:
                    eventArray = "et=245&et=52&et=242&et=123&et=122&et=215";
                    break;
            }
            if (includeTripStartStop && eventGroup != EventTypeGroup.TripStartStop)
                eventArray += "&et=245&et=52&et=242&et=123&et=122&et=215";
            if (includeTachographData)
                eventArray += ("&et=" + ((int)TripEventType.Tachograph).ToString());
            if (includePeriodicPosition)
                eventArray += ("&et=" + ((int)TripEventType.PeriodicPosition).ToString());

            if (isUnitId)
                requestUrl = string.Format(URL_STATUS_EVENTS_BY_ARRAY_BETWEEN_DATES_UNITID, vehicleId, startDateString, endDateString, eventArray);
            else
                requestUrl = string.Format(URL_STATUS_EVENTS_BY_ARRAY_BETWEEN_DATES, vehicleId, startDateString, endDateString, eventArray);


            string optionalKey = "EVENTS(" + eventGroup.ToString() + ")" + ": " + vehicleId + "-" + eventArray;

            try
            {
                var response = await RestService.GetServiceResponseFromCacheAsync<Event>(key, requestUrl, enablePagination, optionalKey);
                return response;
            }
            catch (InvalidCastException e)
            {
                Log.Debug(e.Message);
                return null;
            }
            catch (Exception ex){
                Log.Debug(ex.Message);
                return null;
            }
        }


        public static async Task<List<Event>> GetEventsByArrayFromCache(string vehicleId, DateTime startDate, DateTime endDate, EventTypeGroup eventGroup = EventTypeGroup.TripStartStop, bool includeTripStartStop = false, bool includeTachographData = false, bool includePeriodicPosition = false, bool enablePagination = true, bool isUnitId = false)
        {

            var startDateString = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDateString = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var now = DateTime.Now.ToString("yyyyMMddTHHmmss");

            string requestUrl;
            string eventArray;

            switch (eventGroup)
            {
                case EventTypeGroup.FleetExceptions:
                    eventArray = "et=153&et=140&et=144&et=152&et=155&et=236&et=150";
                    break;
                case EventTypeGroup.UBIExceptions:
                    eventArray = "et=54&et=58&et=61&et=56&et=55&et=52&et=51&et=63&et=53&et=59&et=60&et=50&et=57&et=62&et=96&et=95";
                    break;
                case EventTypeGroup.AllExceptions:
                    eventArray = "et=153&et=140&et=144&et=152&et=155&et=236&et=150&et=54&et=58&et=61&et=56&et=55&et=52&et=51&et=63&et=53&et=59&et=60&et=50&et=57&et=62&et=96&et=95";
                    break;
                default:
                case EventTypeGroup.TripStartStop:
                    eventArray = "et=245&et=52&et=242&et=123&et=122&et=215";
                    break;
            }
            if (includeTripStartStop && eventGroup != EventTypeGroup.TripStartStop)
                eventArray += "&et=245&et=52&et=242&et=123&et=122&et=215";
            if (includeTachographData)
                eventArray += ("&et=" + ((int)TripEventType.Tachograph).ToString());
            if (includePeriodicPosition)
                eventArray += ("&et=" + ((int)TripEventType.PeriodicPosition).ToString());


            string key = "EVENTS(" + eventGroup.ToString() + ")" + ": " + vehicleId + "-" + eventArray;

            if (isUnitId)
                requestUrl = string.Format(URL_STATUS_EVENTS_BY_ARRAY_BETWEEN_DATES_UNITID, vehicleId, startDateString, endDateString, eventArray);
            else
                requestUrl = string.Format(URL_STATUS_EVENTS_BY_ARRAY_BETWEEN_DATES, vehicleId, startDateString, endDateString, eventArray);

            try{
                var cache = BlobCache.LocalMachine;
                List<Event> response = await cache.GetObject<List<Event>>(key);
                return response;
            }
            catch (KeyNotFoundException e){
                return null;
            }
        }

        public static async Task<List<Event>> GetEventsArrayByVehicleAsync(string vehicleId, DateTime startDate = default(DateTime), DateTime endDate = default(DateTime), EventTypeGroup eventGroup = EventTypeGroup.TripStartStop, bool includeTacho = false, bool includePeriodicPosition = false)
        {
            //var startDateString = Settings.Current.LastEventReceived;
            var cache = BlobCache.LocalMachine;
            bool cacheEmpty = false;
            List<Event> cachedEvents = new List<Event>();

            if (startDate.Equals(default(DateTime)))
                startDate = DateTime.Now.AddDays(-3);
            if (endDate.Equals(default(DateTime)))
                endDate = DateTime.Now;

            string key = "EVENTS: " + vehicleId + ":" + startDate + "-" + endDate;

            try
            {
                cachedEvents = await cache.GetObject<List<Event>>(key);
                cachedEvents = cachedEvents.Where(x => x.LocalTimestamp >= startDate).ToList();

                if (cachedEvents.Count > 0)
                {
                    startDate = cachedEvents.Last().LocalTimestamp.DateTime;
                    if (startDate > DateTime.Now.AddHours(-1))
                        return cachedEvents;
                }
                else
                    cacheEmpty = true;
            }
            catch (KeyNotFoundException keyNotFound)
            {
                cacheEmpty = true;
            }



            var startDateString = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDateString = endDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

            string requestUrl;
            string eventArray;

            switch (eventGroup)
            {
                case EventTypeGroup.FleetExceptions:
                    eventArray = "et=153&et=140&et=144&et=152&et=155&et=236&et=150";
                    break;
                case EventTypeGroup.UBIExceptions:
                    eventArray = "et=54&et=58&et=61&et=56&et=55&et=52&et=51&et=63&et=53&et=59&et=60&et=50&et=57&et=62&et=96&et=95";
                    break;
                default:
                case EventTypeGroup.TripStartStop:
                    eventArray = "et=245&et=52&et=242&et=123&et=122&et=215";
                    if (includePeriodicPosition)
                        eventArray += ("&et=" + TripEventType.PeriodicPosition.ToString());
                    if (includeTacho)
                        eventArray += ("&et=" + TripEventType.Tachograph.ToString());
                    break;
            }

            requestUrl = string.Format(URL_STATUS_EVENTS_BY_ARRAY_BETWEEN_DATES, vehicleId, startDateString, endDateString, eventArray);
            var response = await RestService.GetServiceResponse<Events>(requestUrl);

            try
            {
                Events responseToken = (Events)response.ResponseToken;

                List<Event> newEvents = responseToken.Items;

                if (!cacheEmpty)
                {
                    var combinedList = cachedEvents.Concat(newEvents).ToList();

                    await cache.InsertObject<List<Event>>(key, combinedList, TimeSpan.FromDays(3));

                    return combinedList;
                }

                await cache.InsertObject<List<Event>>(key, newEvents, TimeSpan.FromDays(3));

                return newEvents;
            }
            catch (InvalidCastException e)
            {
                Log.Debug(e.Message);
                return null;
            }
        }
        public static async Task<List<Event>> GetEventsArrayByVehicleFromCache(string vehicleId)
        {
            string key = "EVENTS: " + vehicleId;
            //var startDateString = Settings.Current.LastEventReceived;
            var cache = BlobCache.LocalMachine;
            List<Event> cachedEvents = new List<Event>();


            try
            {
                cachedEvents = await cache.GetObject<List<Event>>(key);

                if (cachedEvents != null && cachedEvents.Count > 0)
                {
                    return cachedEvents;
                }
            }
            catch (KeyNotFoundException keyNotFound)
            {
                return null;
            }

            return null;
        }


        public static async Task<List<Event>> GetStatusEventsByVehicleAsync(string vehicleId)
        {
            string key = "STATUS: " + vehicleId;
            //var startDateString = Settings.Current.LastEventReceived;
            var cache = BlobCache.LocalMachine;
            bool cacheEmpty = false;
            List<Event> cachedEvents = new List<Event>();

            DateTime startDate = DateTime.Now.AddDays(-3);

            try
            {
                cachedEvents = await cache.GetObject<List<Event>>(key);

                if (cachedEvents.Count > 0)
                {
                    startDate = cachedEvents.Last().LocalTimestamp.DateTime;
                    if (startDate > DateTime.Now.AddHours(-1))
                        return cachedEvents;
                }
            }
            catch (KeyNotFoundException keyNotFound)
            {
                cacheEmpty = true;
            }



            var startDateString = startDate.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
            var endDateString = startDate.AddDays(3).ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);

            var requestUrl = string.Format(URL_STATUS_EVENTS_BY_ARRAY_BETWEEN_DATES, vehicleId, startDateString, endDateString);
            var response = await RestService.GetServiceResponse<Events>(requestUrl);


            try
            {
                Events responseToken = (Events)response.ResponseToken;

                List<Event> newEvents = responseToken.Items;

                if (!cacheEmpty)
                {
                    var combinedList = cachedEvents.Concat(newEvents).Where(x => x.LocalTimestamp >= DateTime.Now).ToList();

                    await cache.InsertObject<List<Event>>(key, combinedList, TimeSpan.FromDays(3));

                    return combinedList;
                }

                await cache.InsertObject<List<Event>>(key, newEvents, TimeSpan.FromDays(3));

                return newEvents;
            }
            catch (InvalidCastException e)
            {
                Log.Debug(e.Message);
                return null;
            }
        }

        public static async Task<Event> GetLatestEventsAsync(string vehicleGroup, string eventType, string eventId)
        {
            var requestUrl = string.Format(URL_LATEST_EVENTS, vehicleGroup, eventType, eventId);
            var response = await RestService.GetServiceResponse<Events>(requestUrl);

            try
            {
                Events responseToken = (Events)response.ResponseToken;

                return responseToken.Items.FirstOrDefault();
                //if (!(responseToken.TotalResults > responseToken.PageSize))
                //    return responseToken.Items;
                //else
                //{
                //    decimal count = (decimal)responseToken.TotalResults / responseToken.PageSize;
                //    var pages = Math.Ceiling(count);

                //    for (int i = 1; i < 5; i++)
                //    {
                //        var url = String.Format(URL_EVENTS, vehicleGroup, startDate, endDate, i);
                //        var resp = await RestService.GetServiceResponse<Events>(url);
                //        responseToken.Items.AddRange(((Events)resp.ResponseToken).Items);
                //    }
                //    return responseToken.Items;
                //}
            }
            catch (InvalidCastException e)
            {
                Log.Debug(e.Message);
                return null;
            }
        }

    }
}
