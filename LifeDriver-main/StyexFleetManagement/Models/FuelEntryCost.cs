using System;
using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
	public class FuelEntryCost
	{
		public string VehicleGroupId { get; set; }
		public string ValueType { get; set; }
		public string PeriodType { get; set; }
		public string DateRangeBegin { get; set; }
		public string DateRangeEnd { get; set; }
		public List<CostItem> Items { get; set; }
		public List<Distance> Distances { get; set; }
    }

	public class CostItem
	{
		public DateTime PeriodStart { get; set; }
		public double Value { get; set; }
		public int NonDefaultCurrencyEntryCount { get; set; }
	}

	public class CostDistance
	{
		public DateTime PeriodStart { get; set; }
		public double Value { get; set; }
	}

}

