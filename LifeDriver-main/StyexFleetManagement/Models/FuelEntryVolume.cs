using System;
using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
	public class FuelEntryVolume
	{
		public string VehicleGroupId { get; set; }
		public string ValueType { get; set; }
		public string PeriodType { get; set; }
		public string DateRangeBegin { get; set; }
		public string DateRangeEnd { get; set; }
		public List<VolumeItem> Items { get; set; }
		public List<Distance> Distances { get; set; }
    }

	public class VolumeItem
	{
		public DateTime PeriodStart { get; set; }
		public double Value { get; set; }
	}

	public class Distance
	{
		public DateTime PeriodStart { get; set; }
		public double Value { get; set; }
	}
}

