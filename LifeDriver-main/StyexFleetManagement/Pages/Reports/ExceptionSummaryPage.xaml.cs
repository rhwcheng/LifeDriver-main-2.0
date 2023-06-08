using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Helpers;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel;
using Syncfusion.SfChart.XForms;
using Syncfusion.SfDataGrid.XForms;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages.Reports
{
    public partial class ExceptionSummaryPage : ReportPage
    {
        private SfChart exceptionVsDistanceBarGraph;
        private SfChart totalExceptionLineGraph;
        private ExceptionSummaryViewModel viewModel;
        private SemaphoreSlim throttler;
        private string selectedVehicle;
        private static List<string> columnNames = new List<string> { AppResources.vehicle_description, AppResources.registration, AppResources.total_count, AppResources.max_speed, AppResources.max_rpm, AppResources.idle_dur, AppResources.distance, AppResources.drive_duration };
        private DateTimeAxis exceptionVsDistancePrimaryAxis;
        SfDataGrid dataGrid;

        bool totalExceptionChartIsFiltered;
        bool exceptVsDistChartIsFiltered;

        bool orderByMostExceptions;
        private GroupByPeriod groupByPeriod;

        public ExceptionSummaryPage() : base()
        {
            InitializeComponent();

            this.Title = AppResources.report_title_fleet_exception_summary;

            viewModel = new ExceptionSummaryViewModel();

            throttler = new SemaphoreSlim(initialCount: 10);

            BindingContext = viewModel;

            orderByMostExceptions = true;

            SetupUI();

            this.ReportHeader.DatePickerLayout.PropertyChanged += Date_PropertyChanged;
            this.ReportHeader.SearchTapped += OnSearch_Tapped;


            LoadData();

        }

        private async Task LoadData()
        {
            App.ShowLoading(true);
            await GetData();
            SetUpGraphs();
            await SetUpTable();
            App.ShowLoading(false);

            //await TableScrollView.ScrollToAsync(20, 0, false);
        }

        private async Task SetUpTable()
        {
            viewModel.ExceptionInfoCollection.Clear();

            if (orderByMostExceptions)
                await viewModel.SetOrderedVehicles(true);
            else
                await viewModel.SetOrderedVehicles(false);

            var rows = new Collection<List<string>>();
            List<VehicleRowItem> vehicleList = viewModel.TopFiveVehicles;

            var vehicles = await VehicleAPI.GetVehicleInGroupAsync(App.SelectedVehicleGroup);

            foreach (VehicleRowItem item in vehicleList)
            {
                ExceptionSummaryInfo exceptionInfo;
                if (item != null)
                {
                    exceptionInfo = new ExceptionSummaryInfo
                    {
                        VehicleId = item.VehicleId,
                        UnitId = item.VehicleDescription,
                        VehicleDescription = vehicles.Where(x => x.Id == item.VehicleId).First().Description,
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
            /*
            var table = new RankTable(columnNames, rows) { HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            table.SelectedRowPropertyChanged += Table_SelectedRowPropertyChanged;
            TableScrollView.Content = table;*/

        }

        private void Table_SelectedRowPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var t = sender;
        }

        private async Task GetData()
        {
            App.ShowLoading(true, isCancel: true);

            var vehicles = await VehicleAPI.GetVehicleInGroupAsync(App.SelectedVehicleGroup);
            if (vehicles == null)
                return;
            var allTasks = new List<Task>();
            var plotFleetEvents = Settings.Current.PlotFleetExceptions;
            List<Event> events = new List<Event>();
            foreach (VehicleItem vehicle in vehicles)
            {
                await throttler.WaitAsync();
                allTasks.Add(
                            Task.Run(async () =>
                            {
                                try
                                {
                                    List<Event> data;
                                    if (plotFleetEvents)
                                        data = await EventAPI.GetEventsByArrayAsync(vehicle.Id.ToString(), viewModel.StartDate, viewModel.EndDate, EventTypeGroup.FleetExceptions);
                                    else
                                        data = await EventAPI.GetEventsByArrayAsync(vehicle.Id.ToString(), viewModel.StartDate, viewModel.EndDate, EventTypeGroup.UBIExceptions);

                                    foreach (var item in data)
                                    {
                                        item.VehicleId = vehicle.Id;
                                    }

                                    Debug.WriteLine("Event for vehicle " + vehicle.Description + ": " + data.Count);
                                    events = events.Concat(data).ToList();
                                }
                                finally
                                {
                                    throttler.Release();
                                }
                            }));

            }


            await Task.WhenAll(allTasks);
            viewModel.EventData = events;

            //Get Aggregated trip data
            viewModel.AggregatedTripData = await TripsAPI.GetAggregatedTripDataAsync(App.SelectedVehicleGroup, viewModel.StartDate, viewModel.EndDate);

            App.ShowLoading(false);
        }


        private void SetUpGraphs()
        {
            groupByPeriod = (viewModel.EndDate - viewModel.StartDate).TotalDays <= 1 ? GroupByPeriod.byhour : GroupByPeriod.byday;

            if (viewModel.EventData == null || viewModel.EventData.Count == 0)
                return;

            SetUpExceptionLineGraphData();
            SetUpExceptionsVsDistanceGraphData();


        }

        private async void SetUpExceptionsVsDistanceGraphData(string vehicleId = default(string))
        {
            //Clear series
            exceptionVsDistanceBarGraph.Series.Clear();

            List<Event> eventData;
            if (!string.IsNullOrEmpty(vehicleId))
            {
                eventData = viewModel.EventData.Where(x => x.VehicleId == vehicleId).ToList();
                exceptVsDistChartIsFiltered = true;
            }
            else
            {
                exceptVsDistChartIsFiltered = false;
                eventData = viewModel.EventData;
            }

            if (eventData == null || eventData.Count == 0)
                return;

            var tempStartDate = viewModel.StartDate;
            var tempEndDate = viewModel.EndDate;

            if (tempStartDate > tempEndDate)
                return;

            var exceptionSeries = new ObservableCollection<ChartDataPoint>();
            var distanceSeries = new ObservableCollection<ChartDataPoint>();

            int interval;

            var vehicleGroup = App.SelectedVehicleGroup;

            int periods;

            switch (groupByPeriod)
            {
                case (GroupByPeriod.byday):
                    interval = 1;
                    periods = (int) (tempEndDate - tempStartDate).TotalDays;
                    break;
                case (GroupByPeriod.byhour):
                    interval = 2;
                    var totalPeriods = (int)(tempEndDate - tempStartDate).TotalHours;
                    periods = totalPeriods / interval;
                    break;
                case (GroupByPeriod.byweek):
                default:
                    interval = 7;
                    periods = (int) (tempEndDate - tempStartDate).TotalDays / 7;
                    break;
            }

            var allTasks = new List<Task>();

            List<DateTime> dates = new List<DateTime>();
            for (int i = 0; i <= periods; i++)
            {
                DateTime tempDate;
                if (groupByPeriod == GroupByPeriod.byday)
                    tempDate = tempStartDate.AddDays(i);
                else if (groupByPeriod == GroupByPeriod.byhour)
                {
                    tempDate = tempStartDate.AddHours(i);
                }
                else
                    tempDate = tempStartDate.AddDays(i * 7);
                dates.Add(tempDate);
            }
            var exceptionDataPoints = new List<ChartDataPoint>();
            var distanceDataPoints = new List<ChartDataPoint>();

            foreach (var date in dates)
            {

                await throttler.WaitAsync();
                allTasks.Add(
                            Task.Run(async () =>
                            {
                                try
                                {

                                    var endDuration = date.AddDays(interval);

                                    int totalExceptions;
                                    double totalDistance;


                                    totalExceptions = eventData.Where(x => x.LocalTimestamp > date && x.LocalTimestamp < endDuration).Count();
                                    var tripData = await TripsAPI.GetAggregatedTripDataAsync(vehicleGroup, date, endDuration);

                                    if (!string.IsNullOrEmpty(vehicleId))
                                        tripData = tripData.Where(x => x.VehicleId == vehicleId).ToList();

                                    if (tripData != null)
                                        totalDistance = tripData.Sum(x => x.TotalDistance);
                                    else
                                        totalDistance = 0;


                                    exceptionDataPoints.Add(new ChartDataPoint(date, totalExceptions));
                                    distanceDataPoints.Add(new ChartDataPoint(date, totalDistance));

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

            if (exceptionDataPoints.Count == 0 || distanceDataPoints.Count == 0)
                return;

            exceptionDataPoints = exceptionDataPoints.OrderBy(x => x.XValue).ToList();
            distanceDataPoints = distanceDataPoints.OrderBy(x => x.XValue).ToList();

            for (int i = 0; i < exceptionDataPoints.Count; i++)
            {
                if (groupByPeriod == GroupByPeriod.byweek)
                {
                    var start = (DateTime) exceptionDataPoints[i].XValue;
                    var end = start.AddDays(6);
                    var label = $"{start.ToString("dd MMM")} - {end.ToString("dd MMM")}";

                    exceptionDataPoints[i].XValue = label;
                    distanceDataPoints[i].XValue = label;
                }


                exceptionSeries.Add(exceptionDataPoints[i]);
                distanceSeries.Add(distanceDataPoints[i]);
            }

            var exceptionLineSeries = new ColumnSeries
            {
                Label = AppResources.exception_title,
                ItemsSource = exceptionSeries,
                EnableTooltip = true
            };
            exceptionVsDistanceBarGraph.Series.Add(exceptionLineSeries);
            var distanceLineSeries = new ColumnSeries
            {
                Label = AppResources.distance,
                ItemsSource = distanceSeries,
                EnableTooltip = true
            };
            exceptionVsDistanceBarGraph.Series.Add(distanceLineSeries);
        }

        private void SetUpExceptionLineGraphData(string vehicleId = default(string))
        {
            //Clear series
            totalExceptionLineGraph.Series.Clear();

            List<Event> eventData;
            if (!string.IsNullOrEmpty(vehicleId))
            {
                eventData = viewModel.EventData.Where(x => x.VehicleId == vehicleId).ToList();
                totalExceptionChartIsFiltered = true;
            }
            else
            {
                totalExceptionChartIsFiltered = false;
                eventData = viewModel.EventData;
            }

            if (eventData == null || eventData.Count == 0)
                return;

            var tempStartDate = viewModel.StartDate;
            var tempEndDate = viewModel.EndDate;
            var groupByPeriod = (tempEndDate - tempStartDate).TotalDays <= 1 ? GroupByPeriod.byhour : GroupByPeriod.byday;

            var speedingEventsSeries = new ObservableCollection<ChartDataPoint>();
            var breakingEventsSeries = new ObservableCollection<ChartDataPoint>();
            var accelerationEventsSeries = new ObservableCollection<ChartDataPoint>();
            var idleEventsSeries = new ObservableCollection<ChartDataPoint>();
            var freewheelingEventsSeries = new ObservableCollection<ChartDataPoint>();
            var recklessDrivingEventsSeries = new ObservableCollection<ChartDataPoint>();
            var excessiveRPMEventsSeries = new ObservableCollection<ChartDataPoint>();
            var accidentEventsSeries = new ObservableCollection<ChartDataPoint>();
            var corneringEventsSeries = new ObservableCollection<ChartDataPoint>();
            var laneChangeEventsSeries = new ObservableCollection<ChartDataPoint>();


            if (tempStartDate > tempEndDate)
                return;

            int interval;

            switch (groupByPeriod)
            {
                case (GroupByPeriod.byday):
                    interval = 1;
                    break;
                case (GroupByPeriod.byhour):
                    interval = 2;
                    break;
                case (GroupByPeriod.byweek):
                default:
                    interval = 7;
                    break;
            }

            while (tempStartDate < tempEndDate)
            {
                var startDuration = tempStartDate;
                DateTime endDuration;
                if (groupByPeriod == GroupByPeriod.byhour)
                {
                    endDuration = startDuration.AddHours(interval);
                }
                else
                {
                    endDuration = startDuration.AddDays(interval);
                }
                if (endDuration > tempEndDate){
                    endDuration = tempEndDate;
                }

                int totalSpeedingEvents;
                int totalBreakingEvents;
                int totalAccelerationEvents;
                int totalIdleEvents;
                int totalFreewheelingEvents;
                int totalRecklessDrivingEvents;
                int totalExcessiveRPMEvents;
                int totalAccidentEvents;
                int totalCorneringEvents = 0;
                int totalLaneChangeEvents = 0;


                if (Settings.Current.PlotFleetExceptions)
                {
                    Debug.WriteLine("Speeding:" + eventData.Where(x => (FleetException)x.EventTypeId == FleetException.SPEEDING).Count().ToString());

                    totalSpeedingEvents = eventData.Where(x => (FleetException)x.EventTypeId == FleetException.SPEEDING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalBreakingEvents = eventData.Where(x => (FleetException)x.EventTypeId == FleetException.HARSHBREAKING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalAccelerationEvents = eventData.Where(x => (FleetException)x.EventTypeId == FleetException.EXCESSIVEACCELERATION && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalIdleEvents = eventData.Where(x => (FleetException)x.EventTypeId == FleetException.EXCESSIVEIDLE && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalFreewheelingEvents = eventData.Where(x => (FleetException)x.EventTypeId == FleetException.FREEWHEELING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalRecklessDrivingEvents = eventData.Where(x => (FleetException)x.EventTypeId == FleetException.RECKLESSDRIVING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalExcessiveRPMEvents = eventData.Where(x => (FleetException)x.EventTypeId == FleetException.EXCESSIVERPM && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalAccidentEvents = eventData.Where(x => (FleetException)x.EventTypeId == FleetException.ACCIDENT && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();

                }
                else
                {
                    totalSpeedingEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.SPEEDING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalBreakingEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.EXCESSIVEBREAKING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalAccelerationEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.ACCELERATION && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalIdleEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.EXCESSIVEIDLE && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalFreewheelingEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.FREEWHEELING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalRecklessDrivingEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.SWERVING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalExcessiveRPMEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.EXCESSIVERPM && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalAccidentEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.ACCIDENT && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalCorneringEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.CORNERING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                    totalLaneChangeEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.LANECHANGE && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                }
                speedingEventsSeries.Add(new ChartDataPoint(tempStartDate, totalSpeedingEvents));
                breakingEventsSeries.Add(new ChartDataPoint(tempStartDate, totalBreakingEvents));
                accelerationEventsSeries.Add(new ChartDataPoint(tempStartDate, totalAccelerationEvents));
                idleEventsSeries.Add(new ChartDataPoint(tempStartDate, totalIdleEvents));
                freewheelingEventsSeries.Add(new ChartDataPoint(tempStartDate, totalFreewheelingEvents));
                recklessDrivingEventsSeries.Add(new ChartDataPoint(tempStartDate, totalRecklessDrivingEvents));
                excessiveRPMEventsSeries.Add(new ChartDataPoint(tempStartDate, totalExcessiveRPMEvents));
                accidentEventsSeries.Add(new ChartDataPoint(tempStartDate, totalAccidentEvents));

                if (!Settings.Current.PlotFleetExceptions)
                {
                    corneringEventsSeries.Add(new ChartDataPoint(tempStartDate, totalCorneringEvents));
                    laneChangeEventsSeries.Add(new ChartDataPoint(tempStartDate, totalLaneChangeEvents));
                }

                if (groupByPeriod == GroupByPeriod.byhour){
                    tempStartDate = tempStartDate.AddHours(interval);
                }
                else{
                    tempStartDate = tempStartDate.AddDays(interval);
                }
            }

            var speedLineSeries = new LineSeries
            {
                Label = AppResources.speeding_label,
                ItemsSource = speedingEventsSeries,
                EnableTooltip = true
            };
            totalExceptionLineGraph.Series.Add(speedLineSeries);

            var breakingLineSeries = new LineSeries
            {
                Label = AppResources.braking_label,
                ItemsSource = breakingEventsSeries,
                EnableTooltip = true
            };
            totalExceptionLineGraph.Series.Add(breakingLineSeries);


            var accelLineSeries = new LineSeries
            {
                Label = AppResources.acceleration_label,
                ItemsSource = accelerationEventsSeries,
                EnableTooltip = true
            };
            totalExceptionLineGraph.Series.Add(accelLineSeries);


            var idleLineSeries = new LineSeries
            {
                Label = AppResources.idle_label,
                ItemsSource = idleEventsSeries,
                EnableTooltip = true
            };
            totalExceptionLineGraph.Series.Add(idleLineSeries);


            var freewheelingLineSeries = new LineSeries
            {
                Label = AppResources.freewheeling_label,
                ItemsSource = freewheelingEventsSeries,
                EnableTooltip = true
            };
            totalExceptionLineGraph.Series.Add(freewheelingLineSeries);


            var recklessLineSeries = new LineSeries
            {
                ItemsSource = recklessDrivingEventsSeries,
                EnableTooltip = true
            };
            if (Settings.Current.PlotFleetExceptions)
                recklessLineSeries.Label = AppResources.reckless_driving_label;
            else
                recklessLineSeries.Label = AppResources.swerving_label;
            totalExceptionLineGraph.Series.Add(recklessLineSeries);


            var accidentLineSeries = new LineSeries
            {
                Label = AppResources.accident_label,
                ItemsSource = accidentEventsSeries,
                EnableTooltip = true
            };
            totalExceptionLineGraph.Series.Add(accidentLineSeries);


            var rpmLineSeries = new LineSeries
            {
                Label = AppResources.rpm_label,
                ItemsSource = excessiveRPMEventsSeries,
                EnableTooltip = true
            };
            totalExceptionLineGraph.Series.Add(rpmLineSeries);

            if (!Settings.Current.PlotFleetExceptions)
            {

                var corneringLineSeries = new LineSeries
                {
                    Label = AppResources.cornering_label,
                    ItemsSource = corneringEventsSeries,
                    EnableTooltip = true
                };
                totalExceptionLineGraph.Series.Add(corneringLineSeries);


                var lanceChangeLineSeries = new LineSeries
                {
                    Label = AppResources.lane_change_label,
                    ItemsSource = laneChangeEventsSeries,
                    EnableTooltip = true
                };
                totalExceptionLineGraph.Series.Add(lanceChangeLineSeries);
            }

        }

        private void SetOverlayVisibility()
        {
            /*if (totalExceptionsDataModel.Utilization.Where(x => x.YValue != 0).Count() > 0)
            {
                viewModel.TotalExceptionOverlayIsVisible = false;
                totalExceptionLineGraph.IsVisible = true;
            }
            else
            {
                viewModel.TotalExceptionOverlayIsVisible = true;
                totalExceptionLineGraph.IsVisible = false;
            }

            if (exceptionVsDistanceDataModel.Utilization.Where(x => !x.YValue.Equals(0)).Count() > 0)
            {
                viewModel.ExceptionVsDistanceOverlayIsVisible = false;
                exceptionVsDistanceBarGraph.IsVisible = true;
            }
            else
            {
                viewModel.ExceptionVsDistanceOverlayIsVisible = true;
                exceptionVsDistanceBarGraph.IsVisible = false;
            }*/
            
        }

        private void SetupUI()
        {
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });

            var totalExceptionTitleTapGesture = new TapGestureRecognizer();
            totalExceptionTitleTapGesture.Tapped += TotalExceptionTapGesture_Tapped;
            totalExceptionLabel.GestureRecognizers.Add(totalExceptionTitleTapGesture);

            var exceptionVsDistanceTitleTapGesture = new TapGestureRecognizer();
            exceptionVsDistanceTitleTapGesture.Tapped += ExceptionVsDistanceTapGesture_Tapped;
            exceptionVsDistanceLabel.GestureRecognizers.Add(exceptionVsDistanceTitleTapGesture);

            var tableLabelTapGesture = new TapGestureRecognizer();
            tableLabelTapGesture.Tapped += TableLabelTapGesture_Tapped;
            TableLabel.GestureRecognizers.Add(tableLabelTapGesture);
            
            //Set overlay background colour
            //exceptionVsDistanceOverlay.BackgroundColor = Color.FromRgba(105, 105, 105, 0.6);
            //totalExceptionOverlay.BackgroundColor = Color.FromRgba(105, 105, 105, 0.6);

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

            //Instantiate charts
            totalExceptionLineGraph = new SfChart { HeightRequest = height, Legend = new ChartLegend { DockPosition = LegendPlacement.Bottom,  }, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };
            exceptionVsDistanceBarGraph = new SfChart { InputTransparent = true, HeightRequest = height, Legend = new ChartLegend { DockPosition = LegendPlacement.Bottom }, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };


            ChartZoomPanBehavior zoomPanBehavior = new ChartZoomPanBehavior { EnablePanning = true, EnableZooming = false, EnableDoubleTap = false};

            totalExceptionLineGraph.ChartBehaviors.Add(zoomPanBehavior);

            totalExceptionLineGraph.PrimaryAxis = new DateTimeAxis { IntervalType = DateTimeIntervalType.Days, Interval = 1, LabelStyle = new ChartAxisLabelStyle { LabelFormat = "dd/MMM" } };
            totalExceptionLineGraph.SecondaryAxis = new NumericalAxis();
            
            exceptionVsDistanceBarGraph.PrimaryAxis = new CategoryAxis { LabelPlacement = LabelPlacement.BetweenTicks, LabelStyle = new ChartAxisLabelStyle { LabelFormat = "dd/MMM" } };
            exceptionVsDistanceBarGraph.SecondaryAxis = new NumericalAxis();

            //Add charts to grid
            totalExceptionGraphStack.Children.Add(totalExceptionLineGraph);
            exceptionVsDistanceGraphStack.Children.Add(exceptionVsDistanceBarGraph);

            //Set up data grid
            dataGrid = new SfDataGrid { HorizontalOptions = LayoutOptions.FillAndExpand, ColumnSizer = ColumnSizer.Star, AutoGenerateColumns = false, GridStyle = new GridStyle(), SelectionMode = Syncfusion.SfDataGrid.XForms.SelectionMode.SingleDeselect};
            dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            

            GridTextColumn unitIdColumn = new GridTextColumn {};
            unitIdColumn.MappingName = "UnitId";
            unitIdColumn.HeaderText = AppResources.device_id;

            GridTextColumn descriptionColumn = new GridTextColumn { };
            descriptionColumn.MappingName = "VehicleDescription";
            descriptionColumn.HeaderText = AppResources.vehicle_description;

            GridTextColumn registrationIdColumn = new GridTextColumn {  };
            registrationIdColumn.MappingName = "Registration";
            registrationIdColumn.HeaderText = AppResources.registration;

            GridTextColumn countColumn = new GridTextColumn { };
            countColumn.MappingName = "Count";
            countColumn.HeaderText = AppResources.total_count;

            GridTextColumn maxSpeedColumn = new GridTextColumn {  };
            maxSpeedColumn.MappingName = "MaxSpeed";
            maxSpeedColumn.HeaderText = AppResources.max_speed;

            GridTextColumn maxRPMColumn = new GridTextColumn { };
            maxRPMColumn.MappingName = "MaxRPM";
            maxRPMColumn.HeaderText = AppResources.max_rpm;

            GridTextColumn idleDurColumn = new GridTextColumn {  };
            idleDurColumn.MappingName = "IdleDuration";
            idleDurColumn.HeaderText = AppResources.idle_dur;

            GridTextColumn distanceColumn = new GridTextColumn { };
            distanceColumn.MappingName = "Distance";
            distanceColumn.HeaderText = AppResources.distance;

            GridTextColumn durationColumn = new GridTextColumn{ };
            durationColumn.MappingName = "Duration";
            durationColumn.HeaderText = AppResources.drive_duration;

            dataGrid.Columns.Add(unitIdColumn);
            dataGrid.Columns.Add(descriptionColumn);
            dataGrid.Columns.Add(registrationIdColumn);
            dataGrid.Columns.Add(countColumn);
            dataGrid.Columns.Add(maxSpeedColumn);
            dataGrid.Columns.Add(maxRPMColumn);
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

        private void ExceptionVsDistanceTapGesture_Tapped(object sender, EventArgs e)
        {
            if (exceptVsDistChartIsFiltered && !totalExceptionChartIsFiltered)
                dataGrid.SelectionMode = Syncfusion.SfDataGrid.XForms.SelectionMode.None;
            else
                SetUpExceptionsVsDistanceGraphData();

        }

        private void TotalExceptionTapGesture_Tapped(object sender, EventArgs e)
        {

            if (!exceptVsDistChartIsFiltered && totalExceptionChartIsFiltered)
                dataGrid.SelectionMode = Syncfusion.SfDataGrid.XForms.SelectionMode.None;
            else
                SetUpExceptionLineGraphData();

        }

        private void DataGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            var selectedItem = (ExceptionSummaryInfo) e.AddedItems.FirstOrDefault();
            var deselectedItem = (ExceptionSummaryInfo)e.RemovedItems.FirstOrDefault();

            if (selectedItem != null)
            {
                SetUpExceptionLineGraphData(selectedItem.VehicleId);
                SetUpExceptionsVsDistanceGraphData(selectedItem.VehicleId);
            }
            else
            {
                SetUpExceptionLineGraphData();
                SetUpExceptionsVsDistanceGraphData();
            }
            
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
        }
    }
}
