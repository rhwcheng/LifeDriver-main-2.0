using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Newtonsoft.Json;
using StyexFleetManagement.Helpers;
using StyexFleetManagement.Services;
using Xamarin.Forms;

namespace StyexFleetManagement.Models
{
	public class Trip : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

		#endregion
        /* Deprecated
		[JsonProperty("UnitId")]
		public string UnitId { get; set; }

		[JsonProperty("VehicleId")]
		public string VehicleId { get; set; }

		[JsonProperty("Description")]
		public string Description { get; set; }

		[JsonProperty("NumberOfTrips")]
		public int NumberOfTrips { get; set; }

		[JsonProperty("TotalDistance")]
		public double TotalDistance { get; set; }

		[JsonProperty("TotalDuration")]
		public double TotalDuration { get; set; }

		[JsonProperty("TotalDurationAsIso")]
		public string TotalDurationAsIso { get; set; }

		[JsonProperty("MinDistance")]
		public double MinDistance { get; set; }

		[JsonProperty("MinDuration")]
		public double MinDuration { get; set; }

		[JsonProperty("MinDurationAsIso")]
		public string MinDurationAsIso { get; set; }

		[JsonProperty("MaxDistance")]
		public double MaxDistance { get; set; }

		[JsonProperty("MaxDuration")]
		public double MaxDuration { get; set; }

		[JsonProperty("MaxDurationAsIso")]
		public string MaxDurationAsIso { get; set; }

		public Trip()
		{
		}*/

        public int Id { get; set; }
        public string UnitId { get; set; }
        public string VehicleId { get; set; }
        public DateTimeOffset StartLocalTimestamp { get; set; }

        [JsonIgnore]
        public DateTime StartLocalTimestampToLocalTime {
            get
            {
                var dateTimeOffset = new DateTimeOffset(StartLocalTimestamp.UtcDateTime, TimeSpan.FromHours(2));
                return dateTimeOffset.LocalDateTime;
            }
        }
        public List<double?> StartPosition { get; set; }
        public string StartLocation { get; set; }
        public DateTimeOffset EndLocalTimestamp { get; set; }
        [JsonIgnore]
        public string EndTime => EndLocalTimestamp.ToString("HH:mm tt");

        [JsonIgnore]
        public string StartTime => StartLocalTimestamp.ToString("HH:mm tt");

        public List<double?> EndPosition { get; set; }
        public string EndLocation { get; set; }
        public double Distance { get; set; }
        [JsonIgnore]
        public string DistanceAsString => FormatHelper.FormatDistance(Math.Round(this.Distance,2).ToString()).ToString();

        [JsonIgnore]
        public double Duration
        {
            get
            {
                if (EndLocalTimestamp == null || StartLocalTimestamp == null)
                    return 0;
                return (EndLocalTimestamp - StartLocalTimestamp).TotalSeconds;
            }
        }
        [JsonIgnore]
        public string DurationAsString
        {
            get
            {
                TimeSpan duration = EndLocalTimestamp - StartLocalTimestamp;
                return FormatHelper.ToShortForm(duration).ToString();
            }
        }
        [JsonIgnore]
        public string Url { get;set; }
        [JsonIgnore]
        private ImageSource mapPreviewSource;
        [JsonIgnore]
        public ImageSource MapPreviewSource => ImageSource.FromUri(new Uri(Url));

        [JsonIgnore]
        public List<Event> FleetEvents { get; set; }
        [JsonIgnore]
        public List<TripException> Exceptions { get; set; }

        [JsonIgnore]
        public List<Event> UBIEvents { get; set; }
        public bool? IsBusiness { get; set; }
        public int? DriverKeyCode { get; set; }
        [JsonIgnore]
        public int NumberOfExceptions
        {
            get
            {
                var plotFleetEvents = Settings.Current.PlotFleetExceptions;
                var plotUbiEvents = Settings.Current.PlotUBIExceptions;
                if (plotFleetEvents)
                {
                    if (FleetEvents != null)
                        return FleetEvents.Count;
                }
                else
                {
                    if (UBIEvents != null)
                        return UBIEvents.Count;
                }
                return 0;
            }
        }

        [JsonIgnore]
        public long StopDuration
        {
            get
            {
                if (PreviousTripEnd == 0)
                    return 0;

                return StartLocalTimestamp.Ticks - PreviousTripEnd;
            }
        }
        [JsonIgnore]
        public TimeSpan StoppedDuration { get; set; }
        [JsonIgnore]
        public string StopDurationAsString { get; set; }

