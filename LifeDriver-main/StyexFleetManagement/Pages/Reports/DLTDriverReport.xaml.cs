using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Data_Models;
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
    public partial class DLTDriverReport : ReportPage
    {
        private SfChart tLicensePieChart;
        private SfChart bLicensePieChart;
        private SfChart personalLicensePieChart;
        private DLTDriverReportViewModel viewModel;
        private DateTime startDate;
        private DateTime endDate;
        
        private ChartDataModel tDataModel, bDataModel, personalDataModel;

        private static List<string> columnNames = new List<string> { AppResources.label_driverId, AppResources.label_gender, AppResources.label_birthDate, AppResources.label_expiraryDate, AppResources.label_licenceNumber, AppResources.label_licenceType, AppResources.label_vehicles };
        private string selectedDriverId;
        private Picker driverPicker;
        private SfDataGrid dataGrid;

        public ReportHeaderLayout ReportHeader { get; private set; }

        public DLTDriverReport()
        {
            InitializeComponent();

            this.Title = AppResources.report_title_dlt_driver_stats;

            viewModel = new DLTDriverReportViewModel();

            BindingContext = viewModel;

            SetupUI();

            this.ReportHeader.DatePickerLayout.PropertyChanged += Date_PropertyChanged;
            this.ReportHeader.SearchTapped += OnSearch_Tapped;

            startDate = App.StartDateSelected;
            endDate = App.EndDateSelected;
            

            LoadData();
        }

        private void SetupUI()
        {
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });

            driverPicker = new Picker { BackgroundColor = Color.Transparent, Title = AppResources.driver_label, HorizontalOptions = LayoutOptions.FillAndExpand };
            ReportHeader = new ReportHeaderLayout(driverPicker);
            mainStack.Children.Insert(0, ReportHeader);

            driverPicker.SelectedIndexChanged += DriverPicker_SelectedIndexChanged;
            //Set overlay background colour
            bOverlay.BackgroundColor = Color.FromRgba(105, 105, 105, 0.6);
            tOverlay.BackgroundColor = Color.FromRgba(105, 105, 105, 0.6);
            personalOverlay.BackgroundColor = Color.FromRgba(105, 105, 105, 0.6);

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
                foreach (RowDefinition rowDefinition in MainGrid.RowDefinitions){
                    rowDefinition.Height = App.ScreenWidth;
                }
            }

            //Instantiate charts
            tLicensePieChart = new SfChart { InputTransparent = true, HeightRequest = height, WidthRequest = height, Legend = new ChartLegend { DockPosition = LegendPlacement.Left }, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };
            bLicensePieChart = new SfChart { InputTransparent = true, HeightRequest = height, WidthRequest = height, Legend = new ChartLegend { DockPosition = LegendPlacement.Left }, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };
            personalLicensePieChart = new SfChart { InputTransparent = true, HeightRequest = height, WidthRequest = height, Legend = new ChartLegend { DockPosition = LegendPlacement.Left,  LabelStyle = new ChartLegendLabelStyle { Font = Font.SystemFontOfSize(Device.GetNamedSize(NamedSize.Small, typeof(Label)))}}, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand};



            //Add charts to grid
            tLicenceStack.Children.Add(tLicensePieChart);
            //tLicenceLayout.LowerChild(tLicensePieChart);

            bLicenceStack.Children.Add(bLicensePieChart);
            //bLicenceLayout.LowerChild(bLicensePieChart);

            personalLicenceStack.Children.Add(personalLicensePieChart);
            //personalLicenceLayout.LowerChild(personalLicensePieChart);

            //Set up data grid
            dataGrid = new SfDataGrid { HorizontalOptions = LayoutOptions.FillAndExpand, ColumnSizer = ColumnSizer.Star, AutoGenerateColumns = false, GridStyle = new GridStyle(), SelectionMode = SelectionMode.SingleDeselect };



            GridTextColumn driverIdColumn = new GridTextColumn { };
            driverIdColumn.MappingName = "DriverId";
            driverIdColumn.HeaderText = AppResources.label_driverId;

            GridTextColumn genderColumn = new GridTextColumn { };
            genderColumn.MappingName = "GenderString";
            genderColumn.HeaderText = AppResources.label_gender;

            GridTextColumn birthDateColumn = new GridTextColumn { };
            birthDateColumn.MappingName = "BirthDateString";
            birthDateColumn.HeaderText = AppResources.label_birthDate;

            GridTextColumn expiraryColumn = new GridTextColumn { };
            expiraryColumn.MappingName = "LicenceExpiraryDateString";
            expiraryColumn.HeaderText = AppResources.label_expiraryDate;

            GridTextColumn licenceNoColumn = new GridTextColumn { };
            licenceNoColumn.MappingName = "LicenseId";
            licenceNoColumn.HeaderText = AppResources.label_licenceNumber;

            GridTextColumn licTypeColumn = new GridTextColumn { };
            licTypeColumn.MappingName = "LicenseString";
            licTypeColumn.HeaderText = AppResources.label_licenceType;

            GridTextColumn lastVehicleColumn = new GridTextColumn { };
            lastVehicleColumn.MappingName = "Vehicle";
            lastVehicleColumn.HeaderText = AppResources.label_vehicles;

            
            dataGrid.Columns.Add(driverIdColumn);
            dataGrid.Columns.Add(genderColumn);
            dataGrid.Columns.Add(birthDateColumn);
            dataGrid.Columns.Add(expiraryColumn);
            dataGrid.Columns.Add(licenceNoColumn);
            dataGrid.Columns.Add(licTypeColumn);
            dataGrid.Columns.Add(lastVehicleColumn);

            TableStack.Children.Add(dataGrid);

        }

        private void DriverPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            var picker = sender as Picker;

            if (picker.SelectedIndex == -1 || picker.SelectedIndex == 0)
            {
                TableLabel.Text = AppResources.top_5_drivers;
                selectedDriverId = string.Empty;
                SetUpTable();
            }
            else
            {
                TableLabel.Text = AppResources.label_driver_filtered;
                selectedDriverId = picker.Items[picker.SelectedIndex];
                SetUpTable();
            }
        }

        private void SetUpPicker()
        {
            driverPicker.Items.Clear();
            driverPicker.Items.Add(AppResources.all_label);
            var drivers = viewModel.DriverIds;
            if (drivers != null && drivers.Count > 0)
            {
                foreach (string driverId in drivers)
                {
                    if (!string.IsNullOrEmpty(driverId))
                        driverPicker.Items.Add(driverId);
                }
            }
            
        }

        private async Task LoadData()
        {
            App.ShowLoading(true);
            await GetDriverEventData();
            SetUpPicker();
            SetUpGraphs();
            SetUpTable();
            App.ShowLoading(false);
            
        }

        private void SetUpTable()
        {
            List<DriverData> driverList;
            List<string> dataList = new List<string>();
            if (string.IsNullOrEmpty(selectedDriverId))
            {
                driverList = viewModel.TopFiveDrivers;
            }
            
            else
            {
                driverList = viewModel.EventData.Where(x => x.DriverData.DriverId == selectedDriverId).Select(x => x.DriverData).ToList();

               
            }


            //var table = new RankTable(columnNames, rows) { HorizontalOptions = LayoutOptions.CenterAndExpand, VerticalOptions = LayoutOptions.FillAndExpand };
            //TableScrollView.Content = table;
            var gridSource = new ObservableCollection<DriverData>(driverList);
            dataGrid.ItemsSource = gridSource;


        }

        private async void OnSearch_Tapped(object sender, EventArgs e)
        {
            await LoadData();
        }

        private void Date_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StartDate")
            {
                var value = (sender as DatePickerLayout).StartDate;
                if (startDate != value)
                {
                    startDate = value;
                }
                else
                    return;
            }
            if (e.PropertyName == "EndDate")
            {
                var value = (sender as DatePickerLayout).EndDate;
                if (endDate != value)
                {
                    endDate = value;
                    //await GetDriverEventData();
                    //SetUpGraphs();
                }
                else
                    return;
            }
        }

        private async Task GetDriverEventData()
        {
            viewModel.EventData.Clear();
            var vehicles = await VehicleAPI.GetVehicleInGroupAsync(App.SelectedVehicleGroup);
            var allTasks = new List<Task>();
            var throttler = new SemaphoreSlim(initialCount: 20);
            foreach (VehicleItem vehicle in vehicles)
            {
                await throttler.WaitAsync();
                allTasks.Add(
                            Task.Run(async () =>
                            {
                                try
                                {
                                    List<DriverEvent> data = await EventAPI.GetDriverEvents(vehicle.Description, startDate, endDate);
                                    if (data != null)
                                        viewModel.EventData = viewModel.EventData.Concat(data).ToList();
                                }
                                finally
                                {
                                    throttler.Release();
                                }
                            }));
                
            }
            await Task.WhenAll(allTasks);
            viewModel.DriverIds = viewModel.EventData.DistinctBy(x => x.DriverData.DriverId).Select(d => d.DriverData.DriverId).ToList();
            viewModel.TopFiveDrivers = viewModel.GetTopFiveDrivers();
            foreach (var driverEvent in viewModel.EventData)
            {
                driverEvent.DriverData.Vehicle = driverEvent.UnitId;
            }
            return;
        }

        private void SetUpGraphs()
        {
            //Clear all charts
            tLicensePieChart.Series.Clear();
            bLicensePieChart.Series.Clear();
            personalLicensePieChart.Series.Clear();

            tDataModel = new ChartDataModel(viewModel.EventData, LicenseCategory.T);
            bDataModel = new ChartDataModel(viewModel.EventData, LicenseCategory.B);
            personalDataModel = new ChartDataModel(viewModel.EventData, LicenseCategory.Personal);

            SetOverlayVisibility();
            

            PieSeries tPieSeries = new PieSeries {
                ItemsSource = tDataModel.Utilization,
                DataMarker = new ChartDataMarker { LabelContent = LabelContent.YValue, ShowLabel = true },
                EnableTooltip = true,
                EnableSmartLabels = true,
                ConnectorLineType = ConnectorLineType.Bezier,
                DataMarkerPosition = CircularSeriesDataMarkerPosition.OutsideExtended,
                StartAngle = 75,
                EndAngle = 435,
                ColorModel = new ChartColorModel { Palette = ChartColorPalette.Metro }
            };
        
            tLicensePieChart.Series.Add(tPieSeries);

            PieSeries bPieSeries = new PieSeries
            {
                ItemsSource = bDataModel.Utilization,
                DataMarker = new ChartDataMarker { LabelContent = LabelContent.YValue, ShowLabel = true },
                EnableTooltip = true,
                EnableSmartLabels = true,
                ConnectorLineType = ConnectorLineType.Bezier,
                DataMarkerPosition = CircularSeriesDataMarkerPosition.OutsideExtended,
                StartAngle = 75,
                EndAngle = 435,
                ColorModel = new ChartColorModel { Palette = ChartColorPalette.Metro }
            };
            bLicensePieChart.Series.Add(bPieSeries);

            PieSeries personalPieSeries = new PieSeries
            {
                ItemsSource = personalDataModel.Utilization,
                DataMarker = new ChartDataMarker { LabelContent = LabelContent.YValue, ShowLabel = true },
                EnableTooltip = true,
                EnableSmartLabels = true,
                ConnectorLineType = ConnectorLineType.Bezier,
                DataMarkerPosition = CircularSeriesDataMarkerPosition.OutsideExtended,
                StartAngle = 75,
                EndAngle = 435,
                ColorModel = new ChartColorModel { Palette = ChartColorPalette.Metro }
            };

            personalLicensePieChart.Series.Add(personalPieSeries);

            GraphScrollView.ScrollToAsync(bLicensePieChart, ScrollToPosition.Center, true);

        }

        private void SetOverlayVisibility()
        {
            if (tDataModel.Utilization.Where(x => x.YValue != 0).Count() > 0)
            {
                viewModel.TLicenseOverlayIsVisible = false;
                tLicensePieChart.IsVisible = true;
            }
            else
            {
                viewModel.TLicenseOverlayIsVisible = true;
                tLicensePieChart.IsVisible = false;
            }

            if (bDataModel.Utilization.Where(x => !x.YValue.Equals(0)).Count() > 0)
            {
                viewModel.BLicenseOverlayIsVisible = false;
                bLicensePieChart.IsVisible = true;
            }
            else
            {
                viewModel.BLicenseOverlayIsVisible = true;
                bLicensePieChart.IsVisible = false;
            }

            if (personalDataModel.Utilization.Where(x => !x.YValue.Equals(0)).Count() > 0)
            {
                viewModel.PersonalLicenseOverlayIsVisible = false;
                personalLicensePieChart.IsVisible = true;
            }
            else
            {
                viewModel.PersonalLicenseOverlayIsVisible = true;
                personalLicensePieChart.IsVisible = false;
            }
        }
    }
}
