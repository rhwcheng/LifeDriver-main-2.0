using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Serilog;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel;
using Syncfusion.SfChart.XForms;
using Syncfusion.SfDataGrid.XForms;
using Xamarin.Forms;
using SelectionMode = Syncfusion.SfDataGrid.XForms.SelectionMode;

namespace StyexFleetManagement.Pages.Reports
{
    public partial class VehicleUtilisationPage : ReportPage
    {
        private SfDataGrid dataGrid;
        private SfChart lineGraph;
        private VehicleUtilisationViewModel viewModel;
        private bool lineGraphIsFiltered;
        private bool orderByMostExceptions;
        private static object _listLock = new object();
        private ReportHeaderLayout ReportHeader;
        private Picker distancePicker;
        private Picker durationPicker;

        private double distanceLine;
        private double durationLine;
        private FastLineSeries dottedDistanceLineSeries;
        private FastLineSeries dottedDurationLineSeries;

        public VehicleUtilisationPage() : base()
        {
            InitializeComponent();

            this.Title = AppResources.report_title_fleet_utilization;

            viewModel = new VehicleUtilisationViewModel();

            BindingContext = viewModel;


            orderByMostExceptions = true;

            SetupUI();

            this.ReportHeader.DatePickerLayout.PropertyChanged += Date_PropertyChanged;
            this.ReportHeader.SearchTapped += OnSearch_Tapped;

            viewModel.StartDate = App.StartDateSelected;
            viewModel.EndDate = App.EndDateSelected;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadData();
        }

        private async Task LoadData()
        {
            await SetUpLineGraphData();
            await SetUpTable();

        }

        private async void TableLabelTapGesture_Tapped(object sender, EventArgs e)
        {
            orderByMostExceptions = (!orderByMostExceptions);

            if (orderByMostExceptions)
                tableLabel.Text = AppResources.title_most_utilised;
            else
                tableLabel.Text = AppResources.title_least_utilised;

            await SetUpTable();
        }

