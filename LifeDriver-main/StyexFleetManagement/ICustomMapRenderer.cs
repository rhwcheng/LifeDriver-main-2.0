using System;
using System.Collections.Generic;
using StyexFleetManagement.Models;

namespace StyexFleetManagement
{
	public interface ICustomMapRenderer
	{
		void PlotTrip(VehicleTrip trip, List<List<float>> tripPositions, List<TripFleetException> tripExceptions);
	}
}

