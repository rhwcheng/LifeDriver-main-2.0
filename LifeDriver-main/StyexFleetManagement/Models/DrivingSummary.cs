using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
	public class DrivingSummary
	{
        public string VehicleGroupId { get; set; }
		public string DateRangeBegin { get; set; }
		public string DateRangeEnd { get; set; }
		public int TotalDrivingTime { get; set; }
		public double TotalDistance { get; set; }
		public int VehicleCount { get; set; }
		public List<TopDrivingDistanceVehicle> TopDrivingDistanceVehicles { get; set; }
		public List<TopDrivingTimeVehicle> TopDrivingTimeVehicles { get; set; }

	}

	public class TopDrivingDistanceVehicle
	{
		public string Id { get; set; }
		public string Description { get; set; }
		public double Distance { get; set; }
	}

	public class TopDrivingTimeVehicle
	{
		public string Id { get; set; }
		public string Description { get; set; }
		public int DrivingTime { get; set; }
	}

}