        [JsonIgnore]
        public long PreviousTripEnd { get; set; }
        [JsonIgnore]
        public double FuelConsumption { get; set; }
        [JsonIgnore]
        public string FuelConsumptionAsString
        {
            get
            {
                if (FuelConsumption == 0)
                    return "---";
                else
                    return $"{Math.Round(this.FuelConsumption, 1).ToString()} litres".ToString();
            }
        }


        public string UtcLastUpdated { get; set; }
        public double MaxSpeed { get; set; }
        [JsonIgnore]
        public string MaxSpeedAsString => FormatHelper.FormatSpeed(Math.Round(this.MaxSpeed).ToString()).ToString();

        [JsonIgnore]
        public bool IsSelected { get; set; }
        public string DateSort => EndLocalTimestamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);

        public Trip()
        {
            FleetEvents = new List<Event>();
            UBIEvents = new List<Event>();
            Exceptions = new List<TripException>();
        }
        

        public override string ToString()
        {
            return StartLocalTimestamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern) + " - " + Distance;
        }

        public static async Task<double> GetFuelConsumption(Trip selectedTrip)
        {
            var consumptionEvent = (await EventAPI.GetEventsById(selectedTrip.VehicleId, 172, selectedTrip.EndLocalTimestamp.AddMinutes(-10).DateTime, selectedTrip.EndLocalTimestamp.AddMinutes(10).DateTime, true)).Where(x => x.FuelData != null && x.FuelData.Reason == 2).FirstOrDefault();
            if (consumptionEvent != default(Event))
            {
                return consumptionEvent.FuelData.AverageFuelConsumption.Value / 1000;
            }
            else
            {
                return 0;
            }
        }

        public static async Task SetExceptions(List<Trip> trips)
        {
            var groupedList = trips.GroupBy(x => x.VehicleId);

            foreach (var group in groupedList)
            {
                if (group.FirstOrDefault() != default(Trip))
                {
                    var earliestTrip = group.MinBy(x => x.StartLocalTimestamp).First();
                    var latestTrip = group.MaxBy(x => x.EndLocalTimestamp).First();

                    var plotFleetEvents = Settings.Current.PlotFleetExceptions;
                    var plotUbiEvents = Settings.Current.PlotUBIExceptions;


                    if (plotFleetEvents)
                    {
                        var fleetEvents = await EventAPI.GetEventsByArrayAsync(group.First().VehicleId, earliestTrip.StartLocalTimestamp.Date, latestTrip.EndLocalTimestamp.DateTime, EventTypeGroup.FleetExceptions);//await EventAPI.GetVehicleExceptionEventsBetweenDates(group.First().VehicleId, EventType.FLEET_EXCEPTION, earliestTrip.StartLocalTimestamp.Date, latestTrip.EndLocalTimestamp.DateTime, true);

                        if (fleetEvents != null && fleetEvents.Count > 0)
                        {
                            foreach (Trip trip in group)
                            {
                                trip.FleetEvents = fleetEvents.Where(x => x.LocalTimestamp >= trip.StartLocalTimestamp.DateTime && x.LocalTimestamp <= trip.EndLocalTimestamp.DateTime).ToList();
                            }

                        }
                    }
                    if (plotUbiEvents)
                    {
                        var ubiEvents = await EventAPI.GetEventsByArrayAsync(group.First().VehicleId, earliestTrip.StartLocalTimestamp.Date, latestTrip.EndLocalTimestamp.DateTime, EventTypeGroup.UBIExceptions);//await EventAPI.GetVehicleExceptionEventsBetweenDates(group.First().VehicleId, EventType.UBI_EXCEPTION, earliestTrip.StartLocalTimestamp.DateTime, latestTrip.EndLocalTimestamp.DateTime, true);

                        if (ubiEvents != null && ubiEvents.Count > 0)
                        {
                            foreach (Trip trip in group)
                            {
                                trip.UBIEvents = ubiEvents.Where(x => x.LocalTimestamp >= trip.StartLocalTimestamp.DateTime && x.LocalTimestamp <= trip.EndLocalTimestamp.DateTime).ToList();
                            }
                        }

                    }
                }
                
            }
           
        }
    }

    public class TripsResponse
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
        public List<Trip> Items { get; set; }
        public bool HasMoreResults { get; set; }

        public TripsResponse()
        {
            Items = new List<Trip>();
        }
    }
}