        private async Task SetUpTable()
        {
            viewModel.UtilisationInfoCollection.Clear();

            if (orderByMostExceptions)
                await viewModel.SetTableData(true);
            else
                await viewModel.SetTableData(false);

            foreach (var item in viewModel.TopFiveVehicles)
            {
                viewModel.UtilisationInfoCollection.Add(item);
            }

            dataGrid.ItemsSource = viewModel.UtilisationInfoCollection;
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

            distancePicker = new Picker { Title = AppResources.distance, HorizontalOptions = LayoutOptions.FillAndExpand };
            durationPicker = new Picker { Title = AppResources.drive_duration, HorizontalOptions = LayoutOptions.FillAndExpand };
            distancePicker.Items.Add(AppResources.vehicle_group_average);
            distancePicker.Items.Add(AppResources.custom);
            durationPicker.Items.Add(AppResources.vehicle_group_average);
            durationPicker.Items.Add(AppResources.custom);
            distancePicker.SelectedIndexChanged += DistancePicker_SelectedIndexChanged;
            durationPicker.SelectedIndexChanged += DurationPicker_SelectedIndexChanged;

            var pickerCollection = new Collection<View>();
            pickerCollection.Add(distancePicker);
            pickerCollection.Add(durationPicker);

            ReportHeader = new ReportHeaderLayout(pickerCollection);
            mainStack.Children.Insert(0, ReportHeader);
            var graphTitleTapGesture = new TapGestureRecognizer();
            graphTitleTapGesture.Tapped += GraphTitleTapGesture_Tapped; ;
            graphLabel.GestureRecognizers.Add(graphTitleTapGesture);

            var tableLabelTapGesture = new TapGestureRecognizer();
            tableLabelTapGesture.Tapped += TableLabelTapGesture_Tapped;
            tableLabel.GestureRecognizers.Add(tableLabelTapGesture);


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
            lineGraph = new SfChart { InputTransparent = true, HeightRequest = height, Legend = new ChartLegend { DockPosition = LegendPlacement.Bottom }, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };

            lineGraph.PrimaryAxis = new DateTimeAxis { IntervalType = DateTimeIntervalType.Days, Interval = 1, LabelStyle = new ChartAxisLabelStyle { LabelFormat = "dd/MMM" } };
            lineGraph.SecondaryAxis = new NumericalAxis();


            dottedDistanceLineSeries = new FastLineSeries { Label = AppResources.distance_marker };
            dottedDistanceLineSeries.StrokeDashArray = new double[] { 10, 10 };
            dottedDistanceLineSeries.Color = Color.FromHex("#C15244");
            lineGraph.Series.Add(dottedDistanceLineSeries);

            dottedDurationLineSeries = new FastLineSeries { Label = AppResources.duration_marker };
            dottedDurationLineSeries.StrokeDashArray = new double[] { 10, 10 };
            dottedDurationLineSeries.Color = Color.FromHex("#90A84E");
            lineGraph.Series.Add(dottedDurationLineSeries);

            //Add charts to grid
            graphStack.Children.Add(lineGraph);

            //Set up data grid
            dataGrid = new SfDataGrid { HorizontalOptions = LayoutOptions.FillAndExpand, ColumnSizer = ColumnSizer.Star, AutoGenerateColumns = false, GridStyle = new GridStyle(), SelectionMode = SelectionMode.SingleDeselect };
            dataGrid.SelectionChanged += DataGrid_SelectionChanged;

            //Data grid columns
            GridTextColumn descriptionColumn = new GridTextColumn { };
            descriptionColumn.MappingName = "VehicleDescription";
            descriptionColumn.HeaderText = AppResources.vehicle_description;

            GridTextColumn unitIdColumn = new GridTextColumn { };
            unitIdColumn.MappingName = "UnitId";
            unitIdColumn.HeaderText = AppResources.device_id;

            GridTextColumn distanceColumn = new GridTextColumn { };
            distanceColumn.MappingName = "DistanceTotalAndAverage";
            distanceColumn.HeaderText = AppResources.distance_average_total;

            GridTextColumn durationCountColumn = new GridTextColumn { };
            durationCountColumn.MappingName = "DurationTotalAndAverage";
            durationCountColumn.HeaderText = AppResources.drive_duration;

            //GridTextColumn lastReportedColumn = new GridTextColumn { };
            //lastReportedColumn.MappingName = "LastReportedTimeAsString";
            //lastReportedColumn.HeaderText = AppResources.last_reported;

            GridTextColumn utilizationColumn = new GridTextColumn { };
            utilizationColumn.MappingName = "UtilisationPercentage";
            utilizationColumn.HeaderText = AppResources.utilisation_period_percent;

            GridImageColumn utilisationResultColumn = new GridImageColumn { };
            utilisationResultColumn.MappingName = "ResultImage";
            utilisationResultColumn.HeaderText = AppResources.result;


            dataGrid.Columns.Add(unitIdColumn);
            dataGrid.Columns.Add(descriptionColumn);
            dataGrid.Columns.Add(distanceColumn);
            dataGrid.Columns.Add(durationCountColumn);
            //dataGrid.Columns.Add(lastReportedColumn);
            dataGrid.Columns.Add(utilizationColumn);
            dataGrid.Columns.Add(utilisationResultColumn);


            TableStack.Children.Add(dataGrid);

        }

        private void DurationPicker_SelectedIndexChanged(object sender, EventArgs e)
        {

            var picker = sender as Picker;

            var oldDuration = durationLine;

            if (picker.Items[picker.SelectedIndex] == AppResources.vehicle_group_average)
            {
                durationLine = viewModel.GetAverageDuration();
            }
            else
            {
                var popup = new EntryPopup("Enter a value:", string.Empty, "OK", "Cancel");
                popup.PopupClosed += (o, closedArgs) =>
                {
                    if (closedArgs.Button == "OK")
                    {
                        var isNumber = double.TryParse(closedArgs.Text, out durationLine);
                        if (!isNumber)
                            DisplayAlert("Value invalid", "Please enter a valid value", "OK");
                        else
                        {
                            ResetLine(dottedDurationLineSeries, durationLine);
                        }
                    }
                };

                popup.Show();
            }

            if (oldDuration != durationLine)
                ResetLine(dottedDurationLineSeries, durationLine);
        }

        private void ResetLine(FastLineSeries _dottedDistanceLineSeries, double _distanceLine)
        {
            var tempStartDate = viewModel.StartDate;
            var tempEndDate = viewModel.EndDate;
            lineGraph.Series.Remove(_dottedDistanceLineSeries);

            if (lineGraph.Series.FirstOrDefault() == null)
            {
                return;
            }

            var dataPoints = new Collection<ChartDataPoint>();
            foreach (var dataPoint in (Collection<ChartDataPoint>)lineGraph.Series.First().ItemsSource)
            {
                dataPoints.Add(new ChartDataPoint(dataPoint.XValue, _distanceLine));
                dataPoints.Add(new ChartDataPoint(dataPoint.XValue, _distanceLine));
            }


            var newLineSeries = new FastLineSeries { ItemsSource = dataPoints, Label = _dottedDistanceLineSeries.Label, StrokeDashArray = _dottedDistanceLineSeries.StrokeDashArray, Color = _dottedDistanceLineSeries.Color };
            if (newLineSeries.Label == AppResources.duration_marker)
                dottedDurationLineSeries = newLineSeries;
            else
                dottedDistanceLineSeries = newLineSeries;

            lineGraph.Series.Add(newLineSeries);
        }

        private void DistancePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;

            var oldDistance = distanceLine;

            if (picker.Items[picker.SelectedIndex] == AppResources.vehicle_group_average)
            {
                distanceLine = viewModel.GetAverageDistance();
            }
            else
            {
                var popup = new EntryPopup("Enter a value:", string.Empty, "OK", "Cancel");
                popup.PopupClosed += (o, closedArgs) =>
                {
                    if (closedArgs.Button == "OK")
                    {
                        var isNumber = double.TryParse(closedArgs.Text, out distanceLine);
                        if (!isNumber)
                            DisplayAlert("Value invalid", "Please enter a valid value", "OK");
                        else
                        {
                            ResetLine(dottedDistanceLineSeries, distanceLine);
                        }
                    }
                };

                popup.Show();
            }

            if (oldDistance != distanceLine)
                ResetLine(dottedDistanceLineSeries, distanceLine);
        }

        private void DataGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            var selectedItem = (VehicleUtilisationInfo)e.AddedItems.FirstOrDefault();
            var deselectedItem = (VehicleUtilisationInfo)e.RemovedItems.FirstOrDefault();

