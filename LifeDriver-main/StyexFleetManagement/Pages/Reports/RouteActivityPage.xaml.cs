using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using StyexFleetManagement.Converters;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Helpers;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel;
using Syncfusion.Data;
using Syncfusion.SfChart.XForms;
using Syncfusion.SfDataGrid.XForms;
using Xamarin.Forms;
using SelectionMode = Syncfusion.SfDataGrid.XForms.SelectionMode;

namespace StyexFleetManagement.Pages.Reports
{
    public partial class RouteActivityPage : ReportPage
    {
        private SfDataGrid dataGrid;
        private SfDataGrid tripGrid;
        private SfChart tripActivityLineGraph;
        private RouteActivityViewModel viewModel;
        private bool lineGraphIsFiltered;
        private SemaphoreSlim throttler;
        private bool orderByMostExceptions;
        private static object _listLock = new object();
        private Picker fuelPicker;
        private double fuelLevelLine;
        private FastLineSeries dottedFuelLineSeries;
        private double oldHeight;
        private GroupByPeriod groupByPeriod;

        public ReportHeaderLayout ReportHeader { get; private set; }

        public RouteActivityPage() : base()
        {
            InitializeComponent();

            this.Title = AppResources.report_title_driver_route_activity;

            viewModel = new RouteActivityViewModel();

            orderByMostExceptions = true;

            BindingContext = viewModel;

            throttler = new SemaphoreSlim(initialCount: 10);


            SetupUI();

            this.ReportHeader.DatePickerLayout.PropertyChanged += Date_PropertyChanged;
            this.ReportHeader.SearchTapped += OnSearch_Tapped;



            LoadData();
        }

        private async Task LoadData()
        {
            try
            {
                App.ShowLoading(true);
                await SetUpTripActivityLineGraphData(loadData: true);
                await SetUpTable();
                App.ShowLoading(false);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                await DisplayAlert("Error", "An error occured retrieving data. Please try again", "Ok");
            }
            
        }

        private async Task SetUpTable()
        {
            viewModel.ExceptionInfoCollection.Clear();

            if (orderByMostExceptions)
                await viewModel.SetVehicleData(true);
            else
                await viewModel.SetVehicleData(false);

            var rows = new Collection<List<string>>();
            List<VehicleRowItem> vehicleList = viewModel.VehicleRowItems;

            foreach (var item in vehicleList)
            {
                ExceptionSummaryInfo exceptionInfo;
                if (item != null)
                {
                    exceptionInfo = new ExceptionSummaryInfo
                    {
                        VehicleId = item.VehicleId,
                        UnitId = item.VehicleDescription,
                        VehicleDescription = await VehicleHelper.GetVehicleDescriptionFromIdAsync(item.VehicleId, App.SelectedVehicleGroup),
                        Registration = item.Registration,
                        Count = item.Count.ToString(),
                        MaxSpeed = $"{Math.Round(item.MaxSpeed, 2).ToString()} {App.GetDistanceAbbreviation() + "/h"}",
                        IdleDuration = FormatHelper.ToShortForm(TimeSpan.FromSeconds(item.IdleDuration)),
                        Distance = $"{Math.Round(item.Distance, 2).ToString()} {App.GetDistanceAbbreviation()}",
                        Duration = FormatHelper.ToShortForm(TimeSpan.FromSeconds(item.Duration))

                    };
                    if ((item.MaxRPM ?? 0) == 0)
                        exceptionInfo.MaxRPM = "N/A";
                    else
                        exceptionInfo.MaxRPM = item.MaxRPM.Value.ToString();
                    
                }

                else
                {
                    exceptionInfo = new ExceptionSummaryInfo();
                }

                viewModel.ExceptionInfoCollection.Add(exceptionInfo);
            }

            dataGrid.ItemsSource = viewModel.ExceptionInfoCollection;
            return;
        }

        private void SetupUI()
        {
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });

            fuelPicker = new Picker { Title = AppResources.fuel, HorizontalOptions = LayoutOptions.FillAndExpand};
            fuelPicker.Items.Add(AppResources.vehicle_group_average);
            fuelPicker.Items.Add(AppResources.custom);
            fuelPicker.SelectedIndexChanged += FuelPicker_SelectedIndexChanged;

            var pickerCollection = new Collection<View>();
            pickerCollection.Add(fuelPicker);

            ReportHeader = new ReportHeaderLayout(pickerCollection);
            mainStack.Children.Insert(0, ReportHeader);

            //Most/Least switcher
            var tableLabelTapGesture = new TapGestureRecognizer();
            tableLabelTapGesture.Tapped += TableLabelTapGesture_Tapped;
            TableLabel.GestureRecognizers.Add(tableLabelTapGesture);

            //Graph toggle button
            //var toggleGesture = new TapGestureRecognizer();
            //toggleGesture.Tapped += ToggleGesture_Tapped;
            //graphToggleButton.GestureRecognizers.Add(toggleGesture);
            graphToggleButton.Clicked += ToggleGesture_Tapped;

            var tripActivityTitleTapGesture = new TapGestureRecognizer();
            tripActivityTitleTapGesture.Tapped += TripActivityTitleTapGesture_Tapped; ;
            tripActivityChartLabel.GestureRecognizers.Add(tripActivityTitleTapGesture);

            //double height = 0;

            //switch (Device.Idiom)
            //{
            //    case TargetIdiom.Phone:
            //        //height = App.ScreenHeight / 3;
            //        height = 200;
            //        break;
            //    case TargetIdiom.Tablet:
            //        //height = App.ScreenHeight / 3;
            //        height = 250;
            //        break;
            //}

            double height = 0;

            switch (Device.Idiom)
            {
                case TargetIdiom.Phone:
                    //height = App.ScreenHeight / 3;
                    height = 200;
                    break;
                case TargetIdiom.Tablet:
                    //height = App.ScreenHeight / 3;
                    height = 250;
                    break;
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                foreach (RowDefinition rowDefinition in MainGrid.RowDefinitions)
                {
                    rowDefinition.Height = App.ScreenWidth;
                }
            }

            //Instantiate chart
            tripActivityLineGraph = new SfChart { InputTransparent = true, HeightRequest = height, Legend = new ChartLegend { DockPosition = LegendPlacement.Bottom }, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };

            tripActivityLineGraph.PrimaryAxis = new DateTimeAxis { IntervalType = DateTimeIntervalType.Days, Interval = 1, LabelStyle = new ChartAxisLabelStyle { LabelFormat = "dd/MMM" } };
            tripActivityLineGraph.SecondaryAxis = new NumericalAxis();

            dottedFuelLineSeries = new FastLineSeries { Label = AppResources.fuel_marker, Color = Color.FromHex("#90A84E") };
            dottedFuelLineSeries.StrokeDashArray = new double[] { 10, 10 };
            tripActivityLineGraph.Series.Add(dottedFuelLineSeries);

            //Add charts to grid
            fuelGraphStack.Children.Add(tripActivityLineGraph);

            //Set up data grid
            dataGrid = new SfDataGrid { HorizontalOptions = LayoutOptions.FillAndExpand, ColumnSizer = ColumnSizer.Star, AutoGenerateColumns = false, GridStyle = new GridStyle(), SelectionMode = SelectionMode.SingleDeselect };
            dataGrid.SelectionChanged += DataGrid_SelectionChanged;


            //Data grid columns

            GridTextColumn unitIdColumn = new GridTextColumn { };
            unitIdColumn.MappingName = "UnitId";
            unitIdColumn.HeaderText = AppResources.device_id;

            GridTextColumn descriptionColumn = new GridTextColumn { };
            descriptionColumn.MappingName = "VehicleDescription";
            descriptionColumn.HeaderText = AppResources.vehicle_description;

