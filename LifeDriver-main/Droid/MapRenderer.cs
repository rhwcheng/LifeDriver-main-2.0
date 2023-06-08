using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using StyexFleetManagement.Map;
using StyexFleetManagement.Resx;
using System;
using System.Collections.Generic;
using System.Linq;
using StyexFleetManagement.Droid;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(ExtendedMap), typeof(MapRenderer))]

namespace StyexFleetManagement.Droid
{
    public class MapRenderer : Xamarin.Forms.GoogleMaps.Android.MapRenderer, GoogleMap.IInfoWindowAdapter
    {
        IList<Pin> _pins;

        public MapRenderer(Context context) : base(context)
        { }

        protected override void OnMapReady(GoogleMap nativeMap, Xamarin.Forms.GoogleMaps.Map map)
        {
            base.OnMapReady(nativeMap, map);

            nativeMap.SetInfoWindowAdapter(this);
            nativeMap.UiSettings.ZoomControlsEnabled = false;
        }

        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Xamarin.Forms.GoogleMaps.Map> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement == null) return;

            var formsMap = (Xamarin.Forms.GoogleMaps.Map)e.NewElement;
            _pins = formsMap.Pins;
        }

        View GoogleMap.IInfoWindowAdapter.GetInfoContents(Marker marker)
        {
            if (!(Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) is LayoutInflater inflater)) return null;

            Android.Views.View view;

            var pin = GetPinByMarker(marker);
            if (pin == null)
            {
                throw new Exception("Pin not found");
            }

            var pinMetadata = (PinMetadata)pin.Tag;

            switch (pinMetadata.Id)
            {
                case MapPage.TripExceptionMarker:
                case MapPage.VehicleMarker:
                    view = inflater.Inflate(Resource.Layout.ExceptionMapInfoWindow, null);
                    break;
                case "finish":
                case "start":
                    {
                        view = inflater.Inflate(Resource.Layout.TripStartAndEndInfoWindow, null);

                        var tripTitle = view.FindViewById<TextView>(Resource.Id.TripTitleLabel);
                        var address = view.FindViewById<TextView>(Resource.Id.AddressLabel);
                        var time = view.FindViewById<TextView>(Resource.Id.TimeLabel);

                        tripTitle.Text = pinMetadata.Id == "start" ? AppResources.trip_start_label : AppResources.trip_end_label;
                        address.Text = pinMetadata.Description;
                        time.Text = pinMetadata.Date;

                        return view;
                    }
                default:
                    view = inflater.Inflate(Resource.Layout.MapInfoWindow, null);
                    break;
            }

            var title = view.FindViewById<TextView>(Resource.Id.TitleLabel);
            var date = view.FindViewById<TextView>(Resource.Id.DateLabel);
            var violationTitle = view.FindViewById<TextView>(Resource.Id.ViolationTitleLabel);
            var violation = view.FindViewById<TextView>(Resource.Id.ViolationDataLabel);
            var limitTitle = view.FindViewById<TextView>(Resource.Id.LimitTitleLabel);
            var limit = view.FindViewById<TextView>(Resource.Id.LimitDataLabel);
            var icon = view.FindViewById<ImageView>(Resource.Id.PinIcon);

            if (title != null)
            {
                title.Text = pinMetadata.Description;
            }
            if (date != null)
            {
                date.Text = pinMetadata.Date;
            }
            if (pinMetadata.Id == MapPage.VehicleMarker)
            {
                if (date != null) date.Text = pinMetadata.Location;
                if (title != null) title.Text = pinMetadata.ToString();
            }
            if (violation != null && pinMetadata.Violation != null)
            {
                if (violationTitle != null)
                {
                    violationTitle.Text = "Violation:";
                }
                violation.Text = pinMetadata.Violation;
            }
            if (limit != null && pinMetadata.Violation != null)
            {
                if (limitTitle != null)
                {
                    limitTitle.Text = $"{AppResources.limit_label}:";
                }
                limit.Text = pinMetadata.Limit;
            }

            if (pinMetadata.Icon == null || pinMetadata.Icon.Length <= 0) return view;

            var bmp = BitmapFactory.DecodeByteArray(pinMetadata.Icon, 0, pinMetadata.Icon.Length);
            icon.SetImageBitmap(bmp);

            return view;
        }

        private Pin GetPinByMarker(Marker marker)
        {
            var position = new Position(marker.Position.Latitude, marker.Position.Longitude);
            return _pins.FirstOrDefault(pin => pin.Position == position);
        }

        Android.Views.View Android.Gms.Maps.GoogleMap.IInfoWindowAdapter.GetInfoWindow(Android.Gms.Maps.Model.Marker marker)
        {
            return null;
        }
    }
}