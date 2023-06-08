using StyexFleetManagement.Abstractions;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using System.Collections.ObjectModel;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.AppSettings;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
    public partial class ReportsPage : ContentPage
    {
        private Collection<ReportTile> reportTiles;
        public ReportsPage()
        {
            InitializeComponent();

            this.Title = AppResources.reports_title;

            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });
            SetupReportTiles();
        }

        private void SetupReportTiles()
        {
            reportTiles = new Collection<ReportTile>();

            //Vehicle tiles
            reportTiles.Add(new ReportTile(ReportType.EXCEPTION_SUMMARY));
            reportTiles.Add(new ReportTile(ReportType.ROUTE_ACTIVITY));
            reportTiles.Add(new ReportTile(ReportType.FLEET_UTILIZATION));

            //Entities
            //reportTiles.Add(new ReportTile(ReportType.DLT_DRIVER_STATS));

            //Fuel tiles
            reportTiles.Add(new ReportTile(ReportType.FUEL_CONSUMPTION));
            reportTiles.Add(new ReportTile(ReportType.FUEL_COST));

            //TODO: coming soon
            //reportTiles.Add(new ReportTile(ReportType.ALERT_SUMMARY, true));
            //reportTiles.Add(new ReportTile(ReportType.RISK_MANAGEMENT, true));
            //reportTiles.Add(new ReportTile(ReportType.TOTAL_COST_OF_OWNERSHIP, true));
            //reportTiles.Add(new ReportTile(ReportType.ACCIDENT_SUMMARY, true));
            //reportTiles.Add(new ReportTile(ReportType.CUSTOM_DATA_SUMMARY, true));
            //reportTiles.Add(new ReportTile(ReportType.DTC_CODE_MAINTENANCE, true));
            //reportTiles.Add(new ReportTile(ReportType.OBD_ROUTE_DATA, true));
            //reportTiles.Add(new ReportTile(ReportType.FUEL_LEVEL_MONITOR, true));
            //reportTiles.Add(new ReportTile(ReportType.CONSUMPTION_MEASURES, true));
            //reportTiles.Add(new ReportTile(ReportType.FUEL_SUMMARY, true));
            //reportTiles.Add(new ReportTile(ReportType.POI_ACTIVITY, true));
            //reportTiles.Add(new ReportTile(ReportType.PER_VARIABLE_DISPLAY, true));
            //reportTiles.Add(new ReportTile(ReportType.VEHICLE_SCORE_RATING, true));
            //reportTiles.Add(new ReportTile(ReportType.DRIVER_ROUTE_ACTIVITY, true));
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Device.Idiom == TargetIdiom.Phone)
            {
                if (DependencyService.Get<IDeviceOrientationService>().GetOrientation() == DeviceOrientation.Landscape)
                    DependencyService.Get<IOrientationService>().Portrait();
            }
            SetUpGrid();

        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
        }

        private void SetUpGrid()
        {
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.Children.Clear();
            MainGrid.RowDefinitions.Clear();

            if (Device.Idiom == TargetIdiom.Phone)
            {
                if (Device.RuntimePlatform == Device.Android)
                    MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = App.ScreenWidth - 20 });
                else
                    MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                int row = 0;
                foreach (var tile in reportTiles)
                {
                    MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    //set frame cols and rows
                    MainGrid.Children.Add(tile, 0, row);
                    row += 1;
                }

            }
            else
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                int row = 0;
                int column = 0;
                foreach (var tile in reportTiles)
                {
                    if (column == 3)
                    {
                        column = 0;
                        row += 1;
                        MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                    }
                    //set frame cols and rows
                    MainGrid.Children.Add(tile, column, row);
                    column += 1;
                }
                

            }
        }
    }
}