using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using StyexFleetManagement.ViewModel;
using Syncfusion.SfChart.XForms;
using Syncfusion.SfDataGrid.XForms;
using Xamarin.Forms;
using SelectionMode = Syncfusion.SfDataGrid.XForms.SelectionMode;

namespace StyexFleetManagement.Pages.Reports
{
    public partial class FuelCostPage : ReportPage
    {
        private FuelCostViewModel viewModel;
        private bool fuelConsumptionChartIsFiltered;
        private bool fuelCostChartIsFiltered;
        private SfChart fuelEfficiencyGraph;
        private SfChart fuelConsumptionGraph;
        private SfDataGrid dataGrid;
        private Picker costPicker;
        private SemaphoreSlim throttler;
        private ReportHeaderLayout ReportHeader;
        private FuelCost fuelCost;
        private double fuelValue;
        private bool orderByMostEfficient;

        public FuelCostPage()
        {
            InitializeComponent();

            orderByMostEfficient = true;

            this.Title = AppResources.report_title_fuel_cost;

            viewModel = new FuelCostViewModel();

            BindingContext = viewModel;

            SetupUI();

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
        }

        private async Task SetUpTable()
        {

            await viewModel.SetTableData(orderByMostEfficient);

            dataGrid.ItemsSource = viewModel.FuelCostInfoCollection;
            return;
        }

        private async Task GetData()
        {
            App.ShowLoading(true, isCancel: true);

            await viewModel.GetData(App.SelectedVehicleGroup);


            App.ShowLoading(false);
        }

        private void SetUpGraphs()
        {
            if (viewModel.FuelData == null || viewModel.FuelData.Count == 0)
                return;

            SetUpFuelEfficiencyGraphData();
            SetUpFuelConsumptionGraphData();
        }

        private void SetupUI()
        {
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });

            costPicker = new Picker { Title = AppResources.fuel_cost };
            costPicker.Items.Add(AppResources.mzone_cost);
            costPicker.Items.Add(AppResources.custom);
            costPicker.SelectedIndex = 0;
            costPicker.SelectedIndexChanged += CostPicker_SelectedIndexChanged;

            var pickerCollection = new Collection<View>();
            pickerCollection.Add(costPicker);

            ReportHeader = new ReportHeaderLayout(pickerCollection);
            MainStack.Children.Insert(0, ReportHeader);
            ReportHeader.DatePickerLayout.PropertyChanged += Date_PropertyChanged;

            var fuelEfficiencyTitleTapGesture = new TapGestureRecognizer();
            fuelEfficiencyTitleTapGesture.Tapped += FuelEfficiencyTapGesture_Tapped;
            fuelEfficiencyLabel.GestureRecognizers.Add(fuelEfficiencyTitleTapGesture);

            var fuelConsumptionTitleTapGesture = new TapGestureRecognizer();
            fuelConsumptionTitleTapGesture.Tapped += FuelConsumptionTapGesture_Tapped;
            fuelConsumptionLabel.GestureRecognizers.Add(fuelConsumptionTitleTapGesture);

            var tableLabelTapGesture = new TapGestureRecognizer();
            tableLabelTapGesture.Tapped += TableLabelTapGesture_Tapped;
            TableLabel.GestureRecognizers.Add(tableLabelTapGesture);

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
            fuelEfficiencyGraph = new SfChart { InputTransparent = true, HeightRequest = height, Legend = new ChartLegend { DockPosition = LegendPlacement.Bottom }, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };
            fuelConsumptionGraph = new SfChart { InputTransparent = true, HeightRequest = height, Legend = new ChartLegend { DockPosition = LegendPlacement.Bottom }, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };

            fuelEfficiencyGraph.PrimaryAxis = new DateTimeAxis { IntervalType = DateTimeIntervalType.Auto, Interval = 1, LabelStyle = new ChartAxisLabelStyle { LabelFormat = "dd/MMM" }/*, LabelRotationAngle = -45 */};
            fuelEfficiencyGraph.SecondaryAxis = new NumericalAxis();

