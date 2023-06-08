using System;
using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
    public class TripException
    {
        public DateTime LocalTimestamp { get; set; }
        public List<double> Position { get; set; }
        public string Description { get; set; }
        public int EventTypeId { get; set; }
    }
}