            GridTextColumn registrationIdColumn = new GridTextColumn { };
            registrationIdColumn.MappingName = "Registration";
            registrationIdColumn.HeaderText = AppResources.registration;

            GridTextColumn exceptionCountColumn = new GridTextColumn { };
            exceptionCountColumn.MappingName = "Count";
            exceptionCountColumn.HeaderText = AppResources.exception_count;

            GridTextColumn maxSpeedColumn = new GridTextColumn { };
            maxSpeedColumn.MappingName = "MaxSpeed";
            maxSpeedColumn.HeaderText = AppResources.max_speed;

            GridTextColumn idleDurColumn = new GridTextColumn { };
            idleDurColumn.MappingName = "IdleDuration";
            idleDurColumn.HeaderText = AppResources.idle_dur;

            GridTextColumn distanceColumn = new GridTextColumn { };
            distanceColumn.MappingName = "Distance";
            distanceColumn.HeaderText = AppResources.distance;

            GridTextColumn durationColumn = new GridTextColumn { };
            durationColumn.MappingName = "Duration";
            durationColumn.HeaderText = AppResources.drive_duration;

            dataGrid.Columns.Add(unitIdColumn);
            dataGrid.Columns.Add(descriptionColumn);
            dataGrid.Columns.Add(registrationIdColumn);
            dataGrid.Columns.Add(exceptionCountColumn);
            dataGrid.Columns.Add(maxSpeedColumn);
            dataGrid.Columns.Add(idleDurColumn);
            dataGrid.Columns.Add(distanceColumn);
            dataGrid.Columns.Add(durationColumn);

           

