using System;
using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
    public class TripUBIExceptionList
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
        public List<TripUBIException> Items { get; set; }
        public bool HasMoreResults { get; set; }

        public TripUBIExceptionList()
        {
            Items = new List<TripUBIException>();
        }
    }

    public class TripUBIException
    {
        public DateTime LocalTimestamp { get; set; }
        public List<float> Position { get; set; }
        public string Description { get; set; }

        public TripUBIException()
        {
            Position = new List<float>();
        }
    }
   
}
