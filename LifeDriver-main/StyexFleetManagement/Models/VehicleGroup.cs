using System.Collections.Generic;
using Newtonsoft.Json;

namespace StyexFleetManagement.Models
{
	public class VehicleGroup
	{
		[JsonProperty("Id")]
		public string Id { get; set;}

		[JsonProperty("Description")]
		public string Description { get; set; }

		[JsonProperty("UtcLastModified")]
		public string UtcLastModified { get; set; }
    }

	public class VehicleGroupCollection
	{
		[JsonProperty("Items")]
		public List<VehicleGroup> VehicleGroups { get; set;}

		public string FindIdFromDescription(string description)
		{
			if (VehicleGroups.Count > 0)
			{
				foreach (VehicleGroup vehicleGroup in VehicleGroups)
				{
					if (vehicleGroup.Description.Equals(description))
					{
						return vehicleGroup.Id;
					}
				}

			}
			return "";
		}

		public string FindDescriptionFromId(string id)
		{
			
			if (VehicleGroups != null && VehicleGroups.Count > 0)
			{
				foreach (VehicleGroup vehicleGroup in VehicleGroups)
				{
					if (vehicleGroup.Id.Equals(id))
					{
						return vehicleGroup.Description;
					}
				}

			}
			return "";
		}
	}
}

