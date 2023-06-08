using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;

namespace StyexFleetManagement.Models
{
	public class VehicleTrips
	{
		public int Page { get; set; }
		public int PageSize { get; set; }
		public int TotalResults { get; set; }
		public List<VehicleTrip> Items { get; set; }
		public bool HasMoreResults { get; set; }

        public VehicleTrips()
		{
			Items = new List<VehicleTrip>();
        }


	}

	public class VehicleTrip : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged implementation

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		public int Id { get; set; }
		public DateTimeOffset StartLocalTimestamp { get; set; }
		public List<double> StartPosition { get; set; }
		public DateTimeOffset EndLocalTimestamp { get; set; }
		public List<double> EndPosition { get; set; }
		public double Distance { get; set; }
		public string StartLocation { get; set; }
		public string EndLocation { get; set; }
		public bool IsBusiness { get; set; }
		public int NumberOfExceptions { get; set; }

        [JsonIgnore]
        public bool IsSelected { get; set; }
        public int NumberOfDriverBehaviourExceptions { get; set; }	

		public string DateSort => EndLocalTimestamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);

        public override string ToString()
		{
			return StartLocalTimestamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern) + " - " + Distance;
		}
	}
}

