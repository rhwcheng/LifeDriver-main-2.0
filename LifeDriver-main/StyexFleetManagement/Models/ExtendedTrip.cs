using System.Collections.Generic;
using Newtonsoft.Json;

namespace StyexFleetManagement.Models
{
    public class ExtendedTripResponse
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
        public List<ExtendedTrip> Items { get; set; }
        public bool HasMoreResults { get; set; }
    }
    public class ExtendedTrip
    {
        public int Id { get; set; }
        public string UnitId { get; set; }
        public string VehicleId { get; set; }
        public string StartLocalTimestamp { get; set; }
        public List<double?> StartPosition { get; set; }
        public string StartLocation { get; set; }
        public string EndLocalTimestamp { get; set; }
        public List<double?> EndPosition { get; set; }
        public string EndLocation { get; set; }
        public double Distance { get; set; }
        public bool? IsBusiness { get; set; }
        public int NumberOfExceptions { get; set; }
        public int? DriverKeyCode { get; set; }
        public string UtcLastUpdated { get; set; }
        public string UtcUpdatedLastEvent { get; set; }
        public List<TripEvent> TripEvent { get; set; }
    }

    public class TripEvent
    {
        [JsonProperty("E")]
        public int EventType { get; set; }

        [JsonProperty("P")]
        public List<double?> Position { get; set; }

        [JsonProperty("PA")]
        public object PositionAccuracy { get; set; }

        [JsonProperty("T")]
        public string Timestamp { get; set; }

        [JsonProperty("S")]
        public double Speed { get; set; }

        [JsonProperty("R")]
        public int? RPM { get; set; }

        [JsonProperty("D")]
        public double? Direction { get; set; }

        [JsonProperty("A")]
        public List<double?> AccelerometerValue { get; set; }
    }

}
