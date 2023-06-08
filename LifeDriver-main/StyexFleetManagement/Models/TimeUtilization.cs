using System.Collections.Generic;
using Newtonsoft.Json;

namespace StyexFleetManagement.Models
{
	public class TimeUtilization
	{
        [JsonProperty("VehicleGroupId")] 
		public string VehicleGroupId { get; set; }

		[JsonProperty("DateRangeBegin")]
		public string DateRangeBegin { get; set; }

		[JsonProperty("DateRangeEnd")]
		public string DateRangeEnd { get; set; }

		[JsonProperty("VehicleCount")]
		public int VehicleCount { get; set; }

		[JsonProperty("VehicleCountWithoutWorkHours")]
		public int VehicleCountWithoutWorkHours { get; set; }

		[JsonProperty("TotalTimeProfileTime")]
		public int TotalTimeProfileTime { get; set; }

		[JsonProperty("TotalDrivingTime")]
		public int TotalDrivingTime { get; set; }

		[JsonProperty("TotalIdleDuration")]
		public int TotalIdleDuration { get; set; }

		[JsonProperty("MostUtilized")]
		public List<MostUtilized> MostUtilized { get; set; }

		[JsonProperty("LeastUtilized")]
		public List<LeastUtilized> LeastUtilized { get; set; }

		[JsonProperty("UtilizationHistogram")]
		public List<UtilizationHistogram> UtilizationHistogram { get; set; }
	}

	public class MostUtilized
	{
		[JsonProperty("Id")]
		public string Id { get; set; }

		[JsonProperty("Description")]
		public string Description { get; set; }

		[JsonProperty("DrivingTime")]
		public int DrivingTime { get; set; }

		[JsonProperty("TimeProfileTime")]
		public int TimeProfileTime { get; set; }

		[JsonProperty("IdleDuration")]
		public int IdleDuration { get; set; }
	}

	public class LeastUtilized
	{
		[JsonProperty("Id")]
		public string Id { get; set; }

		[JsonProperty("Description")]
		public string Description { get; set; }

		[JsonProperty("DrivingTime")]
		public int DrivingTime { get; set; }

		[JsonProperty("TimeProfileTime")]
		public int TimeProfileTime { get; set; }

		[JsonProperty("IdleDuration")]
		public int IdleDuration { get; set; }
	}

	public class UtilizationHistogram
	{
		[JsonProperty("Bin")]
		public int Bin { get; set; }

		[JsonProperty("Value")]
		public int Value { get; set; }
	}
}

