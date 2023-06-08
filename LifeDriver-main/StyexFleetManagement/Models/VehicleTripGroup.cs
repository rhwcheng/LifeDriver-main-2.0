using System.Collections.ObjectModel;

namespace StyexFleetManagement.Models
{
	public class VehicleTripGroup: ObservableCollection<VehicleTrip>
	{
		public string Title { get; set; }

		private VehicleTripGroup(string title)
		{
			Title = title;
		}

		public static ObservableCollection<VehicleTripGroup> All { private set; get; }

		static VehicleTripGroup()
		{
			ObservableCollection<VehicleTripGroup> Groups = new ObservableCollection<VehicleTripGroup>();

			All = Groups;
		}



	}
}

