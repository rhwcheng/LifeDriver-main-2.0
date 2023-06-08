﻿using System;
using FFImageLoading.Forms;
using StyexFleetManagement.Models;
using StyexFleetManagement.Resx;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using static StyexFleetManagement.Models.MapMarker;

namespace StyexFleetManagement.Map
{
    public class ExtendedPin
    {
        public Pin Pin { get; set; }
        public string Id { get; set; }
        public string VehicleId { get; set; }
        public int TripId { get; set; }
        public string Description { get; set; }

        private MapMarkerType _markerState;

        public MapMarkerType MarkerState
        {
            get => _markerState;
            set
            {
                if (_markerState == value) return;

                _markerState = value;
            }
        }

        public string ImageFileName { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public string Location { get; set; }
        public string Violation { get; set; }
        public string Limit { get; set; }
        public byte[] Icon { get; set; }
        public Event LastEvent { get; internal set; }


        public ExtendedPin() : base()
        {
            MarkerState = MapMarkerType.PURPLE;

            //IsVisible = true;

            //ShowCallout = true;

            //IsCalloutClickable = true;
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

        //private void SetPinColor()
        //{
        //    switch (MarkerState)
        //    {
        //        case MapMarker.MapMarkerType.GREEN:
        //            DefaultPinColor = Color.Green;
        //            break;
        //        case MapMarker.MapMarkerType.AMBER:
        //            DefaultPinColor = Color.Orange;
        //            break;
        //        case MapMarker.MapMarkerType.RED:
        //            DefaultPinColor = Color.Red;
        //            break;
        //        case MapMarker.MapMarkerType.BLACK:
        //            DefaultPinColor = Color.Black;
        //            break;
        //        case MapMarker.MapMarkerType.PURPLE:
        //            DefaultPinColor = Color.Purple;
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException();
        //    }
        //}

        public override string ToString()
        {
            if (LastEvent != null && LastEvent.Speed != 0)
                return $"{Description} {GetMarkerStateString()} ({LastEvent.Speed} {AppResources.speed_km_h_abbr})";
            else
                return $"{Description} {GetMarkerStateString()}";
        }
    }
}