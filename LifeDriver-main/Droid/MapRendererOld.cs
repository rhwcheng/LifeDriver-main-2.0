//using StyexFleetManagement;
//using StyexFleetManagement.Droid;

//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using Android.Content;
//using Android.Gms.Maps;
//using Android.Gms.Maps.Model;
//using Android.Widget;
//using Xamarin.Forms;
//using Xamarin.Forms.Maps;
//using Xamarin.Forms.Maps.Android;
//using Android.Graphics;
//using System.Collections.ObjectModel;
//using System.Linq;
//using StyexFleetManagement.Map;
//using StyexFleetManagement.Models;
//using Xamarin.Forms.Platform.Android;
//using StyexFleetManagement.Resx;

//[assembly: ExportRenderer(typeof(AndroidCustomMap), typeof(StyexMapRenderer))]
//namespace StyexFleetManagement.Droid
//{
//    public class StyexMapRenderer : Xamarin.Forms.Maps.Android.MapRenderer, GoogleMap.IInfoWindowAdapter, IOnMapReadyCallback
//    {

//        GoogleMap map;
//        ObservableCollection<TripCoordinates> routeCoordinates;
//        ObservableCollection<AndroidCustomPin> customPins;
//        bool isDrawn = false;
//        bool infoWindowClicked = false;


//        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Xamarin.Forms.Maps.Map> e)
//        {
//            base.OnElementChanged(e);



//            if (e.OldElement != null)
//            {
//                // Unsubscribe
//                map.InfoWindowClick -= OnInfoWindowClick;
//            }

//            if (e.NewElement != null)
//            {
//                var formsMap = (AndroidCustomMap)e.NewElement;
//                routeCoordinates = formsMap.RouteCoordinates;
//                customPins = formsMap.CustomPins;

//                ((MapView)Control).GetMapAsync(this);
//            }
//        }


//        public void OnMapReady(GoogleMap googleMap)
//        {
//            map = googleMap;
//            map.InfoWindowClick += OnInfoWindowClick;
//            map.SetInfoWindowAdapter(this);
//            map.UiSettings.ZoomControlsEnabled = false;


//            foreach (var route in routeCoordinates)
//            {
//                var polylineOptions = new PolylineOptions();

//                polylineOptions.InvokeColor(Android.Graphics.Color.ParseColor(route.RouteColour));

//                foreach (var position in route.RouteCoordinates)
//                {
//                    polylineOptions.Add(new LatLng(position.Latitude, position.Longitude));
//                }


//                map.AddPolyline(polylineOptions);
//            }

//        }

//        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
//        {
//            base.OnElementPropertyChanged(sender, e);

//            if (map != null)
//            {
//                map.UiSettings.ZoomControlsEnabled = false;
//            }


//            if (e.PropertyName.Equals("HasScrollEnabled") || e.PropertyName.Equals("HasZoomEnabled"))
//            {

//                //TODO: FIX -- map.Clear();
//                if (!infoWindowClicked)
//                    map.Clear();
//                else
//                    infoWindowClicked = false;

//                foreach (var pin in customPins)
//                {
//                    var marker = new MarkerOptions();
//                    marker.SetPosition(new LatLng(pin.Pin.Position.Latitude, pin.Pin.Position.Longitude));
//                    marker.SetTitle(pin.Pin.Label);
//                    marker.SetSnippet(pin.Pin.Address);
//                    var fileName = pin.ImageFileName.Split('.')[0];
//                    var id = Resources.GetIdentifier(fileName, "drawable", MainActivity.PACKAGE_NAME);
//                    marker.SetIcon(BitmapDescriptorFactory.FromResource(id));

//                    if (pin.IsVisible)
//                        map.AddMarker(marker);
//                }

//                var formsMap = (AndroidCustomMap)sender;
//                formsMap.HasZoomEnabled = true;
//                formsMap.HasScrollEnabled = true;

//                if (formsMap.RouteCoordinates.Count != 0)
//                {
//                    routeCoordinates = formsMap.RouteCoordinates;
//                    customPins = formsMap.CustomPins;

//                    ((MapView)Control).GetMapAsync(this);
//                }
//                isDrawn = true;

//            }

//            if (e.PropertyName == AndroidCustomMap.RouteCoordinatesProperty.PropertyName)
//            {
//                //map.Clear();
//                var formsMap = (AndroidCustomMap)sender;
//                if (formsMap.RouteCoordinates.Count != 0)
//                {
//                    routeCoordinates = formsMap.RouteCoordinates;
//                    customPins = formsMap.CustomPins;

//                    ((MapView)Control).GetMapAsync(this);
//                }