            fuelConsumptionGraph.PrimaryAxis = new DateTimeAxis { IntervalType = DateTimeIntervalType.Auto, Interval = 1, LabelStyle = new ChartAxisLabelStyle { LabelFormat = "dd/MMM" }/*, LabelRotationAngle = -45 */};
            fuelConsumptionGraph.SecondaryAxis = new NumericalAxis();

            //Add charts to grid
            fuelEfficiencyGraphStack.Children.Add(fuelEfficiencyGraph);
            fuelConsumptionGraphStack.Children.Add(fuelConsumptionGraph);

            //Set up data grid
            dataGrid = new SfDataGrid { HorizontalOptions = LayoutOptions.FillAndExpand, ColumnSizer = ColumnSizer.Star, AutoGenerateColumns = false, GridStyle = new GridStyle(), SelectionMode = Syncfusion.SfDataGrid.XForms.SelectionMode.SingleDeselect };
            dataGrid.SelectionChanged += DataGrid_SelectionChanged;


            GridTextColumn descriptionColumn = new GridTextColumn { };
            descriptionColumn.MappingName = "VehicleDescription";
            descriptionColumn.HeaderText = AppResources.vehicle_description;

            GridTextColumn registrationIdColumn = new GridTextColumn { };
            registrationIdColumn.MappingName = "Registration";
            registrationIdColumn.HeaderText = AppResources.registration;

            GridTextColumn fuelSourceColumn = new GridTextColumn { };
            fuelSourceColumn.MappingName = "FuelSource";
            fuelSourceColumn.HeaderText = AppResources.fuel_source;

            GridTextColumn consumptionColumn = new GridTextColumn { };
            consumptionColumn.MappingName = "FuelConsumption";
            consumptionColumn.HeaderText = AppResources.fuel_consumption;

            GridTextColumn maxSpeedColumn = new GridTextColumn { };
            maxSpeedColumn.MappingName = "MaxSpeed";
            maxSpeedColumn.HeaderText = AppResources.max_speed;


            GridTextColumn distanceColumn = new GridTextColumn { };
            distanceColumn.MappingName = "Distance";
            distanceColumn.HeaderText = AppResources.distance;

            GridTextColumn durationColumn = new GridTextColumn { };
            durationColumn.MappingName = "Duration";
            durationColumn.HeaderText = AppResources.drive_duration;


            dataGrid.Columns.Add(descriptionColumn);
            dataGrid.Columns.Add(registrationIdColumn);
            dataGrid.Columns.Add(fuelSourceColumn);
            dataGrid.Columns.Add(consumptionColumn);
            dataGrid.Columns.Add(maxSpeedColumn);
            dataGrid.Columns.Add(distanceColumn);
            dataGrid.Columns.Add(durationColumn);

