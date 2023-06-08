using Akavache;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.Statics;
using Syncfusion.SfChart.XForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin;
using Xamarin.Forms;
using System.Reactive.Linq;
using Acr.UserDialogs.Infrastructure;
using StyexFleetManagement.Helpers;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages;

namespace StyexFleetManagement.CustomControls
{
    public partial class DashboardTile : ContentView
    {
        private DateTime startDate;
        private DateTime endDate;
        private DashboardTileType tileType;
        private ReportDateRange reportDateRange;
        private List<Event> eventData;
        private int goalValue;
        private int vehiclesInSideGoal;
        private int vehiclesOutsideGoal;
        private List<AggregatedTripData> tripData;
        private double average;
        private bool fullScreen;
        private List<ChartDataPoint> lineSeriesData;
        private SemaphoreSlim throttler;
        private ObservableCollection<ChartDataPoint> barData;
        private DateTimeAxis lineXAxis;
        private List<ConsumptionItem> fuelConsumption;
        private static object _listLock = new object();
        private string vehicleGroup;
        private List<Event> fuelEvents;
        private bool goalIsAbove;

        public DashboardTile(DashboardTileType tileType, List<Event> eventData = null, bool isFullScreen = false) : base()
        {
            BindingContext = this;
            InitializeComponent();

            goalIsAbove = false;

            vehicleGroup = Settings.Current.DashboardVehicleGroup;

            barData = new ObservableCollection<ChartDataPoint>();
            lineSeriesData = new List<ChartDataPoint>();

            fullScreen = isFullScreen;

            throttler = new SemaphoreSlim(initialCount: 10);

            tripData = new List<AggregatedTripData>();

            fuelEvents = new List<Event>();

            this.tileType = tileType;
            this.eventData = eventData;

            SetUpGestureRecognizers();

        }

        public async Task Init()
        {
            await SetReportDateRange();

            datePicker.SelectedDate = reportDateRange;
            datePicker.PropertyChanged += DatePicker_PropertyChanged;

            //Get updated tile
            SetUpTile();
        }


        private async Task SetReportDateRange()
        {
            var cache = BlobCache.LocalMachine;
            string key = tileType.ToString();

            try
            {
                reportDateRange = await cache.GetObject<ReportDateRange>(key);
            }
            catch (KeyNotFoundException)
            {
                reportDateRange = ReportDateRange.TODAY;
            }

            startDate = DateHelper.GetDateRangeStartDate(reportDateRange);
            endDate = DateHelper.GetDateRangeEndDate(reportDateRange);
        }

        private async void DatePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedDate")
            {
                var value = (sender as DatePickerLayout).SelectedDate;
                if (reportDateRange != value)
                {
                    reportDateRange = value;

                    await BlobCache.LocalMachine.InsertObject<ReportDateRange>(tileType.ToString(), value);

                    startDate = DateHelper.GetDateRangeStartDate(reportDateRange);
                    endDate = DateHelper.GetDateRangeEndDate(reportDateRange);
                    SetUpTile();
                }
                else
                    return;
            }

        }

        private async void SetUpTile()
        {
            var resposne = await SetUpTileData();

            var tempHeader = header;
            switch (resposne)
            {
                case DasboardTileResponse.Error:
                    mainStack.Children.Clear();
                    mainStack.Children.Add(tempHeader);
                    mainStack.Children.Add(new Label { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Text = AppResources.error_dashboard_tile });
                    break;
                case DasboardTileResponse.NoData:
                    mainStack.Children.Clear();
                    mainStack.Children.Add(tempHeader);
                    mainStack.Children.Add(new Label { HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Text = AppResources.no_data_message });
                    break;
                default:
                    break;
            }
        }

        private void SetUpGestureRecognizers()
        {
            var tileTappedGesture = new TapGestureRecognizer { Command = new Command(() => ShowPopup()) };
            ContentStack.GestureRecognizers.Add(tileTappedGesture);

            var datePickerGesture = new TapGestureRecognizer { Command = new Command(() => datePicker.FocusDatePicker()) };
            calloutButton.GestureRecognizers.Add(datePickerGesture);
        }

        private void ShowPopup()
        {
            if (!fullScreen)
            {
                ((MainPage)App.MainDetailPage).NavigateTo(typeof(ReportTilePage), tileType, eventData);
            }
        }