//            }
//            if (e.PropertyName.Equals("IsShowingVisibilityControl"))
//            {
//                var formsMap = sender as AndroidCustomMap;
//                if (formsMap.IsShowingVisibilityControl)
//                {
//                    SetPadding();
//                }
//                else
//                {
//                    RemovePadding();
//                }
//            }
//        }

//        private void SetPadding()
//        {
//            var padding = (int)(App.ScreenWidth / 6 * Resources.DisplayMetrics.Density);
//            map.SetPadding(padding, 0, padding, 0);
//        }
//        private void RemovePadding()
//        {
//            map.SetPadding(0, 0, 0, 0);
//        }

//        protected override void OnLayout(bool changed, int l, int t, int r, int b)
//        {
//            base.OnLayout(changed, l, t, r, b);

//            if (changed)
//            {
//                isDrawn = false;
//            }
//        }

//        void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e)
//        {
//            /*var customPin = GetCustomPin(e.Marker);
//			if (customPin == null)
//			{
//				throw new Exception("Custom pin not found");
//			}*/
//            e.Marker.HideInfoWindow();

//        }



//        public Android.Views.View GetInfoContents(Marker marker)
//        {
//            infoWindowClicked = true;

//            var inflater = Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) as Android.Views.LayoutInflater;
//            if (inflater != null)
//            {
//                Android.Views.View view;

//                var customPin = GetCustomPin(marker);
//                if (customPin == null)
//                {
//                    throw new Exception("Custom pin not found");
//                }

//                if (customPin.Id == MapPage.TripExceptionMarker || customPin.Id == MapPage.VehicleMarker)
//                {
//                    view = inflater.Inflate(Resource.Layout.ExceptionMapInfoWindow, null);

//                }
//                else
//                {
//                    if (customPin.Id == "finish" || customPin.Id == "start")
//                    {
//                        view = inflater.Inflate(Resource.Layout.TripStartAndEndInfoWindow, null);

//                        var tripTitle = view.FindViewById<TextView>(Resource.Id.TripTitleLabel);
//                        var address = view.FindViewById<TextView>(Resource.Id.AddressLabel);
//                        var time = view.FindViewById<TextView>(Resource.Id.TimeLabel);

//                        if (customPin.Id == "start")
//                            tripTitle.Text = StyexFleetManagement.Resx.AppResources.trip_start_label;
//                        else
//                            tripTitle.Text = StyexFleetManagement.Resx.AppResources.trip_end_label;
//                        address.Text = customPin.Description;
//                        time.Text = customPin.Date;

//                        return view;
//                    }
//                    else
//                    {
//                        view = inflater.Inflate(Resource.Layout.MapInfoWindow, null);
//                    }
//                }



//                var title = view.FindViewById<TextView>(Resource.Id.TitleLabel);
//                var date = view.FindViewById<TextView>(Resource.Id.DateLabel);
//                var violationTitle = view.FindViewById<TextView>(Resource.Id.ViolationTitleLabel);
//                var violation = view.FindViewById<TextView>(Resource.Id.ViolationDataLabel);
//                var limitTitle = view.FindViewById<TextView>(Resource.Id.LimitTitleLabel);
//                var limit = view.FindViewById<TextView>(Resource.Id.LimitDataLabel);
//                var icon = view.FindViewById<ImageView>(Resource.Id.PinIcon);

//                if (title != null)
//                {
//                    title.Text = customPin.Description;
//                }
//                if (date != null)
//                {
//                    date.Text = customPin.Date;
//                }
//                if (customPin.Id == MapPage.VehicleMarker)
//                {
//                    date.Text = customPin.Location;
//                    title.Text = customPin.ToString();
//                }
//                if (violation != null && customPin.Violation != null)
//                {
//                    if (violationTitle != null)
//                    {
//                        violationTitle.Text = $"{AppResources.violation_label}:";
//                    }
//                    violation.Text = customPin.Violation;
//                }
//                if (limit != null && customPin.Limit != null)
//                {
//                    if (limitTitle != null)
//                    {
//                        limitTitle.Text = $"{AppResources.limit_label}:";
//                    }
//                    limit.Text = customPin.Limit;
//                }
//                //if (customPin.Icon != null && customPin.Icon.Length > 0)
//                //{
//                //    Bitmap bmp = BitmapFactory.DecodeByteArray(customPin.Icon, 0, customPin.Icon.Length);
//                //    icon.SetImageBitmap(bmp);
//                //}


//                return view;
//            }
//            return null;
//        }

//        public Android.Views.View GetInfoWindow(Marker marker)
//        {
//            return null;
//        }

//        AndroidCustomPin GetCustomPin(Marker annotation)
//        {
//            var position = new Position(annotation.Position.Latitude, annotation.Position.Longitude);
//            foreach (var pin in customPins)
//            {
//                if (pin.Pin.Position == position)
//                {
//                    return pin;
//                }
//            }
//            return null;
//        }
//    }
//}