            if (selectedItem != null)
            {
                SetUpLineGraphData(selectedItem.VehicleId);
            }
            else
            {
                SetUpLineGraphData();
            }

        }


        private async Task SetUpLineGraphData(string vehicleId = default(string))
        {
            //Clear series
            lineGraph.Series.Clear();
            viewModel.TripData.Clear();

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

            var groupByPeriod = (tempEndDate - tempStartDate).TotalDays <= 1 ? GroupByPeriod.byhour : GroupByPeriod.byday;

            var durationSeries = new ObservableCollection<ChartDataPoint>();
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

            await FetchTripData(tempStartDate, tempEndDate);

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

            foreach (var date in dates)
            {
                var endDuration = groupByPeriod == GroupByPeriod.byhour ? date.AddHours(interval) : date.AddDays(interval);

                //Distance and Duration
                double totalDistance;
                double totalDuration;

                var tripData = viewModel.CurrentPeriodTripData.Where(x => x.StartLocalTimestamp >= date && x.StartLocalTimestamp <= endDuration);

                if (!string.IsNullOrEmpty(vehicleId))
                    tripData = tripData.Where(x => x.VehicleId == vehicleId).ToList();

                if (tripData != null)
                {
                    totalDistance = tripData.Sum(x => x.Distance);
                    totalDuration = tripData.Sum(x => (x.EndLocalTimestamp - x.StartLocalTimestamp).TotalMinutes);
                }
                else
                {
                    totalDistance = 0;
                    totalDuration = 0;
                }


                distanceSeries.Add(new ChartDataPoint(date, totalDistance));
                durationSeries.Add(new ChartDataPoint(date, totalDuration));


            }

            var distanceLineSeries = new LineSeries
            {
                Label = AppResources.distance,
                ItemsSource = distanceSeries,
                EnableTooltip = true
            };
            lineGraph.Series.Add(distanceLineSeries);
            var durationLineSeries = new LineSeries
            {
                Label = AppResources.drive_duration,
                ItemsSource = durationSeries,
                EnableTooltip = true
            };
            lineGraph.Series.Add(durationLineSeries);
        }

        private async Task FetchTripData(DateTime tempStartDate, DateTime tempEndDate)
        {
            var allTasks = new List<Task>();
            var vehicles = await VehicleAPI.GetVehicleInGroupAsync(App.SelectedVehicleGroup);

            if (vehicles == null || vehicles.Count == 0)
            {
                return;
            }
            //Calculate start date of previous period
            DateTime prevStartDate;
            var currentDuration = Math.Round((tempEndDate - tempStartDate).TotalDays - 1);
            prevStartDate = tempStartDate.AddDays(-1 * currentDuration);

            var totalCount = vehicles.Count;
            var iter = 0;
            using (var loading = UserDialogs.Instance.Progress("Fetching Data", null, null, true, MaskType.Black))
            {
                foreach (var vehicle in vehicles)
                {
                    await viewModel.Throttler.WaitAsync();
                    allTasks.Add(
                    Task.Run(async () =>
                    {
                        try
                        {

                            var tripData = await TripsAPI.GetTripsWithStats(vehicle.Id.ToString(), prevStartDate, tempEndDate);

                            iter++;
                            var progress = (iter * 100) / totalCount;
                            if (progress > 0)
                                loading.PercentComplete = progress;

                            lock (_listLock)
                            {
                                if (tripData != null && tripData.Count > 0)
                                {
                                    viewModel.TripData = viewModel.TripData.Concat(tripData).ToList();
                                }
                            }


                        }
                        catch (NullReferenceException ne)
                        {
                            Log.Debug(ne.Message);
                        }
                        catch (Exception e)
                        {
                            Log.Debug(e.Message);
                        }
                        finally
                        {
                            viewModel.Throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(allTasks);
            }

            viewModel.TripData = viewModel.TripData.OrderBy(x => x.VehicleId).ThenByDescending(x => x.StartLocalTimestamp).ToList();
            viewModel.CurrentPeriodTripData = viewModel.TripData.Where(x => x.StartLocalTimestamp >= tempStartDate && x.StartLocalTimestamp <= tempEndDate).ToList();
            viewModel.PreviousPeriodTripData = viewModel.TripData.Where(x => x.StartLocalTimestamp >= prevStartDate && x.StartLocalTimestamp <= tempStartDate).ToList();
        }


        #region Event Methods
        private void GraphTitleTapGesture_Tapped(object sender, EventArgs e)
        {
            if (lineGraphIsFiltered)
                dataGrid.SelectionMode = SelectionMode.None;
            else
                SetUpLineGraphData();
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


            //Re-configure distance line
            if (distancePicker.SelectedIndex > -1 && distancePicker.Items[distancePicker.SelectedIndex] == AppResources.vehicle_group_average)
            {
                distanceLine = viewModel.GetAverageDistance();
            }

            ResetLine(dottedDistanceLineSeries, distanceLine);

            //Re-configure duration line
            if (durationPicker.SelectedIndex > -1 && durationPicker.Items[durationPicker.SelectedIndex] == AppResources.vehicle_group_average)
            {
                durationLine = viewModel.GetAverageDuration();
            }

            ResetLine(dottedDurationLineSeries, durationLine);

        }
        #endregion
    }
}
