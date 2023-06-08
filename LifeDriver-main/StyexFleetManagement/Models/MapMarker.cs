using System;
using Xamarin.Forms;

namespace StyexFleetManagement.Models
{
	public class MapMarker
	{
		public MapMarker()
		{
		}

		public string Title;
		public string Summary;
		public bool IsChecked;
		public MapMarkerType Type;

		public MapMarker(string title, string summary, MapMarkerType type)
		{
			Title = title;
			Summary = summary;
			Type = type;
			IsChecked = App.GetInstance().GetMapShowVehicles(type);
		}

		public ImageSource GetIcon()
		{
			switch (Type)
			{
				case MapMarkerType.GREEN:
					return ImageSource.FromFile("ic_map_marker_green.png");
				case MapMarkerType.AMBER:
					return ImageSource.FromFile("ic_map_marker_yellow.png");
				case MapMarkerType.RED:
					return ImageSource.FromFile("ic_map_marker_red.png");
				case MapMarkerType.BLACK:
					return ImageSource.FromFile("ic_map_marker_black.png");
                case MapMarkerType.PURPLE:
                    return ImageSource.FromFile("ic_map_marker_purple.png");
                default:
					return null;
			}
		}

		public enum MapMarkerType
		{
			GREEN,
			AMBER,
			RED,
			BLACK,
            PURPLE
		}
	}
}