            TableStack.Children.Add(dataGrid);
            
        }

        private async void TableLabelTapGesture_Tapped(object sender, EventArgs e)
        {
            orderByMostExceptions = (!orderByMostExceptions);

            if (orderByMostExceptions)
                TableLabel.Text = AppResources.most_exceptions;
            else
                TableLabel.Text = AppResources.least_exceptions;

            await SetUpTable();
        }


        private void FuelPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;

            var oldFuelLevel = fuelLevelLine;

            if (picker.Items[picker.SelectedIndex] == AppResources.vehicle_group_average)
            {
                fuelLevelLine = viewModel.GetAverageFuelLevel();
            }
            else
            {
                var popup = new EntryPopup("Enter a value:", string.Empty, "OK", "Cancel");
                popup.PopupClosed += (o, closedArgs) => {
                    if (closedArgs.Button == "OK")
                    {
                        var isNumber = double.TryParse(closedArgs.Text, out fuelLevelLine);
                        if (!isNumber)
                            DisplayAlert("Value invalid", "Please enter a valid value", "OK");
                        else
                        {
                            ResetLine(dottedFuelLineSeries, fuelLevelLine);
                        }
                    }
                };

                popup.Show();
            }

            if (oldFuelLevel != fuelLevelLine)
                ResetLine(dottedFuelLineSeries, fuelLevelLine);
        }

        private void ResetLine(FastLineSeries _dottedFuelLineSeries, double _fuelLine)
        {
            if (tripActivityLineGraph.Series.FirstOrDefault() == null)
            {
                return;
            }

            var tempStartDate = viewModel.StartDate;
            var tempEndDate = viewModel.EndDate;
            tripActivityLineGraph.Series.Remove(_dottedFuelLineSeries);

            var dataPoints = new Collection<ChartDataPoint>();
            foreach (var dataPoint in (Collection<ChartDataPoint>)tripActivityLineGraph.Series.First().ItemsSource)
            {
                dataPoints.Add(new ChartDataPoint(dataPoint.XValue, _fuelLine));
                dataPoints.Add(new ChartDataPoint(dataPoint.XValue, _fuelLine));
            }

            var newLineSeries = dottedFuelLineSeries = new FastLineSeries { ItemsSource = dataPoints, Label = _dottedFuelLineSeries.Label, StrokeDashArray = _dottedFuelLineSeries.StrokeDashArray, Color = _dottedFuelLineSeries.Color };
            tripActivityLineGraph.Series.Add(newLineSeries);
            Debug.WriteLine("Dotted distance line series added to line graph");
        }

        private void ToggleGesture_Tapped(object sender, EventArgs e)
        {
            ToggleGrids();
        }

        private async void DataGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            var selectedItem = (ExceptionSummaryInfo)e.AddedItems.FirstOrDefault();
            var deselectedItem = (ExceptionSummaryInfo)e.RemovedItems.FirstOrDefault();
            Debug.WriteLine("Selected: " + selectedItem.VehicleId);
            if (selectedItem != null)
            {
                await SetUpTripActivityLineGraphData(selectedItem.VehicleId);
                ToggleGrids(selectedItem);
            }
            else
            {
                await SetUpTripActivityLineGraphData();
            }
            var dataGrid = sender as SfDataGrid;
            dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
            dataGrid.SelectedItems.Clear();
            dataGrid.SelectionChanged += DataGrid_SelectionChanged;
        }

        private void ToggleGrids(ExceptionSummaryInfo selectedItem = null)
        {
            var isShowingDataGrid = TableStack.Children.Contains(dataGrid);

            if (isShowingDataGrid && selectedItem != null)
            {
                graphToggleButton.IsVisible = true;
                SetUpTripGrid(selectedItem);
                Debug.WriteLine("Line 343");
                TableStack.Children[TableStack.Children.IndexOf(dataGrid)].IsVisible = false;
                if (TableStack.Children.Contains(tripGrid))
                    TableStack.Children[TableStack.Children.IndexOf(tripGrid)].IsVisible = true;
                else
                    TableStack.Children.Add(tripGrid);
                Debug.WriteLine("Line 346");
                if (oldHeight == 0)
                    oldHeight = TableStack.Height;
                TableStack.HeightRequest = 450;
                Debug.WriteLine("Line 350");
                scrollView.ScrollToAsync(TableStack, ScrollToPosition.End, true);
                Debug.WriteLine("Line 352");
            }
            else
            {
                graphToggleButton.IsVisible = false;
                TableStack.Children[TableStack.Children.IndexOf(tripGrid)].IsVisible = false;
                TableStack.Children[TableStack.Children.IndexOf(dataGrid)].IsVisible = true;
                TableStack.HeightRequest = oldHeight;
            }
        }

        private void SetUpTripGrid(ExceptionSummaryInfo selectedItem)
        {
            Debug.WriteLine("Line 361");
            var data = viewModel.TripData.Where(x => x.VehicleId == selectedItem.VehicleId && x.Distance > 0.1).OrderByDescending(x => x.StartLocalTimestamp);

            tripGrid = new SfDataGrid { HorizontalOptions = LayoutOptions.CenterAndExpand, ColumnSizer = ColumnSizer.Auto, ScrollingMode = ScrollingMode.PixelLine, AutoGenerateColumns = false, GridStyle = new GridStyle(), SelectionMode = SelectionMode.None };

            Debug.WriteLine("Line 367");
            //Trip grid columns
            GridTextColumn timeStartColumn = new GridTextColumn { };
            timeStartColumn.MappingName = "StartTime";
            timeStartColumn.HeaderText = AppResources.start;

            GridTextColumn timeEndColumn = new GridTextColumn { };
            timeEndColumn.MappingName = "EndTime";
            timeEndColumn.HeaderText = AppResources.end;

            GridTextColumn tripDistanceColumn = new GridTextColumn { };
            tripDistanceColumn.MappingName = "DistanceAsString";
            tripDistanceColumn.HeaderText = AppResources.distance;

            GridTextColumn stopDurColumn = new GridTextColumn { };
            stopDurColumn.MappingName = "StopDurationAsString";
            stopDurColumn.HeaderText = AppResources.stop_duration;

            GridTextColumn tripMaxSpeedColumn = new GridTextColumn { };
            tripMaxSpeedColumn.MappingName = "MaxSpeedAsString";
            tripMaxSpeedColumn.HeaderText = AppResources.max_speed;

            GridTextColumn fuelConsColumn = new GridTextColumn { };
            fuelConsColumn.MappingName = "FuelConsumptionAsString";
            fuelConsColumn.HeaderText = AppResources.fuel_used;

            GridTextColumn exceptionColumn = new GridTextColumn { };
            exceptionColumn.MappingName = "NumberOfExceptions";
            exceptionColumn.HeaderText = AppResources.exception_count;

            GridTextColumn endLocColumn = new GridTextColumn { };
            endLocColumn.MappingName = "EndLocation";
            endLocColumn.TextAlignment = TextAlignment.Start;
            endLocColumn.HeaderText = AppResources.end_location;

            tripGrid.Columns.Add(timeStartColumn);
            tripGrid.Columns.Add(timeEndColumn);
            tripGrid.Columns.Add(tripDistanceColumn);
            tripGrid.Columns.Add(stopDurColumn);
            tripGrid.Columns.Add(tripMaxSpeedColumn);
            tripGrid.Columns.Add(fuelConsColumn);
            tripGrid.Columns.Add(exceptionColumn);
            tripGrid.Columns.Add(endLocColumn);

            tripGrid.GroupColumnDescriptions.Add(new GroupColumnDescription()
            {
                ColumnName = "StartLocalTimestamp",
                Converter = new GroupDateConverter()
            });
            tripGrid.GroupCaptionTextFormat = "{Key}";
            tripGrid.AllowSorting = true;

            tripGrid.SortComparers.Add(new SortComparer()
            {
                PropertyName = "StartLocalTimestamp",
                Comparer = new CustomComparer()
            });

            tripGrid.SortColumnDescriptions.Add(new SortColumnDescription()
            {
                ColumnName = "StartLocalTimestamp",
                SortDirection = ListSortDirection.Descending
            });
            tripGrid.AllowGroupExpandCollapse = true;

            tripGrid.ItemsSource = new ObservableCollection<Trip>(data);
            Debug.WriteLine("Distance: " + ((ObservableCollection<Trip>)tripGrid.ItemsSource)[0].DistanceAsString);


        }

        private async Task SetUpTripActivityLineGraphData(string vehicleId = default(string), bool loadData = false)
        {
            //Clear series
            tripActivityLineGraph.Series.Clear();
            
            if (!string.IsNullOrEmpty(vehicleId))
            {
                //eventData = viewModel.EventData.Where(x => x.VehicleId == vehicleId).ToList();
                lineGraphIsFiltered = true;
            }
            else
            {
                lineGraphIsFiltered = false;
                //eventData = viewModel.EventData;
            }
            
            
            var tempStartDate = viewModel.StartDate;
            var tempEndDate = viewModel.EndDate;
            groupByPeriod = (tempEndDate - tempStartDate).TotalDays <= 1 ? GroupByPeriod.byhour : GroupByPeriod.byday;

            var fuelConsumptionSeries = new ObservableCollection<ChartDataPoint>();
            var distanceSeries = new ObservableCollection<ChartDataPoint>();


            if (tempStartDate > tempEndDate)
                return;

            var vehicleGroup = App.SelectedVehicleGroup;
            int interval;
            int periods;

            switch (groupByPeriod)
            {
                case (GroupByPeriod.byday):
                    interval = 1;
                    periods = (int)(tempEndDate - tempStartDate).TotalDays;
                    break;
                case (GroupByPeriod.byhour):
                    interval = 2;
                    var totalPeriods = (int)(tempEndDate - tempStartDate).TotalHours;
                    periods = totalPeriods / interval;
                    break;
                case (GroupByPeriod.byweek):
                default:
                    interval = 7;
                    periods = (int)(tempEndDate - tempStartDate).TotalDays / 7;
                    break;
            }

            List<DateTime> dates = new List<DateTime>();
            for (int i = 0; i <= periods; i++)
            {
                DateTime tempDate;
                if (groupByPeriod == GroupByPeriod.byday)
                    tempDate = tempStartDate.AddDays(i);
                else if (groupByPeriod == GroupByPeriod.byhour)
                    tempDate = tempStartDate.AddHours(i);
                else
                    tempDate = tempStartDate.AddDays(i * 7);
                dates.Add(tempDate);
            }

            if (loadData)
            {
                var allTasks = new List<Task>();
                var vehicles = await VehicleAPI.GetVehicleInGroupAsync(App.SelectedVehicleGroup);
                List<AggregatedTripData> tripList = new List<AggregatedTripData>();

                viewModel.TripData.Clear();
                viewModel.FuelConsumption.Clear();

                foreach (var vehicle in vehicles)
                {
                    await throttler.WaitAsync();
                    allTasks.Add(
                                Task.Run(async () =>
                                {
                                    try
                                    {

                                        var tripData = await TripsAPI.GetTripsWithStats(vehicle.Id.ToString(), tempStartDate, tempEndDate);
                                        var fuelConsumption = (await EventAPI.GetEventsById(vehicle.Id, 172, tempStartDate, tempEndDate, true)).Where(x => x.FuelData != null && x.FuelData.Reason == 2).ToList();

                                        var startTrip = tripData.OrderBy(x => x.EndLocalTimestamp).FirstOrDefault();
                                        var endTrip = tripData.OrderByDescending(x => x.EndLocalTimestamp).FirstOrDefault();

                                        if (startTrip != default(Trip) && endTrip != default(Trip))
                                        {
                                            var startDate = startTrip.EndLocalTimestamp;
                                            var endDate = endTrip.EndLocalTimestamp;

                                            if (fuelConsumption != null && fuelConsumption.Count > 0)
                                            {
                                                foreach (Trip trip in tripData)
                                                {

                                                    var consumptionEvent = fuelConsumption
                                                    .Where(fuelEvent => fuelEvent.LocalTimestamp >= trip.EndLocalTimestamp.AddMinutes(-10) && fuelEvent.LocalTimestamp <= trip.EndLocalTimestamp.AddMinutes(10))
                                                    .FirstOrDefault();

                                                    if (consumptionEvent != default(Event))
                                                    {
                                                        if (consumptionEvent.FuelData.AverageFuelConsumption != null)
                                                            trip.FuelConsumption = FuelHelper.CalculateFuelUsed(consumptionEvent.FuelData.AverageFuelConsumption.Value);
                                                    }
                                                    
                                                }
                                            }
                                        }

                                        lock (_listLock)
                                        {
                                            viewModel.TripData = viewModel.TripData.Concat(tripData).ToList();
                                            viewModel.FuelConsumption = viewModel.FuelConsumption.Concat(fuelConsumption).ToList();
                                        }


                                    }
                                    catch (NullReferenceException e)
                                    {
                                        Log.Debug(e.Message);
                                    }
                                    finally
                                    {
                                        throttler.Release();
                                    }
                                }));
                }

                await Task.WhenAll(allTasks);
                
                
                viewModel.TripData = viewModel.TripData.OrderBy(x => x.VehicleId).ThenByDescending(x => x.StartLocalTimestamp).ToList();
                await Trip.SetExceptions(viewModel.TripData);
            }

            if (viewModel.TripData == null || viewModel.TripData.Count() == 0)
                return;

            for (int i = 0; i < viewModel.TripData.Count; i++)
            {
                if (i < viewModel.TripData.Count - 1)
                {
                    var current = viewModel.TripData[i];
                    var prev = viewModel.TripData[i + 1];

                    if (viewModel.TripData[i].VehicleId == viewModel.TripData[i + 1].VehicleId)
                        viewModel.TripData[i].StopDurationAsString = FormatHelper.ToShortForm(current.StartLocalTimestamp - prev.EndLocalTimestamp).ToString();
                }
                else
                {
                    viewModel.TripData[i].StopDurationAsString = "---";
                }

            }

            //var fuelConsumption = await FuelAPI.GetFuelConsumptionAsync(vehicleGroup, tempStartDate, tempEndDate, groupByPeriod.ToString());
            //viewModel.FuelConsumption = fuelConsumption;

            foreach (var date in dates)
            {
                DateTime endDuration;
                if (groupByPeriod == GroupByPeriod.byhour){
                    endDuration = date.AddHours(interval);
                }
                else{
                    endDuration = date.AddDays(interval);
                }

                //Distance
                double totalDistance;

                //var tripData = await TripsAPI.GetAggregatedTripDataAsync(vehicleGroup, date, endDuration);
                var tripData = viewModel.TripData.Where(x => x.StartLocalTimestamp >= date && x.StartLocalTimestamp <= endDuration);

                if (!string.IsNullOrEmpty(vehicleId))
                    tripData = tripData.Where(x => x.VehicleId == vehicleId).ToList();

                if (tripData != null)
                    totalDistance = tripData.Sum(x => x.Distance);
                else
                    totalDistance = 0;


                distanceSeries.Add(new ChartDataPoint(date, totalDistance));

                //Fuel Consumption
                double totalConsumption;
                var _consEvents = viewModel.FuelConsumption.Where(x => x.FuelData != null && x.FuelData.AverageFuelConsumption != null).Where(x => x.LocalTimestamp.Day == date.Day);
                //totalConsumption = (_consEvents.Sum(x => x.FuelData.AverageFuelConsumption.Value)*_consEvents.Sum(x => x.FuelData.TripDistance))/10000000;
                totalConsumption = FuelHelper.CalculateFuelUsed(_consEvents.Sum(x => x.FuelData.AverageFuelConsumption.Value));
                fuelConsumptionSeries.Add(new ChartDataPoint(date, totalConsumption));

            }



            Debug.WriteLine("Line 620");
            var distanceLineSeries = new LineSeries
            {
                Label = AppResources.distance,
                ItemsSource = distanceSeries,
                EnableTooltip = true
            };
            tripActivityLineGraph.Series.Add(distanceLineSeries);
            var fuelLineSeries = new LineSeries
            {
                Label = AppResources.fuel_consumption,
                ItemsSource = fuelConsumptionSeries,
                EnableTooltip = true
            };
            tripActivityLineGraph.Series.Add(fuelLineSeries);

            Debug.WriteLine("Line 636");
        }

        #region Event Methods
        private async void TripActivityTitleTapGesture_Tapped(object sender, EventArgs e)
        {
            if (lineGraphIsFiltered)
                dataGrid.SelectionMode = SelectionMode.None;
            else
                await SetUpTripActivityLineGraphData();
        }

       

        private void Date_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StartDate")
            {
                var value = (sender as DatePickerLayout).StartDate;
                if (viewModel.StartDate != value)
                {
                    viewModel.StartDate = value;
                }
                else
                    return;
            }
            if (e.PropertyName == "EndDate")
            {
                var value = (sender as DatePickerLayout).EndDate;
                if (viewModel.EndDate != value)
                {
                    viewModel.EndDate = value;
                    //await GetDriverEventData();
                    //SetUpGraphs();
                }
                else
                    return;
            }
        }
        private async void OnSearch_Tapped(object sender, EventArgs e)
        {
            await LoadData();

            //Re-configure fuel picker
            if (fuelPicker.SelectedIndex > -1 && fuelPicker.Items[fuelPicker.SelectedIndex] == AppResources.vehicle_group_average)
            {
                fuelLevelLine = viewModel.GetAverageFuelLevel();
            }
            
            ResetLine(dottedFuelLineSeries, fuelLevelLine);
        }
        #endregion
    }
}
