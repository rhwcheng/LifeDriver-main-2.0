using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
	public partial class ActionsPage : ContentPage
	{

        public ActionsPage()
		{
			InitializeComponent();

            this.Title = AppResources.report_title_actions;

            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });

            //Create tiles
            //locationTile = new ReportTile(ReportType.LOCATION_MONITOR);

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
                //MainGrid.Children.Add(locationTile, 0, 0);

            }
            else
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                //set frame cols and rows
                //MainGrid.Children.Add(locationTile, 0, 0);

            }
        }
    }
}
