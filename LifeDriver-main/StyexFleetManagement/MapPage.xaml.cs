using Acr.UserDialogs;
using FFImageLoading.Forms;
using MoreLinq;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Extensions;
using StyexFleetManagement.Helpers;
using StyexFleetManagement.Map;
using StyexFleetManagement.Models;
using StyexFleetManagement.Models.Enum;
using StyexFleetManagement.Pages;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.Statics;
using StyexFleetManagement.ViewModel;
using Syncfusion.Data.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;
using Distance = Xamarin.Forms.GoogleMaps.Distance;
using Switch = Xamarin.Forms.Switch;

namespace StyexFleetManagement
{
    public partial class MapPage : ISearchPage
    {
        #region Properties

        private LastKnownPositions _lastKnownPositions;

        private StackLayout _panel;

        private double _panelWidth = -1;

        private bool _panelShowing;

        private readonly int _maxRetryCount = 3;

        private StackLayout _vehicleListLayout;
        private StackLayout _tripLayout;
        private Picker _vehicleGroupPicker;

        public CustomCell LastSelectedCell { get; set; }

        private Grid _tripInfoPanel;

        private readonly MapViewModel _viewModel;

        private Vehicle _selectedVehicle;
        private Trip _selectedTrip;

        private readonly string[] _pinImageFileNames =
        {
            "ic_map_marker_black",
            "ic_map_marker_green",
            "ic_map_marker_purple",
            "ic_map_marker_red",
            "ic_map_marker_yellow",
            "ic_truck_marker_black",
            "ic_truck_marker_green",
            "ic_truck_marker_purple",
            "ic_truck_marker_red",
            "ic_truck_marker_yellow",

            "ic_marker_finish",
            "ic_marker_start",

            "ic_behaviour_exception_acceleration",
            "ic_behaviour_exception_cornering",
            "ic_behaviour_exception_freewheeling",
            "ic_behaviour_exception_harsh_breaking",
            "ic_behaviour_exception_idle",
            "ic_behaviour_exception_lane_change",
            "ic_behaviour_exception_reckless_driving",
            "ic_behaviour_exception_rpm",
            "ic_behaviour_exception_speeding",
            "ic_behaviour_exception_swerving",
            "ic_exception_accident"
        };


        public static readonly string[] PlottingColors =
            {"#f44336", "#9C27B0", "#673AB7", "#3F51B5", "#2196F3", "#009688", "#FF9800", "#FF5722"};

        private ActivityIndicator _syncIndicator;

        private ActivityIndicator _mapLoadingIndicator;

        private string _selectedGroup;

        private static int _tripInfoPanelTitleFontSize;
        private static int _tripInfoPanelDataFontSize;

        private SearchOverlay _searchBar;

        private Image _button;

        public Task SetUpMarkersTask { get; private set; }

        public const string VehicleMarker = "8b5ec3d4-2097-4797-994f-e650cfefdc2d";

        public const string TripExceptionMarker = "f459c906-11ac-4cc1-98a2-919ea3a52545";

        private static double _zoomBuffer;

        private ExtendedMap _map;


        private Label _titleLabel;
        private Label _locationLabel;
        private Label _lastEventLabel;
        private Label _locationDateLabel;
        private CachedImage _selectedVehicleMarker;
        private Label _tripsLabel;
        private ListView _tripsListView;
        private DatePicker _startDatepicker;
        private DatePicker _endDatepicker;
        private List<Trip> _tripList;
        private ListView _vehicleListView;

        public Task PlotTripTask { get; set; }

        private bool _isPortrait;
        private double _width;
        private double _height;
        private Label _noRecordsLabel;
        private int _routeColourIndex;
        private ToolbarItem _mapTypeButton;
        private Switch _visibilitySwitch;
        private StackLayout _switchStack;
        private CachedImage _collapseTripPanelButton;
        private StackLayout _tripInfoStack;
        private StackLayout _vehicleSelector;
        private CustomDatePicker _datePicker;

        private DateTime _startDate;
        private DateTime _endDate;
        private Grid _customDateStack;
        private readonly List<Pin> _hiddenMarkers;
        private List<Vehicle> _vehicleList;
        private float _maxLat;
        private float _maxLng;
        private float _minLat;
        private float _minLng;
        private StackLayout _legend;

        private readonly IDictionary<string, BitmapDescriptor> _bitmaps;

        public ICommand ToggleMapTypeCommand { get; private set; }
        public static CancellationTokenSource EventCancellationToken { get; private set; }
        public Task FetchLatestEventsTask { get; private set; }

        #endregion

        public MapPage()
        {
            InitializeComponent();

            _viewModel = new MapViewModel();

            _bitmaps = LoadBitmapDescriptorDictionary();

            EventCancellationToken = new CancellationTokenSource();

            _startDate = App.StartDateSelected;
            _endDate = App.EndDateSelected;

            _vehicleList = new List<Vehicle>();

            _hiddenMarkers = new List<Pin>();

            ToggleMapTypeCommand = new Command(ToggleMapType);

            if (Device.Idiom == TargetIdiom.Phone)
            {
                _tripInfoPanelTitleFontSize = 12;
                _tripInfoPanelDataFontSize = 16;
                _zoomBuffer = 1.05;
            }
            else
            {
                _tripInfoPanelTitleFontSize = 20;
                _tripInfoPanelDataFontSize = 26;
                _zoomBuffer = 1.15;
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                _zoomBuffer = 1.05;
            }

            AddVehicleMarkerToggle();
            CreateLegend();

            Title = AppResources.title_map;

            App.MainDetailPage.MasterBehavior = MasterBehavior.Popover;

            //LatestVehicleEvents = new List<Event>();


            ToolbarItems.Add(new ToolbarItem
            {
                IconImageSource = ImageSource.FromFile("ic_loop_white_36dp_rotated"),
                Order = ToolbarItemOrder.Primary,
                Command = new Command(RefreshPage)
            });
            ToolbarItems.Add(new ToolbarItem
            {
                IconImageSource = ImageSource.FromFile("ic_map_legend"),
                Order = ToolbarItemOrder.Primary,
                Command = new Command(ToggleLegend)
            });

            SetUpMapTypeSelector();

            ToolbarItems.Add(new ToolbarItem
            {
                IconImageSource = ImageSource.FromFile("ic_action_settings"),
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => App.MainDetailPage.NavigateTo(typeof(SettingsPage)))
            });

            CreateView();
            BindingContext = _viewModel;
            SetMapType();

            AddLoadingIndicator();

            SetUpPicker();

            CreatePanel();
            CreateSearchBar();

            SetupVehicles();
        }

        private IDictionary<string, BitmapDescriptor> LoadBitmapDescriptorDictionary()
        {
            return _pinImageFileNames.ToDictionary(imageFileName => imageFileName, imageFileName => BitmapDescriptorFactory.FromBundle($"{imageFileName}.png"));
        }

