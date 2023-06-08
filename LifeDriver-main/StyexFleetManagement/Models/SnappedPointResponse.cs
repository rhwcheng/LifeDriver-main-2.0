using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
    public class Location
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }

        
    }

    public class SnappedPoint
    {
        public Location Location { get; set; }
        public int OriginalIndex { get; set; }
        public string PlaceId { get; set; }
    }

    public class SnappedPointResponse
    {
        public List<SnappedPoint> SnappedPoints { get; set; }
    }
}
