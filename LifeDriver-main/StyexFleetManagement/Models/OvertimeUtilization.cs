using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
	public class OvertimeUtilization
	{
        public string VehicleGroupId { get; set; }
		public string DateRangeBegin { get; set; }
		public string DateRangeEnd { get; set; }
		public int TotalDrivingTimeInsideProfileTime { get; set; }
		public int TotalDrivingTimeOutsideProfileTime { get; set; }
		public List<BestUtilized> BestUtilized { get; set; }
		public List<WorstUtilized> WorstUtilized { get; set; }
	}

	public class BestUtilized
	{
		public string Id { get; set; }
		public string Description { get; set; }
		public int DrivingTimeInsideProfileTime { get; set; }
		public int DrivingTimeOutsideProfileTime { get; set; }
		public int NumberOfDrivers { get; set; }
	}

	public class WorstUtilized
	{
		public string Id { get; set; }
		public string Description { get; set; }
		public int DrivingTimeInsideProfileTime { get; set; }
		public int DrivingTimeOutsideProfileTime { get; set; }
		public int NumberOfDrivers { get; set; }
	}
}

