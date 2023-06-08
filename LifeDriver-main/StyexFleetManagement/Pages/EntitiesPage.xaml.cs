using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
	public partial class EntitiesPage
    {
        private ReportTile driverRouteTile;
        private ReportTile driverStatsTile;
        private ReportTile poiActivtyTile;
        private ReportTile perVariableDisplayTile;
        private ReportTile vehicleScoreTile;

        public EntitiesPage()
		{
			InitializeComponent();

            this.Title = AppResources.report_title_entities;

            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });

            //Create tiles
            driverRouteTile = new ReportTile(ReportType.DRIVER_ROUTE_ACTIVITY, true);
            driverStatsTile = new ReportTile(ReportType.DLT_DRIVER_STATS);
            poiActivtyTile = new ReportTile(ReportType.POI_ACTIVITY, true);
            perVariableDisplayTile = new ReportTile(ReportType.PER_VARIABLE_DISPLAY, true);
            vehicleScoreTile = new ReportTile(ReportType.VEHICLE_SCORE_RATING,true);
           

            SetUpGrid();
		}

        private void SetUpGrid()
        {

            if (Device.Idiom == TargetIdiom.Phone)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                //set frame cols and rows
                MainGrid.Children.Add(driverRouteTile, 0, 0);
                MainGrid.Children.Add(driverStatsTile, 0, 1);
                MainGrid.Children.Add(poiActivtyTile, 0, 2);
                MainGrid.Children.Add(perVariableDisplayTile, 0, 3);
                MainGrid.Children.Add(vehicleScoreTile, 0, 4);
                
            }
            else
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                //set frame cols and rows
                MainGrid.Children.Add(driverRouteTile, 0, 0);
                MainGrid.Children.Add(driverStatsTile, 1, 0);
                MainGrid.Children.Add(poiActivtyTile, 2, 0);
                MainGrid.Children.Add(perVariableDisplayTile, 0, 1);
                MainGrid.Children.Add(vehicleScoreTile, 1, 1);
                
            }
        }
    }
}