        private async Task<DasboardTileResponse> SetUpTileData()
        {
            try
            {
                barData.Clear();
                lineSeriesData.Clear();

                int totalPeriods;

                int count = 0;
                if (!fullScreen)
                    count = 5; //Take 5 from watchlist
                else
                    count = 10;

                var allTasks = new List<Task>();
                var dateFilteredEventData = eventData.Where(x => x.LocalTimestamp >= startDate && x.LocalTimestamp <= endDate);

                int periods;
                int interval;
                var groupByPeriod = GroupByPeriod.byday;
                var tempStartDate = startDate;
                var tempEndDate = endDate;

                if (tempStartDate > tempEndDate)
                    return DasboardTileResponse.Error;

                var tasks = new List<Task>();
                var vehicles = await VehicleAPI.GetVehicleInGroupAsync(vehicleGroup);
                var tripDataWithStats = new List<Trip>();
                Color valueColour;

                switch (reportDateRange)
                {
                    case ReportDateRange.TODAY:
                        subTitleLabel.Text = AppResources.today;
                        groupByPeriod = GroupByPeriod.byhour;
                        break;
                    case ReportDateRange.LAST_SEVEN_DAYS:
                        subTitleLabel.Text = string.Format(AppResources.last_x_days, "7");
                        break;
                    case ReportDateRange.THIS_MONTH:
                        subTitleLabel.Text = AppResources.this_month;
                        break;
                    case ReportDateRange.PREVIOUS_MONTH:
                        subTitleLabel.Text = AppResources.previous_month;
                        break;
                }

                switch (groupByPeriod)
                {
                    case (GroupByPeriod.byhour):
                        interval = 2;
                        totalPeriods = (int)(DateTime.Now - tempStartDate).TotalHours;
                        periods = totalPeriods / interval;
                        break;
                    case (GroupByPeriod.byday):
                        interval = 1;
                        totalPeriods = periods = (int)(tempEndDate - tempStartDate).TotalDays;
                        break;
                    case (GroupByPeriod.byweek):
                        interval = 7;
                        totalPeriods = periods = (int)(tempEndDate - tempStartDate).TotalDays / 7;
                        break;
                    default:
                        interval = 7;
                        totalPeriods = periods = (int)(tempEndDate - tempStartDate).TotalDays / 7;
                        break;
                }


                List<DateTime> dates = new List<DateTime>();
                for (int i = 0; i <= periods; i++)
                {
                    DateTime tempDate = new DateTime();
                    if (groupByPeriod == GroupByPeriod.byhour)
                        tempDate = tempStartDate.AddHours(interval * i);
                    if (groupByPeriod == GroupByPeriod.byday)
                        tempDate = tempStartDate.AddDays(i);
                    if (groupByPeriod == GroupByPeriod.bymonth || groupByPeriod == GroupByPeriod.byweek)
                        tempDate = tempStartDate.AddDays(i * 7);
                    dates.Add(tempDate);
                }


                switch (tileType)
                {

                    case DashboardTileType.ACCIDENT_COUNT:
                        #region
                        titleLabel.Text = AppResources.accident_count;

                        goalValue = Settings.Current.AccidentCountThreshold;

                        goalLabel.Text = $"{AppResources.goal}: {AppResources.below} {goalValue.ToString()}";

                        List<Event> accidentData = await GetEventsById(startDate, endDate, vehicles, 26);

                        //Calculate daily average

                        foreach (var date in dates)
                        {
                            DateTime endDuration = (groupByPeriod == GroupByPeriod.byhour) ? date.AddHours(interval) : date.AddDays(interval);
                            int accidentCount = 0;

                            if (accidentData != null && accidentData.Count > 0)
                                accidentCount = accidentData.Where(x => x.LocalTimestamp >= date && x.LocalTimestamp <= endDuration).Count();

                            lineSeriesData.Add(new ChartDataPoint(date, accidentCount));

                        }

                        /* For accidents, don't calc period average
                        if (accidentData != null && accidentData.Count > 0)
                            average = accidentData.Count() / totalPeriods;
                        else
                            average = 0;
                            */

                        if (accidentData != null && accidentData.Count > 0)
                            average = accidentData.Count();

                        if (average >= goalValue)
                        {
                            valueColour = Color.Red;
                            headerImage.Source = "ic_accident_count_red";
                        }
                        else
                        {
                            valueColour = Color.Green;
                            headerImage.Source = "ic_accident_count_green";
                        }


                        if (accidentData != null && accidentData.Count > 0)
                        {
                            //Calculate vehicles inside and outside of goal
                            var groupedAccidentData = accidentData.GroupBy(x => x.VehicleId).Select(g => new { VehicleId = g.First().VehicleId, UnitId = g.First().UnitId, Count = g.Count() });

                            vehiclesInSideGoal = groupedAccidentData.Where(x => x.Count < goalValue).Count();
                            vehiclesOutsideGoal = groupedAccidentData.Where(x => x.Count >= goalValue).Count();

                            //Calculate watch list
                            var accidentWatchList = groupedAccidentData.OrderByDescending(x => x.Count).Take(10).ToList();

                            //Set up bar data
                            count = Math.Min(accidentWatchList.Count, count);
                            for (int i = 0; i < count; i++)
                            {
                                if (accidentWatchList[i] == null)
                                    break;

                                string vehicleDesciption = vehicles.Where(x => x.Id == accidentWatchList[i].VehicleId).First().Description;
                                vehicleDesciption = vehicleDesciption.Substring(0, Math.Min(vehicleDesciption.Length, 7));

                                barData.Add(new ChartDataPoint(vehicleDesciption, accidentWatchList[i].Count));
                            }
                        }

                        SetValueLabel(average.ToString(), vehiclesInSideGoal.ToString(), (vehiclesInSideGoal + vehiclesOutsideGoal).ToString(), valueColour);
                        break;
                    #endregion
                    case DashboardTileType.DISTANCE:
                        #region
                        titleLabel.Text = AppResources.distance;
                        goalValue = Settings.Current.DistanceDrivenThreshold;
                        goalIsAbove = true;
                        var goalString =
                            $"{goalValue} {DistanceMeasurementUnit.GetAbbreviation(Settings.Current.DistanceMeasurementUnit)}";
                        tripData.Clear();
                        goalLabel.Text = $"{AppResources.goal}: {AppResources.above} {goalString} {""}";


                        //Calculate daily average

                        foreach (var date in dates)
                        {

                            await throttler.WaitAsync();
                            allTasks.Add(
                                        Task.Run(async () =>
                                        {
                                            try
                                            {

                                                DateTime endDuration = (groupByPeriod == GroupByPeriod.byhour) ? date.AddHours(interval) : date.AddDays(interval);

                                                var periodTripData = await TripsAPI.GetAggregatedTripDataAsync(vehicleGroup, date, endDuration);

                                                var dailyDistance = periodTripData.Sum(x => x.TotalDistance);

                                                lineSeriesData.Add(new ChartDataPoint(date, dailyDistance));

                                                tripData = tripData.Concat(periodTripData).ToList();

                                            }
                                            catch (NullReferenceException e)
                                            {
                                                Log.Debug(e.GetType().Name, e.Message);
                                            }
                                            finally
                                            {
                                                throttler.Release();
                                            }
                                        }));

                        }


                        await Task.WhenAll(allTasks);

                        lineSeriesData = lineSeriesData.OrderBy(x => x.XValue).ToList();

                        var groupedTripData = tripData.Where(x => x.TotalDistance > 0).GroupBy(x => x.VehicleId);

                        //var test = await TripsAPI.GetAggregatedTripDataAsync(vehicleGroup, startDate, endDate);
                        //Debug.WriteLine("Test:" + test.Average(x => x.TotalDistance));
                        average = tripData.GroupBy(x => x.VehicleId).Average(group => group.Sum(y => y.TotalDistance));
                        //Debug.WriteLine("Actual:" + average);

                        if (average <= goalValue)
                        {
                            valueColour = Color.Red;
                            headerImage.Source = "ic_distance_driven_red";
                        }
                        else
                        {
                            valueColour = Color.Green;
                            headerImage.Source = "ic_distance_driven_green";
                        }

                        //Calculate vehicles inside and outside of goal
                        vehiclesInSideGoal = groupedTripData.Where(group => group.Sum(x => x.TotalDistance) > goalValue).Count();
                        vehiclesOutsideGoal = groupedTripData.Where(group => group.Sum(x => x.TotalDistance) <= goalValue).Count();

                        SetValueLabel(FormatHelper.FormatDistance(Math.Round(average, 2).ToString()), vehiclesInSideGoal.ToString(), (vehiclesInSideGoal + vehiclesOutsideGoal).ToString(), valueColour);

                        //Calculate watch list
                        var distanceWatchList = groupedTripData.OrderBy(group => group.Sum(x => x.TotalDistance)).Take(10).ToList();

                        //Set up bar data
                        count = Math.Min(distanceWatchList.Count, count);
                        for (int i = 0; i < count; i++)
                        {
                            if (distanceWatchList[i] == null && distanceWatchList[i].FirstOrDefault() != default(AggregatedTripData))
                                break;

                            var vehicle = vehicles.Where(x => x.Id == distanceWatchList[i].First().VehicleId).FirstOrDefault();

                            string vehicleDesciption;

                            if (vehicle != default(VehicleItem))
                            {
                                vehicleDesciption = vehicle.Description;

                                vehicleDesciption = vehicleDesciption.Substring(0, Math.Min(vehicleDesciption.Length, 7));
                            }
                            else
                                vehicleDesciption = "N/A";

                            barData.Add(new ChartDataPoint(vehicleDesciption, distanceWatchList[i].Sum(x => x.TotalDistance)));
                        }
                        break;
                    #endregion
                    case DashboardTileType.DRIVE_DURATION:
                        #region
                        tripData.Clear();
                        titleLabel.Text = AppResources.drive_duration;

                        goalValue = Settings.Current.DriveDurationThreshold; //In hours 

                        goalLabel.Text =
                            $"{AppResources.goal}: {AppResources.below} {goalValue.ToString()} {AppResources.hours}";

                        //Line Data
                        #region LineData
                        foreach (var date in dates)
                        {

                            await throttler.WaitAsync();
                            allTasks.Add(
                                        Task.Run(async () =>
                                        {
                                            try
                                            {


                                                DateTime endDuration = (groupByPeriod == GroupByPeriod.byhour) ? date.AddHours(interval) : date.AddDays(interval);

                                                var periodTripData = await TripsAPI.GetAggregatedTripDataAsync(vehicleGroup, date, endDuration);

                                                var dailyDuration = TimeSpan.FromSeconds(periodTripData.Sum(x => x.TotalDuration)).TotalHours;

                                                lineSeriesData.Add(new ChartDataPoint(date, dailyDuration));

                                                tripData = tripData.Concat(periodTripData.Where(x => x.TotalDuration > 0)).ToList();

                                            }
                                            catch (NullReferenceException e)
                                            {
                                                Log.Debug(e.GetType().Name, e.Message);
                                            }
                                            finally
                                            {
                                                throttler.Release();
                                            }
                                        }));

                        }
                        await Task.WhenAll(allTasks);
                        #endregion
                        lineSeriesData = lineSeriesData.OrderBy(x => x.XValue).ToList();

                        var groupedDurationData = tripData.GroupBy(x => x.VehicleId);

                        //Daily Average
                        TimeSpan timespan = TimeSpan.FromSeconds(groupedDurationData.Average(group => group.Sum(y => y.TotalDuration)));
                        average = timespan.TotalHours;

                        if (average >= goalValue)
                        {
                            valueColour = Color.Red;
                            headerImage.Source = "ic_distance_duration_red";
                        }
                        else
                        {
                            valueColour = Color.Green;
                            headerImage.Source = "ic_distance_duration_green";
                        }


                        //Calculate vehicles inside and outside of goal
                        vehiclesInSideGoal = groupedDurationData.Where(group => TimeSpan.FromSeconds(group.Sum(x => x.TotalDuration)).TotalHours < goalValue).Count();
                        vehiclesOutsideGoal = groupedDurationData.Where(group => TimeSpan.FromSeconds(group.Sum(x => x.TotalDuration)).TotalHours >= goalValue).Count();

                        SetValueLabel(FormatHelper.ToShortForm(timespan), vehiclesInSideGoal.ToString(), (vehiclesInSideGoal + vehiclesOutsideGoal).ToString(), valueColour);

                        //Calculate watch list
                        var durationWatchList = groupedDurationData.OrderBy(group => group.Sum(x => x.TotalDuration)).Take(10).ToList();

                        //Set up bar data
                        count = Math.Min(durationWatchList.Count, count);
                        for (int i = 0; i < count; i++)
                        {
                            if (durationWatchList[i] == null && durationWatchList[i].FirstOrDefault() != default(AggregatedTripData))
                                break;
                            string vehicleDesciption = vehicles.Where(x => x.Id == durationWatchList[i].First().VehicleId).First().Description;
                            vehicleDesciption = vehicleDesciption.Substring(0, Math.Min(vehicleDesciption.Length, 7));

                            barData.Add(new ChartDataPoint(vehicleDesciption, TimeSpan.FromSeconds(durationWatchList[i].Sum(x => x.TotalDuration)).TotalHours));
                        }
                        break;
                    #endregion
                    case DashboardTileType.EXCEPTION_COUNTS:
                        #region
                        //Title
                        titleLabel.Text = AppResources.exception_count;

                        //Goal
                        goalValue = Settings.Current.ExceptionCountTreshold;
                        goalLabel.Text = $"{AppResources.goal}: {AppResources.below} {goalValue.ToString()} {""}";

                        List<Event> exceptionEvents = new List<Event>();

                        if (Settings.Current.PlotFleetExceptions)
                            exceptionEvents = dateFilteredEventData.Where(x => x.EventTypeId == (int)FleetException.ACCIDENT || x.EventTypeId == (int)FleetException.EXCESSIVEACCELERATION || x.EventTypeId == (int)FleetException.EXCESSIVEIDLE || x.EventTypeId == (int)FleetException.EXCESSIVERPM || x.EventTypeId == (int)FleetException.FREEWHEELING || x.EventTypeId == (int)FleetException.HARSHBREAKING || x.EventTypeId == (int)FleetException.RECKLESSDRIVING || x.EventTypeId == (int)FleetException.SPEEDING).ToList();
                        else
                            exceptionEvents = dateFilteredEventData.Where(x => x.EventTypeId == (int)DriverBehaviourException.ACCIDENT || x.EventTypeId == (int)DriverBehaviourException.ACCELERATION || x.EventTypeId == (int)DriverBehaviourException.CORNERING || x.EventTypeId == (int)DriverBehaviourException.EXCESSIVEBREAKING || x.EventTypeId == (int)DriverBehaviourException.EXCESSIVEIDLE || x.EventTypeId == (int)DriverBehaviourException.EXCESSIVERPM || x.EventTypeId == (int)DriverBehaviourException.FREEWHEELING || x.EventTypeId == (int)DriverBehaviourException.LANECHANGE || x.EventTypeId == (int)DriverBehaviourException.SPEEDING || x.EventTypeId == (int)DriverBehaviourException.SWERVING).ToList();


                        //Line Data
                        foreach (var date in dates)
                        {


                            DateTime endDuration = (groupByPeriod == GroupByPeriod.byhour) ? date.AddHours(interval) : date.AddDays(interval);
                            var dailyExceptions = exceptionEvents.Where(x => x.LocalTimestamp >= date && x.LocalTimestamp <= endDuration).Count();

                            lineSeriesData.Add(new ChartDataPoint(date, dailyExceptions));
                        }

                        //Daily Average
                        int exceptionsByVehicle = exceptionEvents.GroupBy(x => x.VehicleId).Count();

                        if (exceptionsByVehicle == 0)
                        {
                            average = 0;
                        }
                        else
                        {
                            average = exceptionEvents.Count() / exceptionsByVehicle; /* / totalPeriods*/;
                        }

                        if (average >= goalValue)
                        {
                            valueColour = Color.Red;
                            headerImage.Source = "ic_exception_counts_red";
                        }
                        else
                        {
                            valueColour = Color.Green;
                            headerImage.Source = "ic_exception_counts_green";
                        }


                        //Calculate vehicles inside and outside of goal
                        vehiclesInSideGoal = exceptionEvents.GroupBy(x => x.VehicleId).Where(g => g.Count() < goalValue).Count();
                        vehiclesOutsideGoal = exceptionEvents.GroupBy(x => x.VehicleId).Where(g => g.Count() >= goalValue).Count();

                        SetValueLabel(FormatHelper.FormatAlerts(Math.Round(average, 2).ToString()), vehiclesInSideGoal.ToString(), (vehiclesInSideGoal + vehiclesOutsideGoal).ToString(), valueColour);

                        //Calculate watch list
                        var exceptionWatchList = exceptionEvents.GroupBy(x => x.VehicleId).OrderByDescending(g => g.Count()).Take(10).ToList();

                        //Set up bar data
                        count = Math.Min(exceptionWatchList.Count, count);
                        for (int i = 0; i < count; i++)
                        {
                            if (exceptionWatchList[i] == null)
                                break;

                            string vehicleDesciption = vehicles.Where(x => x.Id == exceptionWatchList[i].FirstOrDefault().VehicleId).First().Description;
                            vehicleDesciption = vehicleDesciption.Substring(0, Math.Min(vehicleDesciption.Length, 7));

                            barData.Add(new ChartDataPoint(vehicleDesciption, exceptionWatchList[i].Count()));
                        }
                        break;
                    #endregion
                    case DashboardTileType.FUEL_CONSUMPTION:
                        #region Fuel Consumption
                        fuelEvents.Clear();
                        //Title
                        titleLabel.Text = AppResources.fuel_consumption;

                        //Goal
                        goalValue = Settings.Current.FuelConsumptionThreshold;
                        goalLabel.Text =
                            $"{AppResources.goal}: {AppResources.below} {$"{goalValue.ToString()} {AppResources.volume_litres_abbr}/100{AppResources.distance_km_abbr}"}";

                        //Load Data
                        var consumptionTasks = new List<Task>();


                        foreach (var vehicle in vehicles)
                        {
                            await throttler.WaitAsync();
                            consumptionTasks.Add(
                                        Task.Run(async () =>
                                        {
                                            try
                                            {

                                                var response = (await EventAPI.GetEventsById(vehicle.Id, 172, startDate, endDate, true));

                                                var _fuelConsumption = new List<Event>();

                                                if (response != null)
                                                    _fuelConsumption = response.Where(x => x.FuelData != null).ToList();

                                                //Fix API response
                                                foreach (Event fuelEvent in _fuelConsumption)
                                                {
                                                    fuelEvent.VehicleId = vehicle.Id;
                                                }
                                                lock (_listLock)
                                                {
                                                    fuelEvents = fuelEvents.Concat(_fuelConsumption).ToList();
                                                }


                                            }
                                            catch (NullReferenceException e)
                                            {
                                                Log.Debug(e.GetType().Name, e.Message);
                                            }
                                            finally
                                            {
                                                throttler.Release();
                                            }
                                        }));


                            await Task.WhenAll(consumptionTasks);


                        }
                        var fuelConsEvents = fuelEvents.Where(x => x.FuelData != null && x.FuelData.Reason == 2 && x.FuelData.AverageFuelConsumption != null && x.FuelData.AverageFuelConsumption.Value != 0).ToList();
                        var groupedFuelConsumption = fuelConsEvents.GroupBy(x => x.VehicleId).Select(x => new { VehicleId = x.First().VehicleId, FuelConsumption = FuelHelper.CalculateFuelConsumption(x.Sum(y => y.FuelData.AverageFuelConsumption.Value), x.Sum(y => y.FuelData.TripDistance)) });

                        double totalCosumption = 0;
                        //Line Data
                        var consumptionList = new List<double>();
                        foreach (var date in dates)
                        {


                            DateTime endDuration = (groupByPeriod == GroupByPeriod.byhour) ? date.AddHours(interval) : date.AddDays(interval);
                            var _events = fuelConsEvents.Where(x => x.LocalTimestamp >= date && x.LocalTimestamp <= endDuration);

                            var dailyConsumptionLitres = _events.Sum(x => x.FuelData.AverageFuelConsumption.Value);
                            var totalDist = _events.Sum(x => x.FuelData.TripDistance);

                            lineSeriesData.Add(new ChartDataPoint(date, FuelHelper.CalculateFuelConsumption(dailyConsumptionLitres, totalDist)));
                        }


                        //Average
                        average = groupedFuelConsumption.Sum(x => x.FuelConsumption) / groupedFuelConsumption.Count();

                        if (average >= goalValue)
                        {
                            valueColour = Color.Red;
                            headerImage.Source = "ic_fuel_consumption_red";
                        }
                        else
                        {
                            valueColour = Color.Green;
                            headerImage.Source = "ic_fuel_consumption_green";
                        }

                        //Calculate vehicles inside and outside of goal
                        vehiclesInSideGoal = groupedFuelConsumption.Where(g => g.FuelConsumption < goalValue).Count();
                        vehiclesOutsideGoal = groupedFuelConsumption.Where(g => g.FuelConsumption >= goalValue).Count();

                        SetValueLabel(FormatHelper.FormatConsumption(Math.Round(average, 2).ToString()), vehiclesInSideGoal.ToString(), (vehiclesInSideGoal + vehiclesOutsideGoal).ToString(), valueColour);

                        //Calculate watch list
                        var consumptionWatchList = groupedFuelConsumption.OrderByDescending(g => g.FuelConsumption).Take(10).ToList();

                        //Set up bar data
                        count = Math.Min(consumptionWatchList.Count, count);
                        for (int i = 0; i < count; i++)
                        {
                            if (consumptionWatchList[i] == null)
                                break;
                            string vehicleDesciption = "";
                            if (!string.IsNullOrEmpty(consumptionWatchList[i].VehicleId))
                                vehicleDesciption = vehicles.Where(x => x.Id == consumptionWatchList[i].VehicleId).First().Description;

                            vehicleDesciption = vehicleDesciption.Substring(0, Math.Min(vehicleDesciption.Length, 7));

                            barData.Add(new ChartDataPoint(vehicleDesciption, consumptionWatchList[i].FuelConsumption));
                        }
                        break;
                    #endregion
                    case DashboardTileType.FUEL_THEFT:
                        #region
                        fuelEvents.Clear();
                        //Title
                        titleLabel.Text = AppResources.fuel_theft;

                        //Goal
                        goalValue = Settings.Current.FuelTheftThreshold;
                        goalLabel.Text = $"{AppResources.goal}: {AppResources.below} {goalValue.ToString()}";

                        var fuelTheftTasks = new List<Task>();

                        foreach (var vehicle in vehicles)
                        {
                            await throttler.WaitAsync();
                            fuelTheftTasks.Add(
                                        Task.Run(async () =>
                                        {
                                            try
                                            {

                                                var response = (await EventAPI.GetEventsById(vehicle.Id, 172, startDate, endDate, true));

                                                var _fuelConsumption = new List<Event>();

                                                if (response != null)
                                                    _fuelConsumption = response.Where(x => x.FuelData != null).ToList();

                                                //Fix API response
                                                foreach (Event fuelEvent in _fuelConsumption)
                                                {
                                                    fuelEvent.VehicleId = vehicle.Id;
                                                }
                                                lock (_listLock)
                                                {
                                                    fuelEvents = fuelEvents.Concat(_fuelConsumption).ToList();
                                                }


                                            }
                                            catch (NullReferenceException e)
                                            {
                                                Log.Debug(e.GetType().Name, e.Message);
                                            }
                                            finally
                                            {
                                                throttler.Release();
                                            }
                                        }));


                            await Task.WhenAll(fuelTheftTasks);


                        }
                        var fuelTheftExceptions = fuelEvents.Where(x => x.FuelData.Reason == (int)FuelEventReason.Fuel_Theft).ToList();
                        if (fuelTheftExceptions == null || fuelTheftExceptions.Count == 0)
                        {
                            return DasboardTileResponse.NoData;
                        }
                        var groupedFuelTheftExceptions = fuelTheftExceptions.GroupBy(x => x.VehicleId);
                        //Line Data
                        foreach (var date in dates)
                        {


                            DateTime endDuration = (groupByPeriod == GroupByPeriod.byhour) ? date.AddHours(interval) : date.AddDays(interval);
                            var dailyFuelTheftExceptions = fuelTheftExceptions.Where(x => x.LocalTimestamp >= date && x.LocalTimestamp <= endDuration).Count();

                            lineSeriesData.Add(new ChartDataPoint(date, dailyFuelTheftExceptions));
                        }

                        //average
                        average = groupedFuelTheftExceptions.Sum(x => x.Count()) / groupedFuelTheftExceptions.Count();

                        if (average >= goalValue)
                        {
                            valueColour = Color.Red;
                            headerImage.Source = "ic_fuel_theft_red";
                        }
                        else
                        {
                            valueColour = Color.Green;
                            headerImage.Source = "ic_fuel_theft_green";
                        }

                        //Calculate vehicles inside and outside of goal
                        vehiclesInSideGoal = groupedFuelTheftExceptions.Where(g => g.Count() < goalValue).Count();
                        vehiclesOutsideGoal = groupedFuelTheftExceptions.Where(g => g.Count() >= goalValue).Count();

                        SetValueLabel(FormatHelper.FormatAlerts(Math.Round(average, 2).ToString()), vehiclesInSideGoal.ToString(), (vehiclesInSideGoal + vehiclesOutsideGoal).ToString(), valueColour);

                        //Calculate watch list
                        var fuelTheftWatchList = groupedFuelTheftExceptions.OrderByDescending(g => g.Count()).Take(10).ToList();

                        //Set up bar data
                        count = Math.Min(fuelTheftWatchList.Count, count);
                        for (int i = 0; i < count; i++)
                        {
                            if (fuelTheftWatchList[i] == null)
                                break;


                            string vehicleDesciption = vehicles.Where(x => x.Id == fuelTheftWatchList[i].FirstOrDefault().VehicleId).First().Description;
                            vehicleDesciption = vehicleDesciption.Substring(0, Math.Min(vehicleDesciption.Length, 7));

                            barData.Add(new ChartDataPoint(vehicleDesciption, fuelTheftWatchList[i].Count()));
                        }
                        break;
                    #endregion
                    case DashboardTileType.IDLE:
                        #region
                        titleLabel.Text = AppResources.idle_dashboard_label;
                        goalValue = Settings.Current.IdleThreshold;

                        goalLabel.Text = $"{AppResources.goal}: {AppResources.below} {goalValue.ToString()} {"min"}";

                        if (eventData == null)
                            return DasboardTileResponse.NoData;

                        List<Event> idleData;

                        if (Settings.Current.PlotFleetExceptions)
                            idleData = eventData.Where(x => (FleetException)x.EventTypeId == FleetException.EXCESSIVEIDLE && x.LocalTimestamp >= startDate && x.LocalTimestamp <= endDate).ToList();
                        else
                            idleData = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.EXCESSIVEIDLE && x.LocalTimestamp >= startDate && x.LocalTimestamp <= endDate).ToList();

                        if (idleData == null || idleData.Count == 0)
                            return DasboardTileResponse.NoData;
                        var totalIdleTime = idleData.Where(x => x.ExcessiveIdleData != null).Sum(x => x.ExcessiveIdleData.Duration);

                        //Convert to minutes
                        totalIdleTime = totalIdleTime / 60;

                        average = totalIdleTime / totalPeriods;

                        if (average >= goalValue)
                        {
                            valueColour = Color.Red;
                            headerImage.Source = "ic_idle_time_red";
                        }
                        else
                        {
                            valueColour = Color.Green;
                            headerImage.Source = "ic_idle_time_green";
                        }

                        var groupedIdleData = idleData.Where(x => x.ExcessiveIdleData != null).GroupBy(x => x.VehicleId).Select(g => new { VehicleId = g.First().VehicleId, UnitId = g.First().UnitId, IdleDuration = g.Sum(v => v.ExcessiveIdleData.Duration) / 60 });

                        vehiclesInSideGoal = groupedIdleData.Where(x => x.IdleDuration < goalValue).Count();
                        vehiclesOutsideGoal = groupedIdleData.Where(x => x.IdleDuration >= goalValue).Count();

                        SetValueLabel(FormatHelper.FormatTime(Math.Round(average, 2).ToString()), vehiclesInSideGoal.ToString(), (vehiclesInSideGoal + vehiclesOutsideGoal).ToString(), valueColour);

                        //Calculate watch list
                        var idleWatchList = groupedIdleData.OrderByDescending(x => x.IdleDuration).Take(10).ToList();

                        //Line Data

                        foreach (var date in dates)
                        {



                            DateTime endDuration = (groupByPeriod == GroupByPeriod.byhour) ? date.AddHours(interval) : date.AddDays(interval);

                            var periodData = idleData.Where(x => x.LocalTimestamp >= date && x.LocalTimestamp <= endDuration && x.ExcessiveIdleData != null);

                            int dailyIdle = 0;

                            if (periodData != null && periodData.Count() > 0)
                                dailyIdle = periodData.Sum(x => x.ExcessiveIdleData.Duration / 60);

                            lineSeriesData.Add(new ChartDataPoint(date, dailyIdle));


                        }



                        lineSeriesData = lineSeriesData.OrderBy(x => x.XValue).ToList();

                        //Set up bar data
                        count = Math.Min(idleWatchList.Count, count);
                        for (int i = 0; i < count; i++)
                        {
                            if (idleWatchList[i] == null)
                                break;

                            string vehicleDesciption = vehicles.Where(x => x.Id == idleWatchList[i].VehicleId).First().Description;
                            vehicleDesciption = vehicleDesciption.Substring(0, Math.Min(vehicleDesciption.Length, 7));

                            barData.Add(new ChartDataPoint(vehicleDesciption, idleWatchList[i].IdleDuration));
                        }
                        break;
                    #endregion
                    case DashboardTileType.NON_REPORTING:
                        #region
                        //Title
                        titleLabel.Text = AppResources.non_reporting;

                        //Goal
                        goalValue = Settings.Current.NonReportingThreshold;
                        goalLabel.Text =
                            $"{AppResources.goal}: {AppResources.below} {goalValue.ToString()} {AppResources.hours}";

                        int nonReportingTotal = 0;

                        var now = DateTimeOffset.Now;

                        var nonReportingList = vehicles.GroupBy(x => x.Id).Select(group => new NonReportingVehicle { VehicleId = group.First().Id, VehicleDescription = group.First().Description, NonReportingCount = 0, LastEvent = default(Event) }).ToList();

                        foreach (var vehicle in nonReportingList)
                        {
                            vehicle.LastEvent = eventData.Where(x => x.VehicleId == vehicle.VehicleId).OrderByDescending(x => x.LocalTimestamp).FirstOrDefault();

                            if (vehicle.LastEvent == default(Event))
                            {
                                vehicle.LastEvent = eventData.Last();

                            }
                        }
                        //Line Data
                        foreach (var date in dates)
                        {
                            var filteredEventData = eventData.Where(x => x.LocalTimestamp <= date).OrderByDescending(x => x.LocalTimestamp);

                            int nonReportingVehicles = 0;
                            int reportingVehicles = 0;
                            double nonReportingHours = 0;

                            var groupedData = filteredEventData.GroupBy(x => x.VehicleId);

                            foreach (var vehicle in groupedData)
                            {
                                var lastEvent = vehicle.FirstOrDefault();

                                if (lastEvent == default(Event) || lastEvent.LocalTimestamp <= date.AddHours(-1 * goalValue))
                                {
                                    nonReportingVehicles += 1;
                                    nonReportingList.Where(x => x.VehicleId == lastEvent.VehicleId).First().NonReportingCount += 1;

                                    if (lastEvent == default(Event))
                                        nonReportingHours += goalValue;
                                    else
                                        nonReportingHours += (now - lastEvent.LocalTimestamp).TotalHours;
                                }
                                else
                                {
                                    reportingVehicles += 1;

                                    nonReportingHours += (now - lastEvent.LocalTimestamp).TotalHours;
                                }
                            }


                            lineSeriesData.Add(new ChartDataPoint(date, nonReportingHours / vehicles.Count()));

                            nonReportingTotal += nonReportingVehicles;
                        }

                        //Daily Average
                        average = nonReportingList.Average(x => (now - x.LastEvent.LocalTimestamp).TotalHours);


                        if (average >= goalValue)
                        {
                            valueColour = Color.Red;
                            headerImage.Source = "ic_non_reporting_red";
                        }
                        else
                        {
                            valueColour = Color.Green;
                            headerImage.Source = "ic_non_reporting_green";
                        }


                        //Calculate vehicles inside and outside of goal
                        vehiclesOutsideGoal = nonReportingList.Where(x => (now - x.LastEvent.LocalTimestamp).TotalHours >= goalValue).Count();
                        vehiclesInSideGoal = nonReportingList.Where(x => (now - x.LastEvent.LocalTimestamp).TotalHours < goalValue).Count();


                        SetValueLabel(FormatHelper.ToShortForm(TimeSpan.FromHours(average)), vehiclesInSideGoal.ToString(), (vehiclesInSideGoal + vehiclesOutsideGoal).ToString(), valueColour);

                        //Calculate watch list
                        var nonReportingWatchList = nonReportingList.OrderByDescending(g => g.NonReportingCount).ToList();

                        //Set up bar data
                        count = Math.Min(nonReportingWatchList.Count, count);
                        for (int ni = 0; ni < count; ni++)
                        {
                            if (nonReportingWatchList[ni] == null)
                                break;

                            string vehicleDesciption = nonReportingWatchList[ni].VehicleDescription;
                            vehicleDesciption = vehicleDesciption.Substring(0, Math.Min(vehicleDesciption.Length, 7));

                            barData.Add(new ChartDataPoint(vehicleDesciption, (now - nonReportingWatchList[ni].LastEvent.LocalTimestamp).TotalHours));
                        }
                        break;
                    #endregion
                    case DashboardTileType.STOPPED_TIME:
                        #region
                        //Load Data
                        var stoppedTimeTasks = new List<Task>();
                        var stoppedTimeVehicles = await VehicleAPI.GetVehicleInGroupAsync(vehicleGroup);
                        var stoppedTimeTripDataWithStats = new List<Trip>();
                        List<AggregatedTripData> tripList = new List<AggregatedTripData>();

                        foreach (var vehicle in stoppedTimeVehicles)
                        {
                            await throttler.WaitAsync();
                            stoppedTimeTasks.Add(
                                        Task.Run(async () =>
                                        {
                                            try
                                            {

                                                var _tripData = await TripsAPI.GetTripsWithStats(vehicle.Id.ToString(), startDate, endDate);

                                                if (_tripData != null)
                                                {
                                                    lock (_listLock)
                                                    {
                                                        stoppedTimeTripDataWithStats = stoppedTimeTripDataWithStats.Concat(_tripData).ToList();
                                                    }
                                                }

                                            }
                                            catch (NullReferenceException e)
                                            {
                                                Log.Debug(e.GetType().Name, e.Message);
                                            }
                                            finally
                                            {
                                                throttler.Release();
                                            }
                                        }));
                        }

                        await Task.WhenAll(stoppedTimeTasks);


                        for (int i = 0; i < stoppedTimeTripDataWithStats.Count; i++)
                        {
                            if (i < stoppedTimeTripDataWithStats.Count - 1)
                            {
                                var current = stoppedTimeTripDataWithStats[i];
                                var prev = stoppedTimeTripDataWithStats[i + 1];

                                if (stoppedTimeTripDataWithStats[i].VehicleId == stoppedTimeTripDataWithStats[i + 1].VehicleId)
                                    stoppedTimeTripDataWithStats[i].StoppedDuration = current.StartLocalTimestamp - prev.EndLocalTimestamp;
                            }
                            else
                            {
                                stoppedTimeTripDataWithStats[i].StopDurationAsString = "---";
                            }

                        }
                        //Title
                        titleLabel.Text = AppResources.stopped_time_title;

                        //Goal
                        goalValue = Settings.Current.StoppedTimeThreshold;
                        goalLabel.Text =
                            $"{AppResources.goal}: {AppResources.below} {goalValue.ToString()} {AppResources.hours}";

                        double totalStoppedTime = 0;
                        //Line Data
                        foreach (var date in dates)
                        {


                            DateTime endDuration = (groupByPeriod == GroupByPeriod.byhour) ? date.AddHours(interval) : date.AddDays(interval);

                            var stoppedTime = stoppedTimeTripDataWithStats.Where(x => x.StartLocalTimestamp >= date && x.StartLocalTimestamp <= endDuration).Sum(x => x.StoppedDuration.TotalHours);
                            totalStoppedTime += stoppedTime;

                            lineSeriesData.Add(new ChartDataPoint(date, stoppedTime));

                        }
                        var groupedStoppedTimeData = stoppedTimeTripDataWithStats.GroupBy(x => x.VehicleId);
                        //Average per vehicle
                        TimeSpan stoppedTimeAverage = TimeSpan.FromSeconds(groupedStoppedTimeData.Average(group => group.Sum(y => y.StoppedDuration.TotalSeconds)));
                        average = stoppedTimeAverage.TotalHours;

                        if (average >= goalValue)
                        {
                            valueColour = Color.Red;
                            headerImage.Source = "ic_stopped_time_red";
                        }
                        else
                        {
                            valueColour = Color.Green;
                            headerImage.Source = "ic_stopped_time_green";
                        }

                        //Calculate vehicles inside and outside of goal
                        vehiclesOutsideGoal = groupedStoppedTimeData.Where(g => g.Sum(x => x.StoppedDuration.TotalHours) >= goalValue).Count();
                        vehiclesInSideGoal = groupedStoppedTimeData.Where(g => g.Sum(x => x.StoppedDuration.TotalHours) < goalValue).Count();


                        SetValueLabel(FormatHelper.ToShortForm(TimeSpan.FromHours(average)), vehiclesInSideGoal.ToString(), (vehiclesInSideGoal + vehiclesOutsideGoal).ToString(), valueColour);


                        //Calculate watch list
                        var stoppTimeWatchList = groupedStoppedTimeData.OrderByDescending(g => g.Sum(x => x.StoppedDuration.TotalMinutes)).Take(10).ToList();

                        //Set up bar data
                        count = Math.Min(stoppTimeWatchList.Count, count);
                        for (int ni = 0; ni < count; ni++)
                        {
                            if (stoppTimeWatchList[ni] == null)
                                break;


                            string vehicleDesciption = vehicles.Where(x => x.Id == stoppTimeWatchList[ni].FirstOrDefault().VehicleId).First().Description;
                            vehicleDesciption = vehicleDesciption.Substring(0, Math.Min(vehicleDesciption.Length, 7));

                            barData.Add(new ChartDataPoint(vehicleDesciption, stoppTimeWatchList[ni].Sum(x => x.StoppedDuration.TotalHours)));
                        }
                        break;
                    #endregion
                    case DashboardTileType.VEHICLE_DTC:
                        #region
                        //Title
                        titleLabel.Text = AppResources.vehicle_dtc_dashboard;

                        //Goal
                        goalValue = Settings.Current.VehicleDTCCountThreshold;
                        goalLabel.Text = $"{AppResources.goal}: {AppResources.below} {goalValue.ToString()}";


                        var allDTCEvents = new List<DTCEvent>();

                        var dtcTasks = new List<Task>();
                        foreach (VehicleItem vehicle in vehicles)
                        {
                            await throttler.WaitAsync();
                            dtcTasks.Add(
                                        Task.Run(async () =>
                                        {
                                            try
                                            {
                                                var eventHistory = await DTCEventAPI.GetDTCHistoryForVehicle(vehicle.Id, startDate, endDate);

                                                //Fix for vehicle ID = null
                                                foreach (var dtc in eventHistory)
                                                {
                                                    dtc.VehicleId = vehicle.Id;
                                                }

                                                lock (_listLock)
                                                {
                                                    allDTCEvents = allDTCEvents.Concat(eventHistory).ToList();
                                                }
                                            }
                                            catch (NullReferenceException e)
                                            {
                                                Serilog.Log.Debug(e.Message);
                                            }
                                            finally
                                            {
                                                throttler.Release();
                                            }
                                        }));

                        }


                        await Task.WhenAll(dtcTasks);


                        var groupedDtcData = allDTCEvents.GroupBy(x => x.VehicleId).Select(group =>
                           new
                           {
                               Name = group.Key,
                               VehicleId = group.First().VehicleId,
                               Events = group.OrderByDescending(x => x.LocalTimestamp)
                           });

                        //Line Data
                        foreach (var date in dates)
                        {


                            DateTime endDuration = (groupByPeriod == GroupByPeriod.byhour) ? date.AddHours(interval) : date.AddDays(interval);
                            var dailyDTC = allDTCEvents.Where(x => x.LocalTimestamp >= date && x.LocalTimestamp <= endDuration).Count();
                            lineSeriesData.Add(new ChartDataPoint(date, dailyDTC));
                        }


                        //Daily Average
                        if (groupedDtcData.Count() > 0)
                            average = groupedDtcData.Sum(group => group.Events.Count()) / groupedDtcData.Count();
                        else
                            average = 0;

                        if (average >= goalValue)
                        {
                            valueColour = Color.Red;
                            headerImage.Source = "ic_vehicle_dtc_red";
                        }
                        else
                        {
                            valueColour = Color.Green;
                            headerImage.Source = "ic_vehicle_dtc_green";
                        }

                        //Calculate vehicles inside and outside of goal
                        vehiclesOutsideGoal = groupedDtcData.Where(g => g.Events.Count() >= goalValue).Count();
                        vehiclesInSideGoal = groupedDtcData.Where(g => g.Events.Count() < goalValue).Count();


                        SetValueLabel(average.ToString(), vehiclesInSideGoal.ToString(), (vehiclesInSideGoal + vehiclesOutsideGoal).ToString(), valueColour);

                        //Calculate watch list
                        var dtcWatchList = groupedDtcData.OrderByDescending(g => g.Events.Count()).Take(10).ToList();

                        //Set up bar data
                        count = Math.Min(dtcWatchList.Count, count);
                        for (int ni = 0; ni < count; ni++)
                        {
                            if (dtcWatchList[ni] == null)
                                break;

                            string vehicleDesciption = vehicles.Where(x => x.Id == dtcWatchList[ni].VehicleId).First().Description;
                            vehicleDesciption = vehicleDesciption.Substring(0, Math.Min(vehicleDesciption.Length, 7));

                            barData.Add(new ChartDataPoint(vehicleDesciption, dtcWatchList[ni].Events.Count()));
                        }
                        break;
                        #endregion
                }

                //subTitleLabel.Text = AppResources.daily_avg;
                if (vehiclesOutsideGoal == 0 && vehiclesInSideGoal == 0)
                {
                    complianceLabel.Text = "100";
                    complianceStack.BackgroundColor = Color.FromHex("#4CAF50");
                }
                else
                {
                    var complianceValue = (vehiclesInSideGoal * 100 / (vehiclesOutsideGoal + vehiclesInSideGoal));
                    complianceLabel.Text = complianceValue.ToString();

                    if (complianceValue < 50)
                        complianceStack.BackgroundColor = Color.FromHex("#f44336");
                    else
                    {
                        if (complianceValue < 75)
                            complianceStack.BackgroundColor = Color.FromHex("#FF9800");
                        else
                            complianceStack.BackgroundColor = Color.FromHex("#4CAF50");
                    }
                }

                SetUpCharts();
                return DasboardTileResponse.Success;
                //CacheTile();

            }
            catch (DivideByZeroException zeroException)
            {
                Serilog.Log.Debug(zeroException.Message);
                return DasboardTileResponse.Error;
            }
            catch (NullReferenceException nullRefence)
            {
                Serilog.Log.Debug(nullRefence.Message);
                return DasboardTileResponse.Error;
            }
            catch (Exception e)
            {
                Serilog.Log.Debug(e.Message);
                return DasboardTileResponse.Error;
            }
        }

        //private void CacheTile()
        //{
        //    BlobCache.LocalMachine.InsertObject<DashboardTile>(tileType.ToString() + reportDateRange.ToString() + vehicleGroup.ToString(), this, TimeSpan.FromDays(1));
        //}

        private async Task<List<Event>> GetEventsById(DateTime startDate, DateTime endDate, List<VehicleItem> vehicles, int eventId)
        {
            List<Event> events = new List<Event>();
            List<Task> _eventTasks = new List<Task>();
            foreach (var vehicle in vehicles)
            {
                await throttler.WaitAsync();
                _eventTasks.Add(
                            Task.Run(async () =>
                            {
                                try
                                {

                                    var response = (await EventAPI.GetEventsById(vehicle.Id, eventId, startDate, endDate, true));

                                    var _events = new List<Event>();

                                    if (response != null)
                                        _events = response.ToList();

                                    //Fix API response
                                    foreach (Event _event in _events)
                                    {
                                        _event.VehicleId = vehicle.Id;
                                    }
                                    lock (_listLock)
                                    {
                                        events = events.Concat(_events).ToList();
                                    }


                                }
                                catch (NullReferenceException e)
                                {
                                    Serilog.Log.Debug(e.Message);
                                }
                                finally
                                {
                                    throttler.Release();
                                }
                            }));
            }

            await Task.WhenAll(_eventTasks);

            return events;
        }


        private void SetValueLabel(string avg, string compliant, string total, Color colour)
        {
            var fs = new FormattedString();

            fs.Spans.Add(new Span { Text = $"{AppResources.average_short}: ", ForegroundColor = Color.Black });
            fs.Spans.Add(new Span { Text = $"{avg} ", ForegroundColor = colour });
            fs.Spans.Add(new Span { Text = $"({compliant}/{total})", ForegroundColor = Color.Black });

            valueLabel.FormattedText = fs;
        }
        private void SetValueLabel(FormattedString avg, string compliant, string total, Color colour)
        {
            var fs = new FormattedString();

            fs.Spans.Add(new Span { Text = $"{AppResources.average_short}: ", ForegroundColor = Color.Black });
            foreach (Span span in avg.Spans)
            {
                span.ForegroundColor = colour;
                fs.Spans.Add(span);
            }
            fs.Spans.Add(new Span { Text = $" ({compliant}/{total})", ForegroundColor = Color.Black });

            valueLabel.FormattedText = fs;
        }

        private void SetUpCharts()
        {
            //pieChart.Series.Clear();
            barGraph.Series.Clear();
            lineGraph.Series.Clear();



            //var pieSeries = new DoughnutSeries { ItemsSource = pieData, CircularCoefficient = 1 };
            //pieSeries.ColorModel.Palette = ChartColorPalette.Custom;
            //pieSeries.ColorModel.CustomBrushes = colors;
            //pieChart.Series.Add(pieSeries);

            //Bar Graph

            CategoryAxis xAxis = new CategoryAxis { };
            barGraph.PrimaryAxis = xAxis;

            NumericalAxis yAxis = new NumericalAxis();
            barGraph.SecondaryAxis = yAxis;

            BarSeries series = new BarSeries();
            series.EnableTooltip = true;
            series.ItemsSource = barData;

            List<Color> colors = new List<Color>();
            foreach (var item in barData)
            {
                if (!goalIsAbove)
                {
                    if (item.YValue < goalValue)
                        colors.Add(Color.Green);
                    else
                        colors.Add(Color.Red);
                }
                else
                {
                    if (item.YValue > goalValue)
                        colors.Add(Color.Green);
                    else
                        colors.Add(Color.Red);
                }
            }
            series.ColorModel.Palette = ChartColorPalette.Custom;
            series.ColorModel.CustomBrushes = colors;

            barGraph.Series.Add(series);

            //Line graph
            lineXAxis = new DateTimeAxis { Minimum = startDate, Maximum = endDate };
            lineXAxis.LabelStyle.LabelFormat = "dd/MMM";
            if (reportDateRange == ReportDateRange.TODAY)
            {
                lineXAxis.Maximum = DateTime.Now;
                lineXAxis.LabelStyle.LabelFormat = "HH:mm";
            }
            lineGraph.PrimaryAxis = lineXAxis;

            NumericalAxis lineYAxis = new NumericalAxis();
            lineGraph.SecondaryAxis = lineYAxis;

            NumericalStripLine goalStripLine = new NumericalStripLine()
            {

                Start = 0,
                Width = goalValue,
                FillColor = Palette.MainAccent.MultiplyAlpha(0.3)

            };
            lineYAxis.StripLines.Add(goalStripLine);
            if (lineSeriesData != null && lineSeriesData.Count > 0)
                lineYAxis.Maximum = Math.Max(lineSeriesData.Max(x => x.YValue) * 1.1, goalValue * 1.25);
            else
                lineYAxis.Maximum = goalValue * 1.25;

            LineSeries lineSeries = new LineSeries();
            lineSeries.ItemsSource = lineSeriesData;


            lineGraph.Series.Add(lineSeries);
        }

        private void SetUpUI()
        {
            /*mainStack = new StackLayout { InputTransparent = true, HorizontalOptions = LayoutOptions.FillAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };

            //Set header
            headerImage = new CachedImage();
            titleLabel = new Label { FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)) };
            subTitleLabel = new Label { FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)), VerticalOptions = LayoutOptions.End };
            valueLabel = new Label();
            goalLabel = new Label { FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)), VerticalOptions = LayoutOptions.End };
            graphCalloutButton = new CachedImage { HorizontalOptions = LayoutOptions.EndAndExpand };

            header = new StackLayout
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Horizontal,
                Children =
                    {
                        headerImage,
                        new StackLayout {
                            HorizontalOptions = LayoutOptions.StartAndExpand,
                            Children =
                            {
                                new StackLayout { Children = { titleLabel, subTitleLabel }, Orientation = StackOrientation.Horizontal, BackgroundColor = Palette.MainAccent200 },
                                new StackLayout { Children = { valueLabel, goalLabel }, Orientation = StackOrientation.Horizontal }
                            }
                        },
                        graphCalloutButton
                    }
            };
            //Set gauge

            //Set horizontal chart

            //Set line graph

            mainStack.Children.Add(header);

            Content = mainStack;*/
        }

        private class NonReportingVehicle
        {
            public string VehicleId { get; set; }
            public string VehicleDescription { get; set; }
            public int NonReportingCount { get; set; }
            public Event LastEvent { get; set; }
        }
    }

    public enum DashboardTileType
    {
        IDLE = 0,
        DISTANCE = 1,
        NON_REPORTING = 2,
        STOPPED_TIME = 3,
        DRIVE_DURATION = 4,
        EXCEPTION_COUNTS = 5,
        FUEL_CONSUMPTION = 6,
        ACCIDENT_COUNT = 7,
        FUEL_THEFT = 8,
        VEHICLE_DTC = 9
    }

    public enum DasboardTileResponse
    {
        Error = 0,
        Success = 2,
        NoData = 3
    }

}