        public async Task OnSearchSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
            }

            _selectedVehicle = (Vehicle)e.SelectedItem;
            _selectedVehicle.IsSelected = true;
            _searchBar.SetSelection(_selectedVehicle.Description);
            App.ShowLoading(true);

            //Reset Vehicle group picker and set selected group
            _vehicleGroupPicker = new Picker
            { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.Center };
            if (App.VehicleGroups.VehicleGroups != null)
            {
                foreach (var vehicleGroup in App.VehicleGroups.VehicleGroups)
                {
                    _vehicleGroupPicker.Items.Add(vehicleGroup.Description);
                }
            }

            var oldVehicleGroup = _selectedGroup;
            for (var i = 0; i < _vehicleGroupPicker.Items.Count; i++)
            {
                if (_vehicleGroupPicker.Items[i] != _selectedVehicle.VehicleGroupName) continue;

                var selectedIndex = i;
                var selection = _selectedVehicle.VehicleGroupName;
                if (App.VehicleGroups != null)
                {
                    _selectedGroup = App.VehicleGroups.FindIdFromDescription(selection);
                }

                _vehicleGroupPicker.SelectedIndex = selectedIndex;
                break;
            }

            _vehicleGroupPicker.SelectedIndexChanged += VehicleGroupPicker_SelectedIndexChanged;

            _vehicleListLayout.Children.Remove(_vehicleSelector);
            var vehicleSelectorImage = new Image { Source = "ic_truck" };
            var vehicleSelectorGesture = new TapGestureRecognizer();
            vehicleSelectorGesture.Tapped += VehicleSelectorGesture_Tapped;
            vehicleSelectorImage.GestureRecognizers.Add(vehicleSelectorGesture);
            _vehicleSelector = new StackLayout
            {
                VerticalOptions = LayoutOptions.Fill,
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    vehicleSelectorImage,
                    _vehicleGroupPicker
                }
            };
            _vehicleListLayout.Children.Insert(0, _vehicleSelector);

            if (_selectedGroup != oldVehicleGroup)
            {
                //Get last known positions for new group and set up markers
                _lastKnownPositions = await VehicleAPI.GetLastKnownPositions(_selectedGroup);

                if (SetUpMarkersTask != null && SetUpMarkersTask.IsCompleted == false)
                {
                    await SetUpMarkersTask;
                }

                SetUpMarkersTask = SetUpMarkersAsync(showLoading: false, spanMap: false);
                await SetUpMarkersTask;
            }

            //Span to selected vehicle
            var location = _lastKnownPositions.Items.FirstOrDefault(l => l.Id == _selectedVehicle.Id);
            if (location != null)
            {
                _selectedVehicle.LastKnownLocation = location;
            }

            App.ShowLoading(false);

            var pin = _map.Pins.FirstOrDefault(x =>
                Math.Abs(x.Position.Latitude - _selectedVehicle.LastKnownLocation.Position[1]) < float.Epsilon
                && Math.Abs(x.Position.Longitude - _selectedVehicle.LastKnownLocation.Position[0]) < float.Epsilon);
            if (pin != null)
            {
                _map.SelectedPin = pin;
            }

            _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Position(_selectedVehicle.LastKnownLocation.Position[1],
                    _selectedVehicle.LastKnownLocation.Position[0]),
                Distance.FromMiles(1)));
        }


        private async Task SetupVehicles()
        {
            await _viewModel.PopulateAllVehicles();
            await GetLastKnownPositionsAsync(false);
        }

        protected void CreateSearchBar()
        {
            if (_searchBar != null) return;

            _searchBar = new SearchOverlay(_viewModel, this)
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.EndAndExpand
            };

            // add to Layout
            Layout.Children.Add(_searchBar,
                Constraint.RelativeToParent((p) => (Layout.Width - _searchBar.Width) / 2),
                Constraint.RelativeToParent((p) => Layout.Height * 0.05),
                Constraint.Constant(300), //Width
                Constraint.Constant(55) //Height
            );
        }

        private void ToggleLegend()
        {
            if (!_legend.IsVisible)
                CreateLegend();
            _legend.IsVisible = !(_legend.IsVisible);
            _searchBar.IsVisible = !(_legend.IsVisible);
        }

        private void RefreshPage()
        {
            GetLastKnownPositionsAsync();
        }

        private void CreateView()
        {
            _map = new ExtendedMap
            {
                WidthRequest = App.PageWidth,
                HeightRequest = App.PageHeight - 100,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand
            };

            MapLayout.Children.Add(_map);
        }


        private void AddLoadingIndicator()
        {
            _mapLoadingIndicator = new ActivityIndicator
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false,
                IsEnabled = true,
            };

            Layout.Children.Add(_mapLoadingIndicator,
                Constraint.RelativeToParent((p) => { return 10; }),
                Constraint.RelativeToParent((p) => { return 10; }),
                Constraint.RelativeToParent((p) => { return 50; }),
                Constraint.RelativeToParent((p) => { return 50; })
            );
        }

        private void AddVehicleMarkerToggle()
        {
            _visibilitySwitch = new Switch
            { IsToggled = true, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
            //visibilitySwtich.Effects.Add(Effect.Resolve("FleetBI.BackgroundEffect"));
            var semiTransparentColor = new Color(248, 248, 248, 0.8);
            _visibilitySwitch.Toggled += VisibilitySwitchToggled;

            _switchStack = new StackLayout
            {
                Padding = new Thickness(2, 4, 2, 4),
                IsVisible = false,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.Center,
                BackgroundColor = semiTransparentColor,
                Children = { _visibilitySwitch }
            };
            Layout.Children.Add(_switchStack,
                Constraint.RelativeToParent((p) => { return p.Width - 64; }),
                Constraint.RelativeToParent((p) => { return p.Height - 60; }),
                Constraint.RelativeToParent((p) => { return 50; }),
                Constraint.RelativeToParent((p) => { return 50; })
            );
        }

        private void VisibilitySwitchToggled(object sender, ToggledEventArgs e)
        {
            if (e.Value == false)
            {
                foreach (var pin in _map.Pins.ToList().Where(pin => ((PinMetadata)pin.Tag).Id == VehicleMarker))
                {
                    pin.IsVisible = false;

                    //remove marker if set to invisible

                    _hiddenMarkers.Add(pin);
                }
            }
            else
            {
                foreach (var pin in _hiddenMarkers.ToList())
                {
                    pin.IsVisible = true;

                    _hiddenMarkers.Remove(pin);
                }
            }
        }

        private void SetMapType()
        {
            switch ((MapPlotType)Settings.Current.MapType)
            {
                case (MapPlotType.Satellite):
                    _map.MapType = MapType.Satellite;
                    break;
                case (MapPlotType.Terrain):
                    _map.MapType = MapType.Street;
                    break;
                case (MapPlotType.Hybrid):
                    _map.MapType = MapType.Hybrid;
                    break;
            }
        }

        private void SetUpMapTypeSelector()
        {
            var iconName = "";

            switch ((MapPlotType)Settings.Current.MapType)
            {
                case (MapPlotType.Satellite):
                    iconName = "ic_satellite_white";
                    break;
                case (MapPlotType.Terrain):
                    iconName = "ic_streetview_white";
                    break;
                case (MapPlotType.Hybrid):
                    iconName = "ic_terrain_white";
                    break;
            }

            _mapTypeButton = new ToolbarItem
            {
                Icon = iconName,
                Order = ToolbarItemOrder.Primary,
                Command = ToggleMapTypeCommand
            };

            ToolbarItems.Add(_mapTypeButton);
        }

        private void ToggleMapType()
        {
            switch ((MapPlotType)Settings.Current.MapType)
            {
                case (MapPlotType.Satellite):
                    Settings.Current.MapType = (int)MapPlotType.Terrain;
                    _map.MapType = MapType.Street;
                    _mapTypeButton.IconImageSource = ImageSource.FromFile("ic_streetview_white");
                    break;
                case (MapPlotType.Terrain):
                    Settings.Current.MapType = (int)MapPlotType.Hybrid;
                    _map.MapType = MapType.Hybrid;
                    _mapTypeButton.IconImageSource = ImageSource.FromFile("ic_terrain_white");
                    break;
                case (MapPlotType.Hybrid):
                    Settings.Current.MapType = (int)MapPlotType.Satellite;
                    _map.MapType = MapType.Satellite;
                    _mapTypeButton.IconImageSource = ImageSource.FromFile("ic_satellite_white");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        #region Events and handlers

        private void OnButtonClicked(object sender, EventArgs e)
        {
            AnimatePanel();
        }

        private void VehicleGroupPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_vehicleGroupPicker.SelectedIndex == -1)
            {
                App.SelectedVehicleGroup = null;
                App.SelectedVehicleGroupIndex = -1;
            }
            else
            {
                App.SelectedVehicleGroupIndex = (sender as Picker).SelectedIndex;
                var selection = _vehicleGroupPicker.Items[(sender as Picker).SelectedIndex];

                if (App.VehicleGroups == null) return;

                //App.SelectedVehicleGroup = App.VehicleGroups.FindIdFromDescription(selection);

                _selectedGroup = App.VehicleGroups.FindIdFromDescription(selection);

                GetLastKnownPositionsAsync(true);
            }
        }


        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (width.NearlyEqual(_width) && height.NearlyEqual(_height)) return;

            _width = width;
            _height = height;

            _isPortrait = height > width;

            RemoveTripInfoPanel(true, true);

            if (_visibilitySwitch.IsVisible)
                App.MapPadding = _switchStack.Width + 40;
            else
                App.MapPadding = 40;

            //Re-configure visibiltiy switcher
            //Layout.Children.Remove(switchStack);
            //AddVehicleMarkerToggle();

            _map.WidthRequest = _width;
            _map.HeightRequest = _height;

            //map.WidthRequest = width;
            //map.HeightRequest = height;
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private static void OnTripSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
            }

            var selectedItem = (Trip)e.SelectedItem;

            if (selectedItem.IsSelected == true)
                selectedItem.IsSelected = false;

            ((ListView)sender).SelectedItem = null; //uncomment line if you want to disable the visual selection state.
        }


        private void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
            }

            _selectedVehicle = (Vehicle)e.SelectedItem;


            if (_selectedVehicle != null)
            {
                _map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Position(_selectedVehicle.LastKnownLocation.Position[1],
                        _selectedVehicle.LastKnownLocation.Position[0]), Distance.FromMiles(1)));
                _titleLabel.Text = $"{AppResources.vehicle}: {_selectedVehicle.Description}";
                _locationLabel.Text = $"{AppResources.last_location}: {_selectedVehicle.LastKnownLocation.Location}";

                GetTripData(_selectedVehicle.Id);
            }

            //Grid.SetRowSpan(VehicleListLayout, 1);
            _panel.Children.Remove(_vehicleListLayout);
            _vehicleListLayout.Children.Remove(_vehicleSelector);
            _tripLayout.Children.Insert(1, _vehicleSelector);
            _panel.Children.Add(_tripLayout);

            ((ListView)sender).SelectedItem = null;
        }

        #endregion

        private void SetUpPicker()
        {
            _vehicleGroupPicker = new Picker
            { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.Center };
            _datePicker = new CustomDatePicker { VerticalOptions = LayoutOptions.Center };
            //Populate Vehicle Picker
            if (App.VehicleGroups.VehicleGroups != null)
            {
                foreach (var vehicleGroup in App.VehicleGroups.VehicleGroups)
                {
                    _vehicleGroupPicker.Items.Add(vehicleGroup.Description);
                }
            }

            //var e = VehicleGroupPicker.Items.IndexOf(App.SelectedVehicleGroup);
            var defaultIndex = Settings.Current.DefaultVehicleGroup;
            _vehicleGroupPicker.SelectedIndex = defaultIndex;
            _selectedGroup = App.VehicleGroups.FindIdFromDescription(_vehicleGroupPicker.Items[defaultIndex]);

            _vehicleGroupPicker.SelectedIndexChanged += VehicleGroupPicker_SelectedIndexChanged;

            _datePicker.SelectedIndexChanged += (sender, args) =>
            {
                if (_datePicker.SelectedIndex == -1)
                {
                    App.SelectedDate = ReportDateRange.TODAY;
                }
                else
                {
                    switch (_datePicker.SelectedIndex)
                    {
                        case 0:
                            App.SelectedDate = ReportDateRange.TODAY;
                            break;
                        case 1:
                            App.SelectedDate = ReportDateRange.LAST_SEVEN_DAYS;
                            break;
                        case 2:
                            App.SelectedDate = ReportDateRange.THIS_MONTH;
                            break;
                        case 3:
                            App.SelectedDate = ReportDateRange.PREVIOUS_MONTH;
                            break;
                        case 4:
                            App.SelectedDate = ReportDateRange.CUSTOM;
                            break;
                    }


                    if (App.SelectedDate == ReportDateRange.CUSTOM)
                    {
                        //Show custom picker
                        DatePickerSwitchGesture_Tapped(null, null);
                    }
                    else
                    {
                        var oldStartDate = _startDate;
                        var oldEndDate = _endDate;

                        _startDate = DateHelper.GetDateRangeStartDate(App.SelectedDate);
                        _endDate = DateHelper.GetDateRangeEndDate(App.SelectedDate);

                        var startDateEventArgs = new DateChangedEventArgs(oldStartDate, _startDate);
                        var endDateEventArgs = new DateChangedEventArgs(oldEndDate, _endDate);

                        StartDatepicker_DateSelected(this, startDateEventArgs);
                        EndDatepicker_DateSelected(this, endDateEventArgs);

                        GetTripData(_selectedVehicle.Id);
                    }
                }
            };
        }

        #region Create Panel

        /// <summary>
        /// Creates the right side menu panel
        /// </summary>
        private void CreatePanel()
        {
            if (_panel != null) return;

            if (Device.Idiom == TargetIdiom.Phone)
            {
                _panelWidth = Math.Min(App.ScreenWidth, App.ScreenHeight) * 0.9;
            }
            else
            {
                _panelWidth = Math.Min(App.ScreenWidth, App.ScreenHeight) / 2;
            }

            //Set up list view
            _vehicleListView = new ListView(ListViewCachingStrategy.RecycleElement)
            {
                BackgroundColor = Color.Transparent,
                ItemTemplate = new DataTemplate(() =>
                {
                    var cell = new VehicleListCell();
                    cell.SetBinding(VehicleListCell.VehicleNameProperty, "Description");
                    cell.SetBinding(VehicleListCell.ImageFilenameProperty, "MarkerImageFileName");

                    return cell;
                })
            };
            _vehicleListView.SetBinding(ListView.ItemsSourceProperty, "VehicleList");

            _vehicleListView.ItemSelected += OnSelection;


            var vehicleSelectorImage = new Image { Source = "ic_truck" };
            var vehicleSelectorGesture = new TapGestureRecognizer();
            vehicleSelectorGesture.Tapped += VehicleSelectorGesture_Tapped;
            vehicleSelectorImage.GestureRecognizers.Add(vehicleSelectorGesture);
            _vehicleSelector = new StackLayout
            {
                VerticalOptions = LayoutOptions.Fill,
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    vehicleSelectorImage,
                    _vehicleGroupPicker
                }
            };

            _vehicleListLayout = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    _vehicleSelector,
                    _vehicleListView
                }
            };

            _titleLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Start,
                TextColor = Palette._006
            };


            _locationLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = 10
            };
            _locationDateLabel = new Label
            {
                FontSize = 6
            };

            _lastEventLabel = new Label
            {
                FontSize = 6,
                TextColor = Color.Gray
            };

            _selectedVehicleMarker = new CachedImage();

            var locationLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    _selectedVehicleMarker,
                    new StackLayout
                    {
                        Children =
                        {
                            _locationLabel,
                            _locationDateLabel
                        }
                    }
                }
            };


            _tripsLabel = new Label
            {
                Text = AppResources.last_trips
            };


            SetUpTripListView();

            _syncIndicator = new ActivityIndicator
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = false,
                IsEnabled = true,
            };

            var tripsLabelStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal
            };

            tripsLabelStack.Children.Add(_syncIndicator);
            tripsLabelStack.Children.Add(_tripsLabel);

            _noRecordsLabel = new Label
            {
                Text = AppResources.label_no_trips,
                TextColor = Color.Gray,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                IsVisible = false
            };


            _startDatepicker = new DatePicker
            {
                Format = "dd MMM yy",
                VerticalOptions = LayoutOptions.CenterAndExpand,
                Date = DateTime.Now.AddDays(-7)
            };

            _startDatepicker.DateSelected += StartDatepicker_DateSelected;

            _endDatepicker = new DatePicker
            {
                Format = "dd MMM yy",
                VerticalOptions = LayoutOptions.CenterAndExpand
            };

            _endDatepicker.DateSelected += EndDatepicker_DateSelected;

            //set max dates for pickers
            _startDatepicker.MaximumDate = _endDatepicker.Date;
            _endDatepicker.MaximumDate = DateTime.Now;
            _endDatepicker.MinimumDate = _startDatepicker.Date;

            var dateImage = new CachedImage { Source = "ic_calendar", Aspect = Aspect.Fill };
            var datePickerSwitchGesture = new TapGestureRecognizer();
            datePickerSwitchGesture.Tapped += DatePickerSwitchGesture_Tapped;
            dateImage.GestureRecognizers.Add(datePickerSwitchGesture);

            _customDateStack = new Grid
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                IsVisible = false
            };
            _customDateStack.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            _customDateStack.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            _customDateStack.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            _customDateStack.Children.Add(_startDatepicker, 0, 0);
            _customDateStack.Children.Add(
                new Label
                {
                    Text = "-",
                    FontAttributes = FontAttributes.Bold,
                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                    HorizontalOptions = LayoutOptions.CenterAndExpand
                }, 1, 0);
            _customDateStack.Children.Add(_endDatepicker, 2, 0);

            var closeGestureRecognizer = new TapGestureRecognizer();
            closeGestureRecognizer.Tapped += BackButton_Clicked;

            var backButton = new Image
            {
                Source = "ic_back_button",
                HeightRequest = 24,
                HorizontalOptions = LayoutOptions.StartAndExpand
            };
            backButton.GestureRecognizers.Add(closeGestureRecognizer);


            _collapseTripPanelButton = new CachedImage
            { Source = "ic_arrow_drop_down_circle", HorizontalOptions = LayoutOptions.Start };
            var collapseGesture = new TapGestureRecognizer();
            collapseGesture.Tapped += CollapseGesture_Tapped;
            _collapseTripPanelButton.GestureRecognizers.Add(collapseGesture);

            SetUpPicker();

            var collapseButtonLayout = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Children =
                {
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.Start,
                        Children = {backButton}
                    },
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.EndAndExpand,
                        Children = {_collapseTripPanelButton}
                    },
                }
            };

            _tripInfoStack = new StackLayout
            {
                Spacing = 0,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children = {dateImage, _datePicker, _customDateStack}
                    },
                    _titleLabel,
                    _lastEventLabel,
                    locationLayout
                }
            };

            _tripLayout = new StackLayout
            {
                Spacing = 0,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Children =
                {
                    collapseButtonLayout,
                    _tripInfoStack,
                    tripsLabelStack,
                    _noRecordsLabel,
                    _tripsListView
                },
            };

            _panel = new StackLayout
            {
                WidthRequest = _panelWidth,
                Padding = 15,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Color.FromRgba(255, 255, 255, 0.7)
            };


            //panel.Children.Add(vehicleSelector);
            _panel.Children.Add(_vehicleListLayout);
            //Grid.SetRowSpan(VehicleListLayout, 2);


            // add to Layout
            Layout.Children.Add(_panel,
                Constraint.RelativeToParent((p) => Layout.Width - (_panelShowing ? _panelWidth : 0)),
                Constraint.RelativeToParent((p) => { return 0; }),
                Constraint.RelativeToParent((p) =>
                {
                    if (_panelWidth != -1) return _panelWidth;

                    if (Device.Idiom == TargetIdiom.Phone)
                    {
                        _panelWidth = p.Width * 0.9;
                    }
                    else
                    {
                        _panelWidth = p.Width / 2;
                    }

                    return _panelWidth;
                }),
                Constraint.RelativeToParent((p) => { return p.Height; })
            );

            _button = new Image
            {
                Source = ImageSource.FromFile("ic_slideout")
            };

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) => { AnimatePanel(); };

            _button.GestureRecognizers.Add(tapGestureRecognizer);

            Layout.Children.Add(_button,
                Constraint.RelativeToParent((p) =>
                {
                    return (Layout.Width - (_panelShowing ? _panelWidth : 0)) - 130;
                }),
                Constraint.RelativeToParent((p) => { return 50; }),
                Constraint.Constant(130),
                Constraint.Constant(130)
            );
        }

        #region Create Legend

        /// <summary>
        /// Creates the left side legend
        /// </summary>
        private void CreateLegend()
        {
            if (_legend != null)
            {
                Layout.Children.Remove(_legend);
            }

            _legend = new StackLayout
            {
                Padding = 15,
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.EndAndExpand,
                BackgroundColor = Color.FromRgba(255, 255, 255, 0.7)
            };


            var markerImageType = Settings.Current.MapMarker;
            var markerImagePrefix = "";
            switch (markerImageType)
            {
                case (MapMarkerImage.DEFAULT_PIN):
                    markerImagePrefix = "ic_map_marker";
                    break;
                case (MapMarkerImage.VEHICLE):
                    markerImagePrefix = "ic_truck_marker";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var blackLegend = new CachedImage { Source = ImageSource.FromFile(markerImagePrefix + "_black") };
            var greenLegend = new CachedImage { Source = ImageSource.FromFile(markerImagePrefix + "_green") };
            var yellowLegend = new CachedImage { Source = ImageSource.FromFile(markerImagePrefix + "_yellow") };
            var purpleLegend = new CachedImage { Source = ImageSource.FromFile(markerImagePrefix + "_purple") };
            var redLegend = new CachedImage { Source = ImageSource.FromFile(markerImagePrefix + "_red") };

            var blackLegendText = new Label
            {
                Text = AppResources.marker_legend_black,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
            };
            var greenLegendText = new Label
            {
                Text = AppResources.marker_legend_green,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
            };
            var yellowLegendText = new Label
            {
                Text = AppResources.marker_legend_yellow,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
            };
            var purpleLegendText = new Label
            {
                Text = AppResources.marker_legend_purple,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
            };
            var redLegendText = new Label
            {
                Text = AppResources.marker_legend_red,
                LineBreakMode = LineBreakMode.WordWrap,
                HorizontalTextAlignment = TextAlignment.Center,
                FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
            };


            _legend.Children.Add(new StackLayout { Children = { blackLegend, blackLegendText } });
            _legend.Children.Add(new StackLayout { Children = { greenLegend, greenLegendText } });
            _legend.Children.Add(new StackLayout { Children = { yellowLegend, yellowLegendText } });
            _legend.Children.Add(new StackLayout { Children = { purpleLegend, purpleLegendText } });
            _legend.Children.Add(new StackLayout { Children = { redLegend, redLegendText } });


            _legend.IsVisible = false;


            // add to Layout
            if (Device.Idiom == TargetIdiom.Phone)
            {
                Layout.Children.Add(_legend,
                    Constraint.Constant(5),
                    Constraint.Constant(5)
                );
                _legend.WidthRequest = 60;
            }
            else
            {
                Layout.Children.Add(_legend,
                    Constraint.Constant(10),
                    Constraint.Constant(10)
                );
            }
        }

        #endregion

        private void SetUpTripListView()
        {
            var customCell = new DataTemplate(typeof(CustomCell));
            //customCell.SetBinding(CustomCell.EndAddressProperty, "EndLocation");
            //customCell.SetBinding(CustomCell.StartAddressProperty, "StartLocation");
            //customCell.SetBinding(CustomCell.RouteCoordinatesProperty, "EndLocation");
            customCell.SetBinding(CustomCell.TimeProperty, "StartLocalTimestamp.DateTime");
            customCell.SetBinding(CustomCell.EndTimeProperty, "EndLocalTimestamp.DateTime");
            customCell.SetBinding(CustomCell.DistanceProperty, "Distance");
            customCell.SetBinding(CustomCell.IsSelectedProperty, "Value.IsSelected");
            customCell.SetBinding(CustomCell.NumberOfExceptionsProperty, "NumberOfExceptions");
            customCell.SetBinding(CustomCell.TripIdProperty, "Id");
            //customCell.SetBinding(CustomCell.DistanceProperty, "Distance");


            _tripsListView = new ListView { VerticalOptions = LayoutOptions.FillAndExpand };
            _tripsListView.SeparatorVisibility = SeparatorVisibility.None;
            _tripsListView.SeparatorColor = Color.Transparent;
            _tripsListView.GroupHeaderTemplate = new DataTemplate(typeof(HeaderCell));
            _tripsListView.ItemSelected += OnTripSelection;
            _tripsListView.IsGroupingEnabled = true;
            _tripsListView.HasUnevenRows = true;
            _tripsListView.ItemTemplate = customCell;
            _tripsListView.BackgroundColor = Color.Transparent;
        }

        private void DatePickerSwitchGesture_Tapped(object sender, EventArgs e)
        {
            if (_customDateStack.IsVisible == false)
            {
                _datePicker.IsVisible = false;
                _customDateStack.IsVisible = true;
            }
            else
            {
                _datePicker.IsVisible = true;
                _customDateStack.IsVisible = false;
            }
        }

        private void CollapseGesture_Tapped(object sender, EventArgs e)
        {
            var button = sender as CachedImage;

            if (_tripInfoStack.IsVisible == true)
            {
                //Collapse
                button.RotateTo(90, 50, Easing.Linear);
                _vehicleSelector.IsVisible = false;
                _tripInfoStack.IsVisible = false;
            }
            else
            {
                //Expand
                button.RotateTo(0, 50, Easing.Linear);
                _vehicleSelector.IsVisible = true;
                _tripInfoStack.IsVisible = true;
            }
        }

        private async Task AddTripInfoPanel()
        {
            _switchStack.IsVisible = true;
            _searchBar.IsVisible = false;
            //map.IsShowingVisibilityControl = true;

            var maxSpeed = Math.Round((decimal)_selectedTrip.MaxSpeed, 2).ToString();

            _tripInfoPanel = new Grid { BackgroundColor = Color.FromHex("#455A64"), Opacity = 0.7 };

            _tripInfoPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            _tripInfoPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = 1 });
            _tripInfoPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            _tripInfoPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = 1 });
            _tripInfoPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            var distance = Math.Round((decimal)_selectedTrip.Distance, 2);

            var durationLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                Text = FormatHelper.ToShortForm(_selectedTrip.EndLocalTimestamp - _selectedTrip.StartLocalTimestamp),
                TextColor = Color.White,
                FontSize = _tripInfoPanelDataFontSize
            };
            var maxSpeedLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                Text = maxSpeed,
                TextColor = Color.White,
                FontSize = _tripInfoPanelDataFontSize
            };
            var distanceLabel = new Label
            {
                HorizontalTextAlignment = TextAlignment.Center,
                Text = distance.ToString(),
                TextColor = Color.White,
                FontSize = _tripInfoPanelDataFontSize
            };

            var durationStack = new StackLayout
            {
                Spacing = 0,
                Padding = 0,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Children =
                {
                    durationLabel,
                    new Label
                    {
                        VerticalOptions = LayoutOptions.Start, TextColor = Color.White,
                        FontSize = _tripInfoPanelTitleFontSize, Text = AppResources.duration_short_title.ToUpper()
                    }
                }
            };
            var maxSpeedStack = new StackLayout
            {
                Spacing = 0,
                Padding = 0,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Children =
                {
                    maxSpeedLabel,
                    new Label
                    {
                        TextColor = Color.White, FontSize = _tripInfoPanelTitleFontSize,
                        Text = AppResources.max_speed_label.ToUpper()
                    }
                }
            };
            var distanceStack = new StackLayout
            {
                Spacing = 0,
                Padding = 0,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Children =
                {
                    distanceLabel,
                    new Label
                    {
                        TextColor = Color.White, FontSize = _tripInfoPanelTitleFontSize,
                        Text = AppResources.distance_short_title.ToUpper()
                    }
                }
            };

            if (Device.Idiom == TargetIdiom.Phone && _isPortrait)
            {
                _tripInfoPanel.Children.Add(durationStack, 0, 0);
                _tripInfoPanel.Children.Add(new BoxView { BackgroundColor = Color.White, WidthRequest = 1 }, 1, 0);
                _tripInfoPanel.Children.Add(maxSpeedStack, 2, 0);
                _tripInfoPanel.Children.Add(new BoxView { BackgroundColor = Color.White, WidthRequest = 1 }, 3, 0);
                _tripInfoPanel.Children.Add(distanceStack, 4, 0);
            }

            else
            {
                string brakes;
                string accelerationEvents;
                string fuelUsed;
                var consumptionEvent =
                    (await EventAPI.GetEventsById(_selectedTrip.VehicleId, 172,
                        _selectedTrip.EndLocalTimestamp.AddMinutes(-10).UtcDateTime,
                        _selectedTrip.EndLocalTimestamp.AddMinutes(10).UtcDateTime, true))
                    .FirstOrDefault(x => x.FuelData != null && x.FuelData.Reason == 2);
                if (consumptionEvent != default(Event))
                {
                    fuelUsed =
                        $"{FuelHelper.CalculateFuelUsed(consumptionEvent.FuelData.AverageFuelConsumption.Value).ToString()} l";
                }
                else
                {
                    fuelUsed = "-";
                }

                if (Settings.Current.PlotFleetExceptions)
                {
                    brakes = _selectedTrip.FleetEvents
                        .Count(x => x.EventTypeId == (int)FleetException.HARSHBREAKING).ToString();
                    accelerationEvents = _selectedTrip.FleetEvents
                        .Count(x => x.EventTypeId == (int)FleetException.EXCESSIVEACCELERATION).ToString();
                }
                else
                {
                    brakes = _selectedTrip.UBIEvents
                        .Count(x => x.EventTypeId == (int)DriverBehaviourException.EXCESSIVEBREAKING)
                        .ToString();
                    accelerationEvents = _selectedTrip.UBIEvents
                        .Count(x => x.EventTypeId == (int)DriverBehaviourException.ACCELERATION).ToString();
                }

                var brakesLabel = new Label
                {
                    HorizontalTextAlignment = TextAlignment.Center,
                    Text = brakes,
                    TextColor = Color.White,
                    FontSize = _tripInfoPanelDataFontSize
                };
                var accelerationLabel = new Label
                {
                    HorizontalTextAlignment = TextAlignment.Center,
                    Text = accelerationEvents,
                    TextColor = Color.White,
                    FontSize = _tripInfoPanelDataFontSize
                };
                var consumptionLabel = new Label
                {
                    HorizontalTextAlignment = TextAlignment.Center,
                    Text = fuelUsed,
                    TextColor = Color.White,
                    FontSize = _tripInfoPanelDataFontSize
                };

                var brakesStack = new StackLayout
                {
                    Spacing = 0,
                    Padding = 0,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Children =
                    {
                        brakesLabel,
                        new Label
                        {
                            TextColor = Color.White, FontSize = _tripInfoPanelTitleFontSize,
                            Text = AppResources.brakes_title.ToUpper()
                        }
                    }
                };


                var accelStack = new StackLayout
                {
                    Spacing = 0,
                    Padding = 0,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Children =
                    {
                        accelerationLabel,
                        new Label
                        {
                            TextColor = Color.White, FontSize = _tripInfoPanelTitleFontSize,
                            Text = AppResources.acceleration_short_title.ToUpper()
                        }
                    }
                };

                var consumptionStack = new StackLayout
                {
                    Spacing = 0,
                    Padding = 0,
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Children =
                    {
                        consumptionLabel,
                        new Label
                        {
                            TextColor = Color.White, FontSize = _tripInfoPanelTitleFontSize,
                            Text = AppResources.fuel_used_short.ToUpper()
                        }
                    }
                };

                _tripInfoPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = 1 });
                _tripInfoPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                _tripInfoPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = 1 });
                _tripInfoPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                _tripInfoPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = 1 });
                _tripInfoPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                _tripInfoPanel.Children.Add(brakesStack, 0, 0);
                _tripInfoPanel.Children.Add(new BoxView { BackgroundColor = Color.White, WidthRequest = 1 }, 1, 0);
                _tripInfoPanel.Children.Add(durationStack, 2, 0);
                _tripInfoPanel.Children.Add(new BoxView { BackgroundColor = Color.White, WidthRequest = 1 }, 3, 0);
                _tripInfoPanel.Children.Add(maxSpeedStack, 4, 0);
                _tripInfoPanel.Children.Add(new BoxView { BackgroundColor = Color.White, WidthRequest = 1 }, 5, 0);
                _tripInfoPanel.Children.Add(distanceStack, 6, 0);
                _tripInfoPanel.Children.Add(new BoxView { BackgroundColor = Color.White, WidthRequest = 1 }, 7, 0);
                _tripInfoPanel.Children.Add(accelStack, 8, 0);
                _tripInfoPanel.Children.Add(new BoxView { BackgroundColor = Color.White, WidthRequest = 1 }, 9, 0);
                _tripInfoPanel.Children.Add(consumptionStack, 10, 0);
            }


            Layout.Children.Add(_tripInfoPanel,
                Constraint.RelativeToParent((p) => { return 0; }),
                Constraint.RelativeToParent((p) => { return 0; }),
                Constraint.RelativeToParent((p) => { return p.Width; }),
                Constraint.RelativeToParent((p) =>
                {
                    if (Device.Idiom == TargetIdiom.Phone)
                    {
                        if (_isPortrait)
                            return p.Height * 0.15;
                        else
                            return p.Height * 0.2;
                    }
                    else
                        return p.Height * 0.1;
                })
            );

            //Raise slide out panel to the top
            Layout.RaiseChild(_panel);
        }

        private void VehicleSelectorGesture_Tapped(object sender, EventArgs e)
        {
            MoveMapToRegion();
        }

        private void EndDatepicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            if (_endDatepicker.Date != e.NewDate)
            {
                _endDatepicker.Date = e.NewDate;
            }

            _startDatepicker.MaximumDate = _endDatepicker.Date;

            _startDate = _startDatepicker.Date.Date;
            _endDate = _endDatepicker.Date.Date.AddDays(1).AddSeconds(-1);

            if (sender.GetType() == typeof(DatePicker))
                GetTripData(_selectedVehicle.Id);
        }

        private async void RefreshTripList()
        {
            _noRecordsLabel.IsVisible = false;

            using (var scope = new ActivityIndicatorScope(_syncIndicator, true))
            {
                if (_tripList == null)
                    _tripList = await TripsAPI.GetTripsWithStats(_selectedVehicle.Id.ToString(), _startDate, _endDate);

                if (_tripList == null || _tripList.Count == 0)
                {
                    _noRecordsLabel.IsVisible = true;
                    return;
                }

                foreach (var polyline in _map.Polylines)
                {
                    if (_tripList.All(u => u.Id != (int)polyline.Tag)) continue;

                    var index = 0;
                    foreach (var item in _tripList)
                    {
                        if (item.Id == (int)polyline.Tag)
                        {
                            _tripList[index].IsSelected = true;
                            break;
                        }

                        index++;
                    }
                }


                //var trips = await TripsAPI.GetExtendedTripAsync(_selectedGroup, startDatepicker.Date, endDatepicker.Date);

                _tripsListView.ItemsSource = null;

                //Get number of exceptions
                //get earliest trip date
                var earliestTrip = _tripList.MinBy(x => x.StartLocalTimestamp).First();
                var latestTrip = _tripList.MaxBy(x => x.EndLocalTimestamp).First();

                var fleetEvents = await EventAPI.GetVehicleExceptionEventsBetweenDates(_selectedVehicle.Id.ToString(),
                    EventType.FLEET_EXCEPTION, earliestTrip.StartLocalTimestamp.UtcDateTime,
                    latestTrip.EndLocalTimestamp.UtcDateTime, true);

                if (fleetEvents != null && fleetEvents.Count > 0)
                {
                    foreach (var trip in _tripList)
                    {
                        trip.FleetEvents = fleetEvents.Where(x =>
                            x.LocalTimestamp >= trip.StartLocalTimestamp.LocalDateTime &&
                            x.LocalTimestamp <= trip.EndLocalTimestamp.LocalDateTime).ToList();
                    }
                }

                var ubiEvents = await EventAPI.GetVehicleExceptionEventsBetweenDates(_selectedVehicle.Id.ToString(),
                    EventType.UBI_EXCEPTION, earliestTrip.StartLocalTimestamp.UtcDateTime,
                    latestTrip.EndLocalTimestamp.UtcDateTime, true);

                if (ubiEvents != null && ubiEvents.Count > 0)
                {
                    foreach (var trip in _tripList)
                    {
                        trip.UBIEvents = ubiEvents.Where(x =>
                            x.LocalTimestamp >= trip.StartLocalTimestamp.LocalDateTime &&
                            x.LocalTimestamp <= trip.EndLocalTimestamp.LocalDateTime).ToList();
                    }
                }


                var groupedData =
                    _tripList.Where(p => p.Distance > 0.1)
                        .OrderByDescending(p => p.EndLocalTimestamp)
                        .GroupBy(p =>
                            p.EndLocalTimestamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.MonthDayPattern))
                        .Select(p => new ObservableGroupCollection<string, Trip>(p))
                        .ToList();

                _noRecordsLabel.IsVisible = groupedData.Count <= 0;

                _tripsListView.ItemsSource = groupedData;


                _tripsListView.BindingContext =
                    new ObservableCollection<ObservableGroupCollection<string, Trip>>(groupedData);

                //tripsListView.GroupDisplayBinding = new Binding("Key");
            }
        }

        private void StartDatepicker_DateSelected(object sender, DateChangedEventArgs e)
        {
            if (_startDatepicker.Date != e.NewDate)
            {
                _startDatepicker.Date = e.NewDate;
            }

            _endDatepicker.MaximumDate = DateTime.Now;
            _endDatepicker.MinimumDate = _startDatepicker.Date;

            _startDate = _startDatepicker.Date.Date;
            _endDate = _endDatepicker.Date.Date.AddDays(1).AddSeconds(-1);

            if (sender.GetType() == typeof(DatePicker))
                GetTripData(_selectedVehicle.Id);
        }

        private void BackButton_Clicked(object sender, EventArgs e)
        {
            _vehicleSelector.IsVisible = true;
            _panel.Children.Remove(_tripLayout);
            _tripLayout.Children.Remove(_vehicleSelector);
            _vehicleListLayout.Children.Insert(0, _vehicleSelector);
            _panel.Children.Add(_vehicleListLayout);
            RemoveAllPlottedTrips();
            MoveMapToRegion();
            //Grid.SetRowSpan(VehicleListLayout, 2);
        }

        #endregion

        #region Panel

        /// <summary>
        /// Animates the panel in our out depending on the state
        /// </summary>
        private async void AnimatePanel(uint animationDuration = 250)
        {
            // swap the state
            _panelShowing = !_panelShowing;
            _searchBar.IsVisible = !_panelShowing &&
                                   (_tripInfoPanel == null ||
                                    (_tripInfoPanel != null && !_tripInfoPanel.IsVisible)
                                   ); //Hide or show the search bar

            // show or hide the panel
            if (_panelShowing)
            {
                // hide all children
                foreach (var child in _panel.Children)
                {
                    child.Scale = 0;
                }

                // Layout the panel to slide out
                var rect = new Rectangle(Layout.Width - _panel.Width, _panel.Y, _panel.Width, _panel.Height);
                var rectButton = new Rectangle((Layout.Width - _panel.Width) - 130, 50, 130, 130);

                await Task.Run(() =>
                {
                    _panel.LayoutTo(rect, animationDuration, Easing.CubicIn);
                    _button.LayoutTo(rectButton, animationDuration, Easing.CubicIn);
                });


                // scale in the children for the panel
                foreach (var child in _panel.Children)
                {
                    //await child.ScaleTo(1.2, 50, Easing.CubicIn);
                    await child.ScaleTo(1, 50, Easing.CubicOut);
                }
            }
            else
            {
                // Layout the panel to slide in
                var rect = new Rectangle(Layout.Width, _panel.Y, _panel.Width, _panel.Height);

                var rectButton = new Rectangle((Layout.Width - 130), 50, 130, 130);

                await Task.Run(() =>
                {
                    _panel.LayoutTo(rect, 200, Easing.CubicOut);
                    _button.LayoutTo(rectButton, 200, Easing.CubicOut);
                });

                // hide all children
                foreach (var child in _panel.Children)
                {
                    child.Scale = 0;
                }
            }
        }

        #endregion


        private async void GetTripData(Guid id)
        {
            _tripsListView.ItemsSource = null;
            try
            {
                using (var scope = new ActivityIndicatorScope(_syncIndicator, true))
                {
                    _tripList = await TripsAPI.GetTripsWithStats(id.ToString(), _startDate, _endDate);

                    RefreshTripList();
                    /*var groupedData =
                        tripList.Items.Where(p => p.Distance > 0.1)
							 .OrderByDescending(p => p.EndLocalTimestamp)
							 .GroupBy(p => p.EndLocalTimestamp.ToString(CultureInfo.CurrentCulture.DateTimeFormat.MonthDayPattern))
							 .Select(p => new ObservableGroupCollection<string, VehicleTrip>(p))
							 .ToList();

					tripsListView.ItemsSource = groupedData;

					tripsListView.BindingContext = new ObservableCollection<ObservableGroupCollection<string, VehicleTrip>>(groupedData);

					tripsListView.GroupDisplayBinding = new Binding("Key");*/
                }
            }
            catch (NullReferenceException e)
            {
                await DisplayAlert(AppResources.error_label, AppResources.error_trip_information,
                    AppResources.button_ok);
            }

            //selectedTrip = trips.Items[5];


            //PlotTrip(trips.Items[5], positions.Items, exceptions.Items);

            //PlotTrip(trips.Items[0], trips.Items[0].
            /*if (trips.Items != null)
			{
				foreach (VehicleTrip trip in trips.Items)
				{
					tripListViewSource.Add(new VehicleTrip { Distance = trip.Distance, EndLocalTimestamp = trip.EndLocalTimestamp, EndLocation = trip.EndLocation });
				}
			}*/


            //CopyProperties(trips, vehicleTrips);


            //vehicleTrips = (VehicleTrips)trips.MemberwiseClone();

            //vehicleTrips = await VehicleAPI.GetTripsAsync(id);
        }

        public static void CopyProperties(VehicleTrips objSource, VehicleTrips objDestination)
        {
            var destProps = objDestination.GetType().GetTypeInfo().DeclaredProperties;

            //get the list of all properties in the source object
            foreach (var sourceProp in objSource.GetType().GetTypeInfo().DeclaredProperties)
            {
                foreach (var destProperty in destProps)
                {
                    //if we find match between source & destination properties name, set
                    //the value to the destination property
                    if (destProperty.Name != sourceProp.Name || !destProperty.PropertyType.GetTypeInfo()
                        .IsAssignableFrom(sourceProp.PropertyType.GetTypeInfo())) continue;

                    destProperty.SetValue(objDestination, sourceProp.GetValue(objSource));

                    break;
                }
            }
        }


        private async Task GetLastKnownPositionsAsync(bool fromGroupPicker = false, int retryCount = 0, bool spanMap = true)
        {
            try
            {
                App.ShowLoading(true, AppResources.loading);

                _lastKnownPositions = await VehicleAPI.GetLastKnownPositions(_selectedGroup);

                App.ShowLoading(false);


                if (SetUpMarkersTask != null && SetUpMarkersTask.IsCompleted == false)
                {
                    await SetUpMarkersTask;
                }

                SetUpMarkersTask = SetUpMarkersAsync(showLoading: true, spanMap: spanMap);


                if (fromGroupPicker)
                {
                    //BackButton_Clicked(null, null);
                }
            }
            catch (NullReferenceException e)
            {
                if (retryCount < _maxRetryCount)
                {
                    await Task.Delay(1000);
                    GetLastKnownPositionsAsync(retryCount: retryCount += 1);
                }
                else
                {
                    await DisplayAlert(AppResources.error_label, AppResources.error_position, AppResources.button_ok);
                }
            }
            catch (Exception e)
            {
                if (retryCount < _maxRetryCount)
                {
                    await Task.Delay(1000);
                    GetLastKnownPositionsAsync(retryCount: retryCount += 1);
                }
                else
                {
                    await DisplayAlert(AppResources.error_label, AppResources.error_position, AppResources.button_ok);
                }
            }
        }

        //private async Task<List<Event>> GetLatestEvents()
        //{
        //    try
        //    {
        //        App.ShowLoading(true, isCancel: true);

        //        var isCancelled = EventCancellationToken.IsCancellationRequested;

        //        if (isCancelled)
        //            throw new OperationCanceledException();

        //        using (var scope = new ActivityIndicatorScope(mapLoadingIndicator, true))
        //        {
        //            var vehicles = await VehicleAPI.GetVehicleInGroupAsync(_selectedGroup);
        //            var allTasks = new List<Task>();

        //            List<Event> events = new List<Event>();
        //            foreach (VehicleItem vehicle in vehicles)
        //            {
        //                await throttler.WaitAsync();
        //                allTasks.Add(
        //                            Task.Run(async () =>
        //                            {
        //                                try
        //                                {
        //                                    isCancelled = EventCancellationToken.IsCancellationRequested;

        //                                    if (isCancelled)
        //                                        throw new OperationCanceledException();

        //                                    var tempStartDate = DateTime.Now.AddDays(-3);
        //                                    var tempEndDate = DateTime.Now;

        //                                    List<Event> data = await EventAPI.GetEventsByArrayAsync(vehicle.Id, tempStartDate, tempEndDate);

        //                                    isCancelled = EventCancellationToken.IsCancellationRequested;

        //                                    if (isCancelled)
        //                                        throw new OperationCanceledException();

        //                                    if (data != null)
        //                                        events = events.Concat(data).ToList();
        //                                }
        //                                finally
        //                                {
        //                                    throttler.Release();
        //                                }
        //                            }));

        //            }

        //            //var events = await EventAPI.GetLatestEvents(_selectedGroup);

        //            isCancelled = EventCancellationToken.IsCancellationRequested;

        //            if (isCancelled)
        //                throw new OperationCanceledException();


        //            await Task.WhenAll(allTasks);
        //            LatestVehicleEvents = events;
        //            //App.ShowLoading(false);
        //            System.Diagnostics.Debug.WriteLine("SysUpdate: Received latest events");

        //            if (SetUpMarkersTask != null && SetUpMarkersTask.IsCompleted == false)
        //            {
        //                await SetUpMarkersTask;
        //            }

        //            SetUpMarkersTask = SetUpMarkersAsync(true, true);

        //            App.ShowLoading(false);
        //            return events;
        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        App.ShowLoading(false);
        //        Debug.WriteLine("Task Cancelled");
        //        return null;
        //    }

        //}

        private async Task SetUpMarkersAsync(bool showLoading = true, bool spanMap = true)
        {
            await SetUpMarkers(showLoading, spanMap);
        }

        private async Task SetUpMarkers(bool showLoading = true, bool spanMap = true)
        {
            try
            {
                IProgressDialog progressDialog = null;
                ClearMap();

                _vehicleList.Clear();
                _viewModel.VehicleList.Clear();

                if (showLoading)
                {
                    progressDialog = UserDialogs.Instance.Progress(AppResources.loading, null, null, true, MaskType.Black);
                }

                var totalCount = _lastKnownPositions.Items.Count;
                var iter = 0;
                foreach (var location in _lastKnownPositions.Items.ToList())
                {
                    await SetUpMarker(location);
                    if (progressDialog == null) continue;
                    iter++;
                    var progress = (iter * 100) / totalCount;
                    if (progress > 0)
                        progressDialog.PercentComplete = progress;
                }

                if (progressDialog != null)
                {
                    progressDialog.Hide();
                    progressDialog.Dispose();
                }

                if (_vehicleList == null || _vehicleList.Count == 0)
                    return;

                _vehicleList = new List<Vehicle>(_vehicleList.OrderBy(x => x.Description));
                foreach (var vehicle in _vehicleList)
                {
                    _viewModel.VehicleList.Add(vehicle);
                }

                if (spanMap)
                {
                    _maxLat = _vehicleList.Max(x => x.LastKnownLocation.Position[1]);
                    _maxLng = _vehicleList.Max(x => x.LastKnownLocation.Position[0]);

                    _minLat = _vehicleList.Min(x => x.LastKnownLocation.Position[1]);
                    _minLng = _vehicleList.Min(x => x.LastKnownLocation.Position[0]);

                    var lat1 = Math.Abs((_maxLat - _minLat));
                    var latDegrees = lat1 * _zoomBuffer;

                    var lng1 = Math.Abs(_maxLng - _minLng);
                    var lngDegrees = lng1 * _zoomBuffer;

                    Device.BeginInvokeOnMainThread(() => { MoveMapToRegion(); });
                }

                if (FetchLatestEventsTask != null && FetchLatestEventsTask.IsCompleted == false)
                {
                    await FetchLatestEventsTask;
                }

                FetchLatestEventsTask = FetchLatestEventsForVehiclesAndSetPinColour();

                /*
                MapSpan tempRegion = new MapSpan(centre,lat1 ,lng1);

                if (tempRegion !=null)
                    map.MoveToRegion(tempRegion);*/
            }
            catch (NullReferenceException e)
            {
                if (showLoading)
                {
                    Device.BeginInvokeOnMainThread(() => { App.ShowLoading(false); });
                }

                await DisplayAlert(AppResources.error_label, AppResources.error_position, AppResources.button_ok);
            }
            catch (Exception e)
            {
                if (showLoading)
                {
                    Device.BeginInvokeOnMainThread(() => { App.ShowLoading(false); });
                }

                await DisplayAlert(AppResources.error_label, AppResources.error_position, AppResources.button_ok);
            }
        }

        private void ClearMap()
        {
            _map.Pins.Clear();
            _map.Polylines.Clear();
        }

        private async Task<Event> GetLastEventWithSpeed(string vehicleId)
        {
            try
            {
                var now = DateTime.Now.AddDays(1);
                var start = now.AddDays(-3);

                var trips = await TripsAPI.GetTripsWithStats(vehicleId, start, now);

                if (trips == null || trips.Count == 0)
                    return null;

                var lastTrip = trips.OrderByDescending(x => x.EndLocalTimestamp).First();

                var data = await EventAPI.GetEventsByArrayAsync(vehicleId,
                    lastTrip.EndLocalTimestamp.DateTime.AddMinutes(-10), now, includeTachographData: true);

                if (data == null)
                    return null;

                var orderedEvents = data.OrderByDescending(t => t.LocalTimestamp);

                foreach (var anEvent in orderedEvents)
                {
                    if (anEvent.EventTypeId == (int)TripEventType.TripStart)
                        return orderedEvents.FirstOrDefault();

                    if (Math.Abs(anEvent.Speed) > double.Epsilon)
                        return anEvent;
                }

                return orderedEvents.FirstOrDefault();
            }
            catch (KeyNotFoundException e)
            {
                return null;
            }
        }

        private async Task<Event> GetLastEvent(string vehicleId)
        {
            try
            {
                var now = DateTime.Now.AddDays(1);

                var start = DateTime.Now.AddDays(-3);

                var trips = await TripsAPI.GetTripsWithStats(vehicleId, start, now);

                if (trips == null || trips.Count == 0)
                    return null;

                var lastTrip = trips.OrderByDescending(x => x.EndLocalTimestamp).First();

                var data = await EventAPI.GetEventsByArrayAsync(vehicleId,
                    lastTrip.EndLocalTimestamp.DateTime.AddMinutes(-10), now);

                var orderedList = data?.OrderByDescending(t => t.LocalTimestamp);
                return orderedList?.FirstOrDefault();
            }
            catch (KeyNotFoundException e)
            {
                return null;
            }
        }


        private async Task SetUpMarker(UnitLocation location)
        {
            // Create marker
            var pin = new Pin
            {
                Label = location.Description,
                Address = location.Location,
                Position = new Position(location.Position[1], location.Position[0]),
                IsVisible = _visibilitySwitch.IsToggled,
                Tag = new PinMetadata
                {
                    Title = location.Description,
                    Id = VehicleMarker,
                    VehicleId = location.Id.ToString(),
                    Description = location.Description,
                    Date = location.LocalTimestamp,
                    Location = location.Location,
                }
            };

            //Default to purple
            await SetPinImageAndState(MapMarker.MapMarkerType.PURPLE, pin);
            _vehicleList.Add(new Vehicle(location) { MarkerImageFileName = ((PinMetadata)pin.Tag).ImageFileName });

            // Don't plot (0,0) locations
            if (Math.Abs(location.Position[0]) > double.Epsilon && Math.Abs(location.Position[1]) > double.Epsilon)
            {
                _map.Pins.Add(pin);
            }
        }

        private async Task SetPinImageAndState(MapMarker.MapMarkerType markerType, Pin pin)
        {
            var imageFileName = string.Empty;
            var markerImageType = Settings.Current.MapMarker;
            switch (markerType)
            {
                case MapMarker.MapMarkerType.GREEN:
                    ((PinMetadata)pin.Tag).MarkerState = MapMarker.MapMarkerType.GREEN;
                    switch (markerImageType)
                    {
                        case (MapMarkerImage.DEFAULT_PIN):
                            imageFileName = "ic_map_marker_green";
                            break;
                        case (MapMarkerImage.VEHICLE):
                            imageFileName = "ic_truck_marker_green";
                            break;
                    }

                    ((PinMetadata)pin.Tag).LastEvent = await GetLastEventWithSpeed(((PinMetadata)pin.Tag).VehicleId);
                    break;
                case MapMarker.MapMarkerType.AMBER:
                    ((PinMetadata)pin.Tag).MarkerState = MapMarker.MapMarkerType.AMBER;
                    switch (markerImageType)
                    {
                        case (MapMarkerImage.DEFAULT_PIN):
                            imageFileName = "ic_map_marker_yellow";
                            break;
                        case (MapMarkerImage.VEHICLE):
                            imageFileName = "ic_truck_marker_yellow";
                            break;
                    }

                    break;
                case MapMarker.MapMarkerType.RED:
                    ((PinMetadata)pin.Tag).MarkerState = MapMarker.MapMarkerType.RED;
                    switch (markerImageType)
                    {
                        case (MapMarkerImage.DEFAULT_PIN):
                            imageFileName = "ic_map_marker_red";
                            break;
                        case (MapMarkerImage.VEHICLE):
                            imageFileName = "ic_truck_marker_red";
                            break;
                    }

                    ((PinMetadata)pin.Tag).LastEvent = await GetLastEvent(((PinMetadata)pin.Tag).VehicleId);
                    break;
                case MapMarker.MapMarkerType.BLACK:
                    ((PinMetadata)pin.Tag).MarkerState = MapMarker.MapMarkerType.BLACK;
                    switch (markerImageType)
                    {
                        case (MapMarkerImage.DEFAULT_PIN):
                            imageFileName = "ic_map_marker_black";
                            break;
                        case (MapMarkerImage.VEHICLE):
                            imageFileName = "ic_truck_marker_black";
                            break;
                    }

                    break;
                case MapMarker.MapMarkerType.PURPLE:
                    ((PinMetadata)pin.Tag).MarkerState = MapMarker.MapMarkerType.PURPLE;
                    switch (markerImageType)
                    {
                        case (MapMarkerImage.DEFAULT_PIN):
                            imageFileName = "ic_map_marker_purple";
                            break;
                        case (MapMarkerImage.VEHICLE):
                            imageFileName = "ic_truck_marker_purple";
                            break;
                    }

                    break;
            }

            if (!string.IsNullOrWhiteSpace(imageFileName))
            {
                pin.Icon = _bitmaps[imageFileName];
            }
        }

        private async Task FetchLatestEventsForVehiclesAndSetPinColour()
        {
            foreach (var pin in _map.Pins.ToList())
            {
                var markerType = await GetMapMarkerType(((PinMetadata)pin.Tag).VehicleId);
                await SetPinImageAndState(markerType, pin);
                var vehicle = _viewModel.VehicleList.FirstOrDefault(v => v.Description == ((PinMetadata)pin.Tag).Title);
                if (vehicle != null && vehicle.MarkerImageFileName != ((PinMetadata)pin.Tag).ImageFileName)
                {
                    vehicle.MarkerImageFileName = ((PinMetadata)pin.Tag).ImageFileName;
                }

                ((PinMetadata)pin.Tag).Title = pin.ToString();
            }

            if (_panel.Children.Contains(_vehicleListLayout))
            {
                _panel.Children.Remove(_vehicleListLayout);
                _panel.Children.Add(_vehicleListLayout);
            }
        }

        private async Task SetPinDetailsFromLatestEvent(Pin pin)
        {
            var markerType = await GetMapMarkerType(((PinMetadata)pin.Tag).VehicleId);
            await SetPinImageAndState(markerType, pin);
            var vehicle = _viewModel.VehicleList.FirstOrDefault(v => v.Description == ((PinMetadata)pin.Tag).Title);
            if (vehicle != null && vehicle.MarkerImageFileName != ((PinMetadata)pin.Tag).ImageFileName)
            {
                vehicle.MarkerImageFileName = ((PinMetadata)pin.Tag).ImageFileName;
            }

                ((PinMetadata)pin.Tag).Title = pin.ToString();
        }

        private void MoveMapToRegion()
        {
            var centre = new Position((_maxLat + _minLat) / 2, (_maxLng + _minLng) / 2);

            var distance = DistanceCalculation.GeoCodeCalc.CalcDistance(_minLat, _minLng, _maxLat, _maxLng,
                DistanceCalculation.GeoCodeCalcMeasurement.Kilometers) * _zoomBuffer;

            _map.MoveToRegion(MapSpan.FromCenterAndRadius(centre, Distance.FromKilometers(distance / 2)));
        }


        #region Trip plotting

        public async Task PlotTrip(Trip trip)
        {
            try
            {
                using (var scope = new ActivityIndicatorScope(_syncIndicator, true))
                {
                    _selectedTrip = trip;

                    var fleetEvents = new List<Event>();
                    var ubiEvents = new List<Event>();

                    var routeCoords = new List<Position>();

                    var plotFleetEvents = Settings.Current.PlotFleetExceptions;
                    var plotUbiEvents = Settings.Current.PlotUBIExceptions;

                    if (!Settings.Current.AllowMultipleTripPlotting && _map.Polylines.Count > 0)
                    {
                        RemoveAllPlottedTrips();
                        LastSelectedCell.IsSelected = false;
                    }

                    // Remove trip info panel
                    RemoveTripInfoPanel();

                    var tripPositions = await TripsAPI.GetTripPositionsAsync(trip.Id);

                    float minLng = 0, maxLng = 0, minLat = 0, maxLat = 0;
                    var tripValid = false;
                    if (Settings.Current.SnapTripToRoad)
                    {
                        var snappedPoints = await TripsAPI.SnapTripToRoad(tripPositions);

                        foreach (var position in snappedPoints)
                        {
                            if (position.Location.Latitude != 0 && position.Location.Longitude != 0)
                            {
                                tripValid = true;

                                routeCoords.Add(new Position(position.Location.Latitude, position.Location.Longitude));

                                if (position == snappedPoints[0])
                                {
                                    minLng = maxLng = position.Location.Longitude;
                                    minLat = maxLat = position.Location.Latitude;
                                }
                                else
                                {
                                    if (minLng > position.Location.Longitude)
                                        minLng = position.Location.Longitude;

                                    if (maxLng < position.Location.Longitude)
                                        maxLng = position.Location.Longitude;

                                    if (minLat > position.Location.Latitude)
                                        minLat = position.Location.Latitude;

                                    if (maxLat < position.Location.Latitude)
                                        maxLat = position.Location.Latitude;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var position in tripPositions.Items)
                        {
                            if (position[0] != 0 && position[1] != 0)
                            {
                                tripValid = true;

                                routeCoords.Add(new Position(position[1], position[0]));

                                if (position == tripPositions.Items[0])
                                {
                                    minLng = maxLng = position[0];
                                    minLat = maxLat = position[1];
                                }
                                else
                                {
                                    if (minLng > position[0])
                                        minLng = position[0];

                                    if (maxLng < position[0])
                                        maxLng = position[0];

                                    if (minLat > position[1])
                                        minLat = position[1];

                                    if (maxLat < position[1])
                                        maxLat = position[1];
                                }
                            }
                        }
                    }


                    if (tripValid)
                    {
                        // Add trip start and finish markers

                        var startPin = new Pin
                        {
                            Label = AppResources.trip_start_label + _selectedTrip.StartLocation,
                            Position = new Position(_selectedTrip.StartPosition[1] ?? routeCoords[0].Latitude,
                                _selectedTrip.StartPosition[0] ?? routeCoords[0].Longitude),
                            Icon = _bitmaps["ic_marker_start"],
                            Tag = new PinMetadata
                            {

                                Title = AppResources.trip_start_label + _selectedTrip.StartLocation,
                                TripId = trip.Id,
                                Description = _selectedTrip.StartLocation,
                                Date = _selectedTrip.StartLocalTimestamp.LocalDateTime.ToString("HH:mm tt, dd MMM yyyy",
                                    CultureInfo.InvariantCulture),
                                Id = "start",
                            }
                        };

                        _map.Pins.Add(startPin);

                        var endPin = new Pin()
                        {
                            Label = AppResources.trip_end_label + _selectedTrip.EndLocation,
                            Position = new Position(_selectedTrip.EndPosition[1] ?? routeCoords.Last().Latitude,
                                _selectedTrip.EndPosition[0] ?? routeCoords.Last().Longitude),
                            Icon = _bitmaps["ic_marker_finish"],
                            Tag = new PinMetadata
                            {
                                Title = AppResources.trip_end_label + _selectedTrip.EndLocation,
                                TripId = trip.Id,
                                Description = _selectedTrip.EndLocation,
                                Date = _selectedTrip.EndLocalTimestamp.LocalDateTime.ToString("HH:mm tt, dd MMM yyyy",
                                    CultureInfo.InvariantCulture),
                                Id = "finish",
                            }
                        };

                        _map.Pins.Add(endPin);

                        // Plot trip exceptions
                        if (plotFleetEvents)
                        {
                            foreach (var fleetEvent in trip.FleetEvents)
                            {
                                _map.Pins.Add(GetTripExceptionPinFromEvent(trip.Id, fleetEvent, true));
                            }
                        }

                        if (plotUbiEvents)
                        {
                            foreach (var ubiEvent in trip.UBIEvents)
                            {
                                _map.Pins.Add(GetTripExceptionPinFromEvent(trip.Id, ubiEvent, false));
                            }
                        }


                        string routeColour;

                        if (_routeColourIndex < PlottingColors.Length)
                        {
                            routeColour = PlottingColors[_routeColourIndex];
                            _routeColourIndex += 1;
                        }
                        else
                        {
                            _routeColourIndex = 0;
                            routeColour = PlottingColors[_routeColourIndex];
                        }

                        var polyline = new Polyline()
                        {
                            StrokeColor = Color.FromHex(routeColour),
                            StrokeWidth = 8f,
                            Tag = trip.Id
                        };
                        foreach (var position in routeCoords)
                        {
                            polyline.Positions.Add(position);
                        }
                        _map.Polylines.Add(polyline);

                        var centre = new Position((maxLat + minLat) / 2, (maxLng + minLng) / 2);

                        var distance = DistanceCalculation.GeoCodeCalc.CalcDistance(minLat, minLng, maxLat, maxLng,
                            DistanceCalculation.GeoCodeCalcMeasurement.Kilometers) * 1.2;

                        _map.MoveToRegion(MapSpan.FromCenterAndRadius(centre,
                            Distance.FromKilometers(distance / 2)));

                        AnimatePanel(0);


                        AddTripInfoPanel();

                        //AnimatePanel();

                        //LatLngBounds bounds = new LatLngBounds(new LatLng(minLat, minLng), new LatLng(maxLat, maxLng));
                        //map.animateCamera(CameraUpdateFactory.newLatLngBounds(bounds, 100));
                    }
                    else
                    {
                        Serilog.Log.Debug($"Cannot plot trip: {trip.Id}");
                        //Toast.makeText(getActivity(), R.string.toast_cannot_plot_trip_locations_missing, Toast.LENGTH_LONG).show();
                    }
                }
            }
            catch (NullReferenceException e)
            {
                Serilog.Log.Debug(e.Message);
            }
            catch (Exception e)
            {
                Serilog.Log.Debug(e.Message);
            }
        }

        private Pin GetTripExceptionPinFromEvent(int tripId, Event drivingEvent, bool isFleetEvent)
        {
            var pin = new Pin()
            {
                Label = drivingEvent.EventTypeDescription,
                Position = new Position(drivingEvent.Position[1], drivingEvent.Position[0]),
                Tag = new PinMetadata()
                {
                    Title = drivingEvent.EventTypeDescription,
                    //Subtitle = drivingEvent.LocalTimestamp.LocalDateTime.ToString("HH:mm dddd, dd MMMM yyyy",
                    //    CultureInfo.CurrentCulture),
                    TripId = tripId,
                    Id = TripExceptionMarker,
                    Date = drivingEvent.LocalTimestamp.LocalDateTime.ToString("HH:mm dddd, dd MMMM yyyy",
                        CultureInfo.CurrentCulture),
                    Description = drivingEvent.EventTypeDescription
                }
            };

            if (isFleetEvent)
            {
                switch ((FleetException)drivingEvent.EventTypeId)
                {
                    case (FleetException.EXCESSIVEACCELERATION):
                        if (drivingEvent.EndOfExceptionData != null)
                        {
                            var accLimit = drivingEvent.EndOfExceptionData.Limit.ToString();
                            var accViolation = drivingEvent.EndOfExceptionData.Maximum.ToString();
                            ((PinMetadata)pin.Tag).Limit = accLimit.ToString();
                            ((PinMetadata)pin.Tag).Violation = accViolation.ToString();
                            ((PinMetadata)pin.Tag).Title +=
                                $" ({AppResources.limit_label} {accLimit}, {AppResources.violation_label} {accViolation})";
                        }

                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_acceleration";
                        break;
                    case (FleetException.EXCESSIVEIDLE):
                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_idle";
                        break;
                    case (FleetException.EXCESSIVERPM):
                        if (drivingEvent.EndOfExceptionData != null)
                        {
                            var rpmLimit = drivingEvent.EndOfExceptionData.Limit.ToString();
                            var rpmViolation = drivingEvent.EndOfExceptionData.Maximum.ToString();
                            ((PinMetadata)pin.Tag).Limit = rpmLimit.ToString();
                            ((PinMetadata)pin.Tag).Violation = rpmViolation.ToString();
                            ((PinMetadata)pin.Tag).Title +=
                                $" ({AppResources.limit_label} {rpmLimit}, {AppResources.violation_label} {rpmViolation})";
                        }

                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_rpm";
                        break;
                    case (FleetException.FREEWHEELING):
                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_freewheeling";
                        break;
                    case (FleetException.HARSHBREAKING):
                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_harsh_breaking";
                        break;
                    case (FleetException.SPEEDING):
                        if (drivingEvent.EndOfExceptionData != null)
                        {
                            var speedingLimit = drivingEvent.EndOfExceptionData.Limit.ToString();
                            var speedingViolation = drivingEvent.EndOfExceptionData.Maximum.ToString();

                            ((PinMetadata)pin.Tag).Limit = speedingLimit.ToString();
                            ((PinMetadata)pin.Tag).Violation = speedingViolation.ToString();
                            ((PinMetadata)pin.Tag).Title +=
                                $" ({AppResources.limit_label} {speedingLimit}, {AppResources.violation_label} {speedingViolation})";
                        }

                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_speeding";
                        break;
                    case (FleetException.RECKLESSDRIVING):
                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_swerving";
                        break;
                    case (FleetException.ACCIDENT):
                        ((PinMetadata)pin.Tag).ImageFileName = "ic_exception_accident";
                        break;
                    default:
                        ((PinMetadata)pin.Tag).ImageFileName = "";
                        break;
                }
            }
            else
            {
                switch ((DriverBehaviourException)drivingEvent.EventTypeId)
                {
                    case (DriverBehaviourException.ACCELERATION):
                        if (drivingEvent.EndOfExceptionData != null)
                        {
                            var accLimit = drivingEvent.EndOfExceptionData.Limit.ToString();
                            var accViolation = drivingEvent.EndOfExceptionData.Maximum.ToString();
                            ((PinMetadata)pin.Tag).Limit = accLimit.ToString();
                            ((PinMetadata)pin.Tag).Violation = accViolation.ToString();
                            ((PinMetadata)pin.Tag).Title +=
                                $" ({AppResources.limit_label} {accLimit}, {AppResources.violation_label} {accViolation})";
                        }

                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_acceleration";
                        break;
                    case (DriverBehaviourException.EXCESSIVEIDLE):
                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_idle";
                        break;
                    case (DriverBehaviourException.LANECHANGE):
                        if (drivingEvent.EndOfExceptionData != null)
                        {
                            var rpmLimit = drivingEvent.EndOfExceptionData.Limit.ToString();
                            var rpmViolation = drivingEvent.EndOfExceptionData.Maximum.ToString();
                            ((PinMetadata)pin.Tag).Limit = rpmLimit.ToString();
                            ((PinMetadata)pin.Tag).Violation = rpmViolation.ToString();
                            ((PinMetadata)pin.Tag).Title +=
                                $" ({AppResources.limit_label} {rpmLimit}, {AppResources.violation_label} {rpmViolation})";
                        }

                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_lane_change";
                        break;
                    case (DriverBehaviourException.FREEWHEELING):
                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_freewheeling";
                        break;
                    case (DriverBehaviourException.EXCESSIVEBREAKING):
                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_harsh_breaking";
                        break;
                    case (DriverBehaviourException.SPEEDING):
                        if (drivingEvent.EndOfExceptionData != null)
                        {
                            var speedingLimit = drivingEvent.EndOfExceptionData.Limit.ToString();
                            var speedingViolation = drivingEvent.EndOfExceptionData.Maximum.ToString();

                            ((PinMetadata)pin.Tag).Limit = speedingLimit.ToString();
                            ((PinMetadata)pin.Tag).Violation = speedingViolation.ToString();

                            ((PinMetadata)pin.Tag).Title +=
                                $" ({AppResources.limit_label} {speedingLimit}, {AppResources.violation_label} {speedingViolation})";
                        }

                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_speeding";
                        break;
                    case (DriverBehaviourException.CORNERING):
                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_cornering";
                        break;
                    case (DriverBehaviourException.SWERVING):
                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_swerving";
                        break;
                    case (DriverBehaviourException.EXCESSIVERPM):
                        ((PinMetadata)pin.Tag).ImageFileName = "ic_behaviour_exception_rpm";
                        break;
                    case (DriverBehaviourException.ACCIDENT):
                        ((PinMetadata)pin.Tag).ImageFileName = "ic_exception_accident";
                        break;
                    default:
                        ((PinMetadata)pin.Tag).ImageFileName = "";
                        break;
                }
            }

            pin.Icon = _bitmaps[((PinMetadata)pin.Tag).ImageFileName];
            return pin;
        }

        private void RemoveTripInfoPanel(bool isOrientationChanging = false, bool reAddPanel = false)
        {
            if (_tripInfoPanel != null)
            {
                Layout.Children.Remove(_tripInfoPanel);
                _tripInfoPanel = null;

                _visibilitySwitch.IsToggled = true;

                if (reAddPanel)
                    AddTripInfoPanel();
            }

            if (!isOrientationChanging)
            {
                _switchStack.IsVisible = false;
                //map.IsShowingVisibilityControl = false;
            }
        }

        public void RemovePlottedTrip(Trip trip)
        {
            RemoveTripInfoPanel();

            var polyline = _map.Polylines.FirstOrDefault(polyLine => trip.Id == (int)polyLine.Tag);
            if (polyline != default)
            {
                _map.Polylines.Remove(polyline);
            }

            if (_map.Pins == null) return;

            foreach (var pin in _map.Pins.ToList())
            {
                if (((PinMetadata)pin.Tag).TripId == trip.Id)
                {
                    _map.Pins.Remove(pin);
                }
            }
        }

        public void RemoveAllPlottedTrips()
        {
            RemoveTripInfoPanel();

            foreach (var polyline in _map.Polylines.ToList())
            {
                _map.Polylines.Remove(polyline);
            }

            if (_map.Pins == null) return;

            foreach (var pin in _map.Pins.ToList().Where(pin => ((PinMetadata)pin.Tag).TripId != 0))
            {
                _map.Pins.Remove(pin);
            }
        }

        #endregion

        public async Task<MapMarker.MapMarkerType> GetMapMarkerType(string vehicleId)
        {
            try
            {
                var lastEvent = await GetLastEvent(vehicleId);

                if (lastEvent == null)
                    return MapMarker.MapMarkerType.PURPLE;

                if (DateTime.Now.AddDays(-3) > lastEvent.LocalTimestamp)
                    return MapMarker.MapMarkerType.PURPLE;

                switch ((EventType)lastEvent.EventTypeId)
                {
                    case (EventType.DEVICE_UNPLUGGED):
                        return MapMarker.MapMarkerType.RED;
                    case (EventType.IDLE):
                        return MapMarker.MapMarkerType.AMBER;
                    case (EventType.MAINS_LOW):
                        return MapMarker.MapMarkerType.RED;
                    case (EventType.TRIP_SHUTDOWN):
                        return MapMarker.MapMarkerType.BLACK;
                    case (EventType.TRIP_STARTUP):
                    case (EventType.Tachograph):
                    case (EventType.UBI_EXCEPTION):
                        return MapMarker.MapMarkerType.GREEN;
                    default:
                        return MapMarker.MapMarkerType.PURPLE;
                }
            }
            catch (NullReferenceException e)
            {
                Serilog.Log.Error(e, e.Message);
                return MapMarker.MapMarkerType.PURPLE;
            }
        }


        public class DataSourceConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return ((IList)value).Count == 0;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                return value;
            }
        }
    }
}