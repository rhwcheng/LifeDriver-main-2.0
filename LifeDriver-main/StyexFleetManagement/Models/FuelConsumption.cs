using System;
using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
	public class FuelConsumption
	{
        public string VehicleGroupId { get; set; }
		public string ValueType { get; set; }
		public string PeriodType { get; set; }
		public string DateRangeBegin { get; set; }
		public string DateRangeEnd { get; set; }
		public List<ConsumptionItem> Items { get; set; }


	}

	public class ConsumptionItem
	{
		public DateTime PeriodStart { get; set; }
		public double Value { get; set; }
	}

    public class MonthlyFuelConsumption
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public double? Cost { get; set; }
        public double? Distance { get; set; }
        public List<object> RefuelingCosts { get; set; }

    }




}

