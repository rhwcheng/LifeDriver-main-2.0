using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StyexFleetManagement.Models
{
	public class Alert
    {
        public DateTimeOffset LocalTimestamp { get; set; }
        public int AlertType { get; set; }
        public long EventId { get; set; }
        public int EventTypeId { get; set; }
        public DateTime ProcessLocalTimestamp { get; set; }
        public int AlertLevel { get; set; }
        public int Id { get; set; }
        public string Description { get; set; }
        public bool Processed { get; set; }
        public List<double> Position { get; set; }
        [JsonIgnore]
        public string VehicleDescription { get; set; }
        [JsonIgnore]
        public string Lattitude{
            get
            {
                if (Position != null || Position.Count != 0)
                {
                    return Position[1].ToString();
                }
                else
                    return string.Empty;
            }
        }
        [JsonIgnore]
        public string Longitude
        {
            get
            {
                if (Position != null || Position.Count != 0)
                {
                    return Position[0].ToString();
                }
                else
                    return string.Empty;
            }
        }
        [JsonIgnore]
        public string DateString
        {
            get
            {
                if (LocalTimestamp != default(DateTimeOffset))
                {
                    return LocalTimestamp.DateTime.ToString("MMM dd HH:mm tt");
                }
                else
                    return string.Empty;
            }
        }
    }

	public class Alerts
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
		public List<Alert> Items { get; set; }
        public bool HasMoreResults { get; set; }
    }
}