            TableStack.Children.Add(dataGrid);

        }

        private async void TableLabelTapGesture_Tapped(object sender, EventArgs e)
        {
            orderByMostEfficient = (!orderByMostEfficient);

            if (orderByMostEfficient)
                TableLabel.Text = AppResources.most_fuel_efficient;
            else
                TableLabel.Text = AppResources.least_fuel_efficient;

            await SetUpTable();
        }

        private void CostPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;


            if (picker.Items[picker.SelectedIndex] == AppResources.mzone_cost)
            {
                fuelCost = FuelCost.Mzone;

                SetUpGraphs();
            }
            else
            {
                var popup = new EntryPopup("Enter a value:", string.Empty, "OK", "Cancel");
                popup.PopupClosed += (o, closedArgs) =>
                {
                    if (closedArgs.Button == "OK")
                    {
                        var isNumber = double.TryParse(closedArgs.Text, out fuelValue);
                        if (!isNumber)
                            DisplayAlert("Value invalid", "Please enter a valid value", "OK");
                        else
                        {
                            fuelCost = FuelCost.Custom;
                            SetUpGraphs();

                        }
                    }
                };

                popup.Show();
            }


        }

        private void DataGrid_SelectionChanged(object sender, GridSelectionChangedEventArgs e)
        {
            var selectedItem = (FuelCostInfo)e.AddedItems.FirstOrDefault();
            var deselectedItem = (FuelCostInfo)e.RemovedItems.FirstOrDefault();

            if (selectedItem != null)
            {
                SetUpFuelEfficiencyGraphData(selectedItem.VehicleId);
                SetUpFuelConsumptionGraphData(selectedItem.VehicleId);
            }
            else
            {
                SetUpFuelEfficiencyGraphData();
                SetUpFuelConsumptionGraphData();
            }
        }

        private void SetUpFuelConsumptionGraphData(string vehicleId = default(string))
        {
            //Clear series
            fuelConsumptionGraph.Series.Clear();

            List<Event> eventData;
            if (!string.IsNullOrEmpty(vehicleId))
            {
                eventData = viewModel.FuelData.Where(x => x.VehicleId == vehicleId).ToList();
                fuelConsumptionChartIsFiltered = true;
            }
            else
            {
                fuelConsumptionChartIsFiltered = false;
                eventData = viewModel.FuelData;
            }

            if (eventData == null || eventData.Count == 0)
                return;

            var tempStartDate = viewModel.StartDate;
            var tempEndDate = viewModel.EndDate;

            var fuelConsumptionSeriesData = new ObservableCollection<ChartDataPoint>();
            var fuelCostSeriesData = new ObservableCollection<ChartDataPoint>();

            var fuelEventData = eventData.Where(x => x.FuelData != null && x.FuelData.AverageFuelConsumption != null);

            if (tempStartDate > tempEndDate)
                return;

            while (tempStartDate < tempEndDate)
            {
                var startDuration = tempStartDate;

                double totalFuelCost = 0;
                double totalFuelConsumption = fuelEventData.Where(x => x.LocalTimestamp >= startDuration && x.LocalTimestamp <= startDuration.AddDays(1)).Sum(x => FuelHelper.CalculateFuelUsed(x.FuelData.AverageFuelConsumption.Value));

                if (fuelCost == FuelCost.Custom)
                {
                    totalFuelCost = totalFuelConsumption * fuelValue;
                }
                else
                {
                    totalFuelCost = viewModel.GetTotalMonthlyFuelCost(startDuration);
                }


                fuelConsumptionSeriesData.Add(new ChartDataPoint(tempStartDate, totalFuelConsumption));
                fuelCostSeriesData.Add(new ChartDataPoint(tempStartDate, totalFuelCost));

                tempStartDate = tempStartDate.AddDays(1);
            }

            var fuelCostSeries = new ColumnSeries
            {
                Label = AppResources.fuel_cost,
                ItemsSource = fuelCostSeriesData,
                EnableTooltip = true
            };
            fuelConsumptionGraph.Series.Add(fuelCostSeries);
            var fuelConsumptionSeries = new ColumnSeries
            {
                Label = AppResources.fuel_consumption,
                ItemsSource = fuelConsumptionSeriesData,
                EnableTooltip = true
            };
            fuelConsumptionGraph.Series.Add(fuelConsumptionSeries);
        }
        private void SwitchFuelSeries()
        {

        }

        private void SetUpFuelEfficiencyGraphData(string vehicleId = default(string))
        {
            // Clear series
            fuelEfficiencyGraph.Series.Clear();

            List<Event> eventData;
            if (!string.IsNullOrEmpty(vehicleId))
            {
                eventData = viewModel.FuelData.Where(x => x.VehicleId == vehicleId).ToList();
                fuelCostChartIsFiltered = true;
            }
            else
            {
                fuelCostChartIsFiltered = false;
                eventData = viewModel.FuelData;
            }

            if (eventData == null || eventData.Count == 0)
                return;

            var tempStartDate = viewModel.StartDate;
            var tempEndDate = viewModel.EndDate;

            var fuelConsumptionSeriesData = new ObservableCollection<ChartDataPoint>();
            var fuelCostSeriesData = new ObservableCollection<ChartDataPoint>();

            var fuelEventData = eventData.Where(x => x.FuelData != null && x.FuelData.AverageFuelConsumption != null);

            if (tempStartDate > tempEndDate)
                return;

            while (tempStartDate < tempEndDate)
            {

                var startDuration = tempStartDate;

                double totalFuelCost = 0;
                double totalFuelConsumption = 0;
                double count = 0;
                var groupedFuelData = fuelEventData.Where(x => x.LocalTimestamp >= startDuration && x.LocalTimestamp <= startDuration.AddDays(1)).GroupBy(x => x.UnitId);

                foreach (var vehicleFuelData in groupedFuelData)
                {
                    if (vehicleFuelData.Count() > 0)
                    {
                        totalFuelConsumption += FuelHelper.CalculateFuelConsumption(vehicleFuelData.Sum(x => x.FuelData.AverageFuelConsumption.Value), vehicleFuelData.Sum(x => x.FuelData.TripDistance));
                        count += 1;
                    }
                }
                double averageFuelConsumption = totalFuelConsumption / count;

                double totalFuelUsed = fuelEventData.Where(x => x.LocalTimestamp >= startDuration && x.LocalTimestamp <= startDuration.AddDays(1)).Sum(x => FuelHelper.CalculateFuelUsed(x.FuelData.AverageFuelConsumption.Value));

                if (fuelCost == FuelCost.Custom)
                {
                    totalFuelCost = totalFuelUsed * fuelValue;
                }
                else
                {
                    totalFuelCost = viewModel.GetTotalMonthlyFuelCost(startDuration);
                }
                double costPerKM = totalFuelCost / fuelEventData.Where(x => x.LocalTimestamp >= startDuration && x.LocalTimestamp <= startDuration.AddDays(1)).Sum(x => x.FuelData.TripDistance / 1000);

                fuelConsumptionSeriesData.Add(new ChartDataPoint(tempStartDate, averageFuelConsumption));
                fuelCostSeriesData.Add(new ChartDataPoint(tempStartDate, costPerKM));


                tempStartDate = tempStartDate.AddDays(1);
            }

            var fuelCostSeries = new ColumnSeries
            {
                Label = AppResources.fuel_cost_per_km,
                ItemsSource = fuelCostSeriesData,
                EnableTooltip = true
            };
            fuelEfficiencyGraph.Series.Add(fuelCostSeries);
            var fuelConsumptionSeries = new ColumnSeries
            {
                Label = AppResources.average_fuel_consumption,
                ItemsSource = fuelConsumptionSeriesData,
                EnableTooltip = true
            };
            fuelEfficiencyGraph.Series.Add(fuelConsumptionSeries);
        }

        private void FuelConsumptionTapGesture_Tapped(object sender, EventArgs e)
        {
            if (fuelCostChartIsFiltered && !fuelConsumptionChartIsFiltered)
                dataGrid.SelectionMode = SelectionMode.None;
            else
                SetUpFuelConsumptionGraphData();
        }



        private void FuelEfficiencyTapGesture_Tapped(object sender, EventArgs e)
        {
            if (!fuelCostChartIsFiltered && fuelConsumptionChartIsFiltered)
                dataGrid.SelectionMode = SelectionMode.None;
            else
                SetUpFuelEfficiencyGraphData();
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

        private enum FuelCost
        {
            Mzone = 0,
            Custom = 1
        }
    }

    public class FuelCostSummaryInfo
    {
        public string VehicleId { get; set; }
        public string VehicleDescription { get; set; }
        public string Registration { get; set; }
        public string FuelSource { get; set; }
        public string Consumption { get; set; }
        public string MaxSpeed { get; set; }
        public string Distance { get; set; }
        public string Duration { get; set; }
    }
}
