using System;
using StyexFleetManagement.Models;
using Xamarin.Forms;

namespace StyexFleetManagement
{
	public class ToggleMapMarker : RelativeLayout
	{
		private MapMarker.MapMarkerType markerType;
		private bool isSelected;
		private Image markerIcon;
		private Label vehicleCount;

		public ToggleMapMarker(MapMarker.MapMarkerType type)
		{
			markerIcon = new Image();
			vehicleCount = new Label();

			switch (type)
			{
				case (MapMarker.MapMarkerType.AMBER):
					markerIcon.Source = ImageSource.FromFile("marker_amber.png");
					break;
				case (MapMarker.MapMarkerType.GREEN):
					markerIcon.Source = ImageSource.FromFile("marker_green.png");
					break;

			}


		}

	}
}
