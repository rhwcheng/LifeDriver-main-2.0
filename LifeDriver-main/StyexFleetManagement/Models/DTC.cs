using System;

namespace StyexFleetManagement.Models
{
    public class DTCEvent
    {
        public int Id { get; set; }
        public string ErrorCode { get; set; }
        public string Description { get; set; }
        public string EventId { get; set; }
        public string VehicleId { get; set; }
        public DateTimeOffset LocalTimestamp { get; set; }
    }
}
