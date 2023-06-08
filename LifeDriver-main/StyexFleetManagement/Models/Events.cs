using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StyexFleetManagement.Models
{
   

    public class EndOfExceptionData
    {
        public double Limit { get; set; }
        public double Maximum { get; set; }
        public int Duration { get; set; }
    }

    public class Event
    {
        public object Id { get; set; }
        [JsonIgnore]
        public string VehicleId { get; set; }
        public string UnitId { get; set; }
        public int EventTypeId { get; set; }
        public string EventTypeDescription { get; set; }
        public DateTimeOffset LocalTimestamp { get; set; }
        public int Odometer { get; set; }
        public double Speed { get; set; }
        public string UnitOfDistanceCode { get; set; }
        public List<double> Position { get; set; }
        public double? Direction { get; set; }
        public int? DriverKeyCode { get; set; }
        public FuelData FuelData { get; set; }
        public int? RPM { get; set; }
        public EndOfExceptionData EndOfExceptionData { get; set; }
    
        public StartOfExceptionData StartOfExceptionData { get; set; }
        public DriverBehaviourData DriverBehaviourData { get; set; }
        public ExcessiveIdleData ExcessiveIdleData { get; set; }
        [JsonIgnore]
        public string VehicleDescription { get; set; }
        [JsonIgnore]
        public string Lattitude
        {
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
    public class ExcessiveIdleData
    {
        public int Duration { get; set; }
    }

    public class StartOfExceptionData
    {
        public double Limit { get; set; }
    }

    public class DriverBehaviourData
    {
        public double Limit { get; set; }
        public double Maximum { get; set; }
        public int Duration { get; set; }
    }

    public class Events
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
        public List<Event> Items { get; set; }
        public bool HasMoreResults { get; set; }

        public Events()
        {
            Items = new List<Event>();
        }
    }
    
}
