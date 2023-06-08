using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using ProtoBuf;
using StyexFleetManagement.Helpers;

namespace StyexFleetManagement.Models
{
    [ProtoContract]
    public class LogbookTrip : BaseDataObject
    {

       

        [ProtoMember(3)]
        public List<Coordinate> Coordinates { get; set; }
        public string SerializedString { get; set; }

        [ProtoMember(4)]
        public string StartLocation { get; internal set; }

        [ProtoMember(5)]
        public string EndLocation { get; internal set; }
        
        public DateTimeOffset StartLocalTimestamp { get; set; }

        public DateTimeOffset EndLocalTimestamp { get; set; }
        public string UnitId { get; set; }

        public uint TripId { get; set; }

        [ProtoMember(6)]
        public string StartLocalTimestampString { get => StartLocalTimestamp.ToString("o");
            set => StartLocalTimestampString = value;
        }

        [ProtoMember(7)]
        public string EndLocalTimestampString { get => EndLocalTimestamp.ToString("o");
            set => EndLocalTimestampString = value;
        }

        [ProtoMember(8)]
        public double Distance { get; set; }
        public bool IsComplete { get; set; }

        public IList<TripPoint> Points { get; set; }
        public TimeSpan Duration
        {
            get;set;
        }

        #region View cell properties
        [JsonIgnore]
        public string DistanceString => FormatHelper.FormatDistance(Math.Round(Distance,2).ToString()).ToString();

        [JsonIgnore]
        public string DurationString => FormatHelper.ToShortForm(this.Duration);

        [JsonIgnore]
        public string Url { get; set; }

        [JsonIgnore]
        public string StartTime => this.StartLocalTimestamp.ToString("HH:mm tt");


        [JsonIgnore]
        public string EndTime => this.EndLocalTimestamp.ToString("HH:mm tt");

        public uint DriverId { get; internal set; }
        public double StartLatitude { get; internal set; }
        public double StartLongitude { get; internal set; }
        #endregion

        public LogbookTrip() : base()
        {
            //Random uint
            var random = new Random();
            uint thirtyBits = (uint)random.Next(1 << 30);
            uint twoBits = (uint)random.Next(1 << 2);
            uint fullRange = (thirtyBits << 2) | twoBits;
            TripId = fullRange;

            Coordinates = new List<Coordinate>();
        }
    }
    [DataContract]
    public class Coordinate
    {
        public Coordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
        [DataMember]

        public double Latitude { get; set; }
        [DataMember]
        public double Longitude { get; set; }

        public override string ToString()
        {
            return $"Lat: {Math.Round(Latitude, 3).ToString()}, Lng: {Math.Round(Longitude, 3).ToString()}";
        }
    }

    public class TripBlob : BaseDataObject
    {
        public byte[] Blob { get; set; }
        public uint TemplateId { get; set; }

    }

    public class TripPoint : BaseDataObject
    {
        public string TripId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        /// <summary>
        ///     Gets or sets the speed, in km/h
        /// </summary>
        /// <value>The speed.</value>
        public double Speed { get; set; }

        public DateTime RecordedTimeStamp { get; set; }

        /// <summary>
        ///     Gets or sets the sequence order number starting at 0
        /// </summary>
        /// <value>The sequence.</value>
        public int Sequence { get; set; }
        public double Bearing { get; set; }
    }
}