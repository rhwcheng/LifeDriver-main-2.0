using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
    public class DriverEvent
    {
        public object Id { get; set; }
        public string UnitId { get; set; }
        public int EventTypeId { get; set; }
        public string EventTypeDescription { get; set; }
        public string LocalTimestamp { get; set; }
        public List<double> Position { get; set; }
        public double Speed { get; set; }
        public string UnitOfDistanceCode { get; set; }
        public DriverData DriverData { get; set; }
        public double? Direction { get; set; }
    }
}
