using System;
using System.Threading.Tasks;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Data_Models;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.AppSettings;

namespace StyexFleetManagement
{
	public partial class FuelPage : ContentPage
	{
		FuelSummary fuelSummary;
		FuelConsumption fuelConsumption;

		ChartDataModel dataModel;

		SfChart consumptionGraph;

		string volumeUnit;
		string consumptionUnit;

        private ReportTile fuelSummaryTile;
        private ReportTile fuelEntryConsumptionTile;
        private ReportTile fuelCostTile;
        private ReportTile fuelMonitorTile;
        private ReportTile fuelConsumptionMeasuresTile;

        public FuelPage()
		{
			InitializeComponent();

			ToolbarItems.Add(new ToolbarItem
			{
				Icon = "ic_action_settings.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
			});

            //Create tiles
            fuelSummaryTile = new ReportTile(ReportType.FUEL_SUMMARY, true);
            fuelEntryConsumptionTile = new ReportTile(ReportType.FUEL_CONSUMPTION);
            fuelCostTile = new ReportTile(ReportType.FUEL_COST);
            fuelMonitorTile = new ReportTile(ReportType.FUEL_LEVEL_MONITOR, true);
            fuelConsumptionMeasuresTile = new ReportTile(ReportType.CONSUMPTION_MEASURES, true);
            
            SetUpGrid();
            /* Deprecated
			//Set unit abbreviations
			volumeUnit = FluidMeasurementUnit.GetAbbreviation(Settings.Current.FluidMeasurementUnit);
			consumptionUnit = volumeUnit + "/100" + DistanceMeasurementUnit.GetAbbreviation(Settings.Current.DistanceMeasurementUnit);

			InitializeGraph();

			SetUpPickers();

			GetFuelDataAsync();
            */


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
                MainGrid.Children.Add(fuelSummaryTile, 0, 0);
                MainGrid.Children.Add(fuelEntryConsumptionTile, 0, 1);
                MainGrid.Children.Add(fuelCostTile, 0, 2);
                MainGrid.Children.Add(fuelMonitorTile, 0, 3);
                MainGrid.Children.Add(fuelConsumptionMeasuresTile, 0, 4);

            }
            else
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                //set frame cols and rows
                MainGrid.Children.Add(fuelSummaryTile, 0, 0);
                MainGrid.Children.Add(fuelEntryConsumptionTile, 1, 0);
                MainGrid.Children.Add(fuelCostTile, 2, 0);
                MainGrid.Children.Add(fuelMonitorTile, 0, 1);
                MainGrid.Children.Add(fuelConsumptionMeasuresTile, 1, 1);

            }
        }

        /* Deprecated
        async void GetFuelDataAsync()
		{
			//App.ShowLoading(true);
			var viewModel = new BaseViewModel();
			ConsumptionIndicator.BindingContext = viewModel;
			SummaryIndicator.BindingContext = viewModel;
			fuelSummarySection.BindingContext = viewModel;
			fuelConsumptionSection.BindingContext = viewModel;
			viewModel.IsBusy = true;


			var s = DateHelper.getDateRangeStartDate(App.SelectedDate);
			var e = DateHelper.getDateRangeEndDate(App.SelectedDate);

			fuelSummary = await RestService.GetFuelSummaryDataAsync(App.SelectedVehicleGroup, s.ToString(Constants.API_DATE_FORMAT), e.ToString(Constants.API_DATE_FORMAT));


			fuelConsumption = await RestService.GetFuelConsumptionAsync(App.SelectedVehicleGroup, DateHelper.GetGroupByPeriodStartDate(App.SelectedDate, s, e).ToString(Constants.API_DATE_FORMAT), e.ToString(Constants.API_DATE_FORMAT), DateHelper.GetGroupByPeriod(App.SelectedDate).ToString());

			SetUpSummary();
			SetUpGraph();

			viewModel.IsBusy = false;
			//App.ShowLoading(false);
		}


		void SetUpSummary()
		{
			TotalVolumeLabel.FormattedText = FormatHelper.FormatVolume(Math.Round(fuelSummary.TotalVolume, 1, MidpointRounding.AwayFromZero).ToString());
			TotalCostLabel.FormattedText = FormatHelper.FormatCost(Math.Round(fuelSummary.TotalCost, 1, MidpointRounding.AwayFromZero).ToString());
			AverageConsumptionLabel.FormattedText = FormatHelper.FormatConsumption(Math.Round(fuelSummary.AverageConsumption, 1, MidpointRounding.AwayFromZero).ToString());
		}

		private void InitializeGraph()
		{
			//Initializing chart
			consumptionGraph = new SfChart() { VerticalOptions = LayoutOptions.FillAndExpand};

			//consumptionGraph.Title = new ChartTitle() { Text = "Vehicle Entry Consumption" };
			//Initializing Primary Axis
			CategoryAxis primaryAxis = new CategoryAxis();
			consumptionGraph.PrimaryAxis = primaryAxis;

			//Initializing Secondary Axis
			NumericalAxis secondaryAxis = new NumericalAxis();
			consumptionGraph.SecondaryAxis = secondaryAxis;


			ConsumptionChartContainer.Children.Add(consumptionGraph);
		}

		private bool SetUpGraph()
		{
			
			if (fuelConsumption != null)
			{

				dataModel = new VehicleUtilizationDataModel(fuelConsumption);

				//set up graph series
				consumptionGraph.Series.Clear();
				consumptionGraph.Series.Add(new ColumnSeries()
				{
					ItemsSource = dataModel.Utilization,
					Color = Color.FromHex(Constants.ACCENT_COLOUR)
				});
				return true;
			}

			return false;
		}

		private void SetUpPickers()
		{
			//Populate Vehicle Picker
			foreach (VehicleGroup vehicleGroup in App.VehicleGroups.VehicleGroups)
			{
				VehicleGroupPicker.Items.Add(vehicleGroup.Description);
			}

			datePicker.SelectedIndex = (int) App.SelectedDate;
			var s = App.SelectedVehicleGroup;

			//var e = VehicleGroupPicker.Items.IndexOf(App.SelectedVehicleGroup);
			VehicleGroupPicker.SelectedIndex = App.SelectedVehicleGroupIndex;
				
			this.VehicleGroupPicker.SelectedIndexChanged += (sender, args) =>
			{
				if (VehicleGroupPicker.SelectedIndex == -1)
				{
					App.SelectedVehicleGroup = null;
					App.SelectedVehicleGroupIndex = -1;
				}
				else
				{
					App.SelectedVehicleGroupIndex = VehicleGroupPicker.SelectedIndex;
					string selection = this.VehicleGroupPicker.Items[this.VehicleGroupPicker.SelectedIndex];

					if (App.VehicleGroups != null)
					{

						App.SelectedVehicleGroup = App.VehicleGroups.FindIdFromDescription(selection);

						GetFuelDataAsync();


					}

				}
			};

			this.datePicker.SelectedIndexChanged += (sender, args) =>
			{
				if (datePicker.SelectedIndex == -1)
				{
					App.SelectedDate = ReportDateRange.TODAY;
				}
				else
				{



					App.SelectedDate = (ReportDateRange)datePicker.SelectedIndex;

					GetFuelDataAsync();




				}
			};
		}*/
        
	}
}

