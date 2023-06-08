using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
	public class FuelSummary
	{
        public string VehicleGroupId { get; set; }
		public string DateRangeBegin { get; set; }
		public string DateRangeEnd { get; set; }
		public int VehicleCount { get; set; }
		public double TotalVolume { get; set; }
		public double TotalCost { get; set; }
		public double AverageConsumption { get; set; }
		public List<CurrentFuelLevel> CurrentFuelLevel { get; set; }
		public List<TopFuelVolume> TopFuelVolume { get; set; }
		public List<TopFuelConsumption> TopFuelConsumption { get; set; }


	}

	public class CurrentFuelLevel
	{
		public string Id { get; set; }
		public string Description { get; set; }
		public double Distance { get; set; }
		public double? FuelLevel { get; set; }
	}

	public class TopFuelVolume
	{
		public string Id { get; set; }
		public string Description { get; set; }
		public double Volume { get; set; }
		public double Distance { get; set; }
		public double Consumption { get; set; }
	}

	public class TopFuelConsumption
	{
		public string Id { get; set; }
		public string Description { get; set; }
		public double Volume { get; set; }
		public double Distance { get; set; }
		public double Consumption { get; set; }
	}

	public class RootObject
	{

	}

}

