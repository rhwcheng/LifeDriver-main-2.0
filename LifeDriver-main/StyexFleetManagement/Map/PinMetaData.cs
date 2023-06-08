using System;
using FFImageLoading.Forms;
using StyexFleetManagement.Models;
using StyexFleetManagement.Resx;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using static StyexFleetManagement.Models.MapMarker;

namespace StyexFleetManagement.Map
{
    public class PinMetadata
    {
        public string Id { get; set; }
        public string VehicleId { get; set; }
        public int TripId { get; set; }
        public string Description { get; set; }

        public MapMarkerType MarkerState { get; set; }

        public string ImageFileName { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public string Location { get; set; }
        public string Violation { get; set; }
        public string Limit { get; set; }
        public byte[] Icon { get; set; }
        public Event LastEvent { get; internal set; }


        public PinMetadata() : base()
        {
            MarkerState = MapMarkerType.PURPLE;
        }

        private string GetMarkerStateString()
        {
            switch (MarkerState)
            {
                case MapMarker.MapMarkerType.GREEN:
                    return AppResources.label_in_trip;
                case MapMarker.MapMarkerType.AMBER:
                    return AppResources.label_is_idling;
                case MapMarker.MapMarkerType.RED:
                    return AppResources.label_requires_attention;
                case MapMarker.MapMarkerType.BLACK:
                    return AppResources.label_not_in_trip;
                case MapMarker.MapMarkerType.PURPLE:
                    return AppResources.last_known_position;
                default:
                    return "";
            }
        }

        public override string ToString()
        {
            if (LastEvent != null && LastEvent.Speed != 0)
                return $"{Description} {GetMarkerStateString()} ({LastEvent.Speed} {AppResources.speed_km_h_abbr})";
            else
                return $"{Description} {GetMarkerStateString()}";
        }
    }
}