namespace StyexFleetManagement.Models
{
    public class AggregatedTripData
    {
        public string VehicleId { get; set; }
        public string UnitId { get; set; }
        public string Description { get; set; }
        public int NumberOfTrips { get; set; }
        public double TotalDistance { get; set; }
        public int TotalDuration { get; set; }
        public string TotalDurationAsIso { get; set; }
        public double MinDistance { get; set; }
        public int MinDuration { get; set; }
        public string MinDurationAsIso { get; set; }
        public double MaxDistance { get; set; }
        public int MaxDuration { get; set; }
        public string MaxDurationAsIso { get; set; }
    }
}
