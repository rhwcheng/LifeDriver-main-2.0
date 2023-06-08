using System;
using System.ComponentModel;
using FFImageLoading.Forms;
using Newtonsoft.Json;

namespace StyexFleetManagement.Models
{
	public class Vehicle : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		Guid id;
		public string Description { get; set; }

		public UnitLocation LastKnownLocation;

        [JsonIgnore]
		public bool IsSelected;

        [JsonIgnore]
        public string MarkerImageFileName { get; set; }

        [JsonIgnore]
        public string VehicleGroupName { get; set; }

        public long DrivingTime;
		public long DrivingTimeInsideProfileTime;
		public long DrivingTimeOutsideProfileTime;
		public long TimeProfileTime;
		public long IdleDuration;
		public int NumberOfDrivers;
		public float Volume;
		public float Distance;
		public float Consumption;
		public string FuelLevel;

		public CachedImage Icon { get; set;}

		public Guid Id
		{
			set
			{
				if (id != value)
				{
					id = value;

					if (PropertyChanged != null)
					{
						PropertyChanged(this,
							new PropertyChangedEventArgs("Id"));
					}
				}
			}
			get => id;
        }
		public Vehicle()
		{

        }

        public Vehicle(UnitLocation location)
        {
            Id = location.Id;
            Description = location.Description;
            LastKnownLocation = location;
        }

		public Vehicle(UnitLocation location, CachedImage image)
		{
			Id = location.Id;
			Description = location.Description;
			LastKnownLocation = location;

			Icon = image;
		}

		public override string ToString()
		{
			return Description;
		}
	}
}

