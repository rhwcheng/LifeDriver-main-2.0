using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using StyexFleetManagement.Resx;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Data_Models;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Services;

namespace StyexFleetManagement
{
	public partial class VehiclesPage : ContentPage
	{
		VehicleGroupCollection vehicleGroups;
		ChartDataModel utilizationdataModel;

		SfChart overtimeChart;
		SfChart chart;

		string selectedVehicleGroup;
		ReportDateRange selectedDate;

		TimeUtilization utilizationData;
		DrivingSummary drivingSummaryData;
		OvertimeUtilization overtimeUtilizationData;

        private ReportTile exceptionSummaryTile;
        private ReportTile routeActivityTile;
        private ReportTile fleetUtilizationTile;
        private ReportTile costOfOwnershipTile;
        private ReportTile accidentSummaryTile;
        private ReportTile customDataTile;
        private ReportTile dtcCodeTile;
        private ReportTile oBDRouteDataTile;

        public VehiclesPage ()
		{
			InitializeComponent ();

            this.Title = AppResources.report_title_fleet;

            ToolbarItems.Add(new ToolbarItem
			{
				Icon = "ic_action_settings.png",
				Order = ToolbarItemOrder.Primary,
				Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
			});

            //GetUtilizationDataAsync();


            //Create tiles
            exceptionSummaryTile = new ReportTile(ReportType.EXCEPTION_SUMMARY);
            routeActivityTile = new ReportTile(ReportType.ROUTE_ACTIVITY);
            fleetUtilizationTile = new ReportTile(ReportType.FLEET_UTILIZATION);
            costOfOwnershipTile = new ReportTile(ReportType.TOTAL_COST_OF_OWNERSHIP, true);
            accidentSummaryTile = new ReportTile(ReportType.ACCIDENT_SUMMARY, true);
            customDataTile = new ReportTile(ReportType.CUSTOM_DATA_SUMMARY, true);
            dtcCodeTile = new ReportTile(ReportType.DTC_CODE_MAINTENANCE, true);
            oBDRouteDataTile = new ReportTile(ReportType.OBD_ROUTE_DATA, true);
            

            SetUpGrid();

            /* Deprectated
			if (Device.Idiom == TargetIdiom.Phone)
			{
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

				MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width=GridLength.Star});

				//set frame cols and rows
				Grid.SetColumn(SummaryFrame, 0);
				Grid.SetColumn(OvertimeFrame, 0);
				Grid.SetColumn(PieChartFrame, 0);

				Grid.SetRow(SummaryFrame, 0);
				Grid.SetRow(PieChartFrame, 1);
				Grid.SetRow(OvertimeFrame, 2);
			}
			else
			{
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = 300 });

				MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
				MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
				MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

				//set frame cols and rows
				Grid.SetColumn(SummaryFrame, 0);
				Grid.SetColumn(PieChartFrame, 1);
				Grid.SetColumn(OvertimeFrame, 2);

				Grid.SetRow(SummaryFrame, 0);
				Grid.SetRow(PieChartFrame, 0);
				Grid.SetRow(OvertimeFrame, 0);
			}

			BackgroundColor = App.WhiteGray;

			//Set initial date selection
			SetUpPickers();

			selectedDate = App.SelectedDate;
			selectedVehicleGroup = App.SelectedVehicleGroup;

			GetUtilizationDataAsync();
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
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });


                //set frame cols and rows
                MainGrid.Children.Add(exceptionSummaryTile, 0, 0);
                MainGrid.Children.Add(routeActivityTile, 0, 1);
                MainGrid.Children.Add(fleetUtilizationTile, 0, 2);
                MainGrid.Children.Add(costOfOwnershipTile, 0, 3);
                MainGrid.Children.Add(accidentSummaryTile, 0, 4);
                MainGrid.Children.Add(customDataTile, 0, 5);
                MainGrid.Children.Add(dtcCodeTile, 0, 6);
                MainGrid.Children.Add(oBDRouteDataTile, 0, 7);

            }
            else
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                //set frame cols and rows
                MainGrid.Children.Add(exceptionSummaryTile, 0, 0);
                MainGrid.Children.Add(routeActivityTile, 1, 0);
                MainGrid.Children.Add(fleetUtilizationTile, 2, 0);
                MainGrid.Children.Add(costOfOwnershipTile, 0, 1);
                MainGrid.Children.Add(accidentSummaryTile, 1, 1);
                MainGrid.Children.Add(customDataTile, 2, 1);
                MainGrid.Children.Add(dtcCodeTile, 0, 2);
                MainGrid.Children.Add(oBDRouteDataTile, 1, 2);

            }
        }


        async void GetUtilizationDataAsync()
        {
            App.ShowLoading(true);

            var s = DateHelper.GetDateRangeStartDate(selectedDate).ToString(Constants.API_DATE_FORMAT);
            var e = DateHelper.GetDateRangeEndDate(selectedDate).ToString(Constants.API_DATE_FORMAT);

            utilizationData = await RestService.GetUtilizationDataAsync(App.SelectedVehicleGroup, s, e);
            utilizationData.TotalDrivingTime = Math.Max(utilizationData.TotalDrivingTime - utilizationData.TotalIdleDuration, 0);

            overtimeUtilizationData = await RestService.GetOvertimeUtilizationDataAsync(selectedVehicleGroup, s, e);

            drivingSummaryData = await RestService.GetDrivingSummaryAsync(selectedVehicleGroup, s, e);

            App.ShowLoading(false);
        }

        /* Deprecated 
        private void SetUpPickers()
		{
			datePicker.SelectedIndex = (int)App.SelectedDate;

			//Populate Vehicle Picker
			foreach (VehicleGroup vehicleGroup in App.VehicleGroups.VehicleGroups)
			{
				VehicleGroupPicker.Items.Add(vehicleGroup.Description);
			}


			VehicleGroupPicker.SelectedIndex = App.SelectedVehicleGroupIndex;

			this.VehicleGroupPicker.SelectedIndexChanged += (sender, args) =>
			{
				if (VehicleGroupPicker.SelectedIndex == -1)
				{
					//Do nothing

				}
				else
				{
					App.SelectedVehicleGroupIndex = VehicleGroupPicker.SelectedIndex;
					string selection = this.VehicleGroupPicker.Items[this.VehicleGroupPicker.SelectedIndex];

					if (App.VehicleGroups != null)
					{
						selectedVehicleGroup = App.VehicleGroups.FindIdFromDescription(selection);
						App.SelectedVehicleGroup = selectedVehicleGroup;

						GetUtilizationDataAsync();
					}

				}
			};

			this.datePicker.SelectedIndexChanged += (sender, args) =>
			{
				if (datePicker.SelectedIndex == -1)
				{
					//Revert to default
					selectedDate = ReportDateRange.TODAY;
					App.SelectedDate = selectedDate;

				}
				else
				{
					selectedDate = (ReportDateRange)datePicker.SelectedIndex;
					App.SelectedDate = selectedDate;

					GetUtilizationDataAsync();

				}
			};
		


		}

		protected override void OnAppearing()
		{
			if (chart == null)
			{
				InitializePieChart();
			}
			
		}

		

		private async void SetUpSummary()
		{
			drivingSummaryData = await RestService.GetDrivingSummaryAsync(selectedVehicleGroup, DateHelper.getDateRangeStartDate(selectedDate).ToString(Constants.API_DATE_FORMAT), DateHelper.getDateRangeEndDate(selectedDate).ToString(Constants.API_DATE_FORMAT));
			double averageSpeed = 0;

			if (drivingSummaryData.TotalDrivingTime > 0)
			{
				averageSpeed = drivingSummaryData.TotalDistance / (drivingSummaryData.TotalDrivingTime / 3600f);
			}
			else
			{
				averageSpeed = 0;
			}

			TotalTimeLabel.FormattedText = DateHelper.FormatTime(drivingSummaryData.TotalDrivingTime);
			TotalDistanceLabel.FormattedText = FormatHelper.FormatDistance(Math.Round(drivingSummaryData.TotalDistance, 1, MidpointRounding.AwayFromZero).ToString());
			AverageSpeedLabel.FormattedText = FormatHelper.FormatSpeed(Math.Round(averageSpeed, 1, MidpointRounding.AwayFromZero).ToString());
		}

	
		private void InitializePieChart()
		{
			//Initializing chart
			chart = new SfChart() { Title = new ChartTitle { Text = AppResources.report_title_time_profile_utilization } };
			chart.Legend = new ChartLegend();

			overtimeChart = new SfChart(){ Title = new ChartTitle { Text = AppResources.report_title_overtime_utilization } };
			overtimeChart.Legend = new ChartLegend();
			//Initializing Primary Axis
			//CategoryAxis primaryAxis = new CategoryAxis();
			//primaryAxis.Title = new ChartAxisTitle() { Text = "Vehicle" };
			//chart.PrimaryAxis = primaryAxis;

			//Initializing Secondary Axis
			//NumericalAxis secondaryAxis = new NumericalAxis();
			//secondaryAxis.Title = new ChartAxisTitle() { Text = "Utilization" };
			//chart.SecondaryAxis = secondaryAxis;

			chart.VerticalOptions = LayoutOptions.FillAndExpand;
			chart.HorizontalOptions = LayoutOptions.FillAndExpand;
			overtimeChart.VerticalOptions = LayoutOptions.FillAndExpand;
			overtimeChart.HorizontalOptions = LayoutOptions.FillAndExpand;


			PieChart.Children.Add(chart);
			OvertimeChart.Children.Add(overtimeChart);

		}

		private bool SetPieChart()
		{
			//var vehicleGroup = GetSelectedVehicleGroupId();
			//ReportDateRange selectedTime = (ReportDateRange) this.DatePicker.SelectedIndex;
			bool success = false;


			if (utilizationData != null)
			{

				utilizationdataModel = new VehicleUtilizationDataModel(utilizationData);

				chart.Series.Clear();
				chart.Series.Add(new PieSeries()
				{
					DataMarker = new ChartDataMarker { LabelContent = LabelContent.Percentage, ShowLabel = true },
					EnableSmartLabels = true,
					StartAngle = 75,
					EndAngle = 435,
					ItemsSource = utilizationdataModel.Utilization,
					ColorModel = new ChartColorModel { Palette = ChartColorPalette.Custom, CustomBrushes = new List<Color> { Color.Black, Color.Gray, Color.FromHex("#3F51B5") } }
				});
				success = true;
			}

			if (overtimeUtilizationData != null)
			{

				VehicleUtilizationDataModel overtimeUtilizationdataModel = new VehicleUtilizationDataModel(overtimeUtilizationData);

				overtimeChart.Series.Clear();
				overtimeChart.Series.Add(new PieSeries()
				{
					DataMarker = new ChartDataMarker { LabelContent = LabelContent.Percentage, ShowLabel = true },
					EnableSmartLabels = true,
					StartAngle = 75,
					EndAngle = 435,
					ItemsSource = overtimeUtilizationdataModel.Utilization,
					ColorModel = new ChartColorModel { Palette = ChartColorPalette.Custom, CustomBrushes = new List<Color> { Color.FromHex("#3F51B5"), Color.Gray } }
				});
				if (success == true)
				{
					return true;
				}
				else
				{
					return false;
				}
			}

			return false;
		}*/

        void OnUtilizationPieChartTapped(object sender, EventArgs e)
		{
			//if (utilizationData != null)
                //Navigation.PushAsync(new VehicleUtilizationDetailPage(utilizationData, drivingSummaryData, overtimeUtilizationData, daePicker.SelectedIndex, App.SelectedVehicleGroupIndex));
        }

        void OnSummaryTapped(object sender, EventArgs e)
		{
			//if (drivingSummaryData != null)
				//Navigation.PushAsync(new VehicleSummaryDetailPage(utilizationData, drivingSummaryData, overtimeUtilizationData, datePicker.SelectedIndex, App.SelectedVehicleGroupIndex));
		}

		void OnOvertimePieChartTapped(object sender, EventArgs e)
		{
			//if (overtimeUtilizationData != null)
				//Navigation.PushAsync(new VehicleOvertimeDetailPage(utilizationData, drivingSummaryData, overtimeUtilizationData, datePicker.SelectedIndex, App.SelectedVehicleGroupIndex));
		}

	}
}

