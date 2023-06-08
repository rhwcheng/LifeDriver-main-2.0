using System;
using System.Collections.Generic;
using StyexFleetManagement.Data_Models;
using StyexFleetManagement.Models;
using StyexFleetManagement.Services;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
	public partial class VehicleOvertimeDetailPage : ContentPage
	{
		SfChart pieChart;

		private OvertimeUtilization overtimeUtilizationData;

		private VehicleGroupCollection vehicleGroups;

		private string selectedVehicleGroup;
		private ReportDateRange selectedDate;
        private TimeUtilization utilizationData;
        private DrivingSummary drivingSummaryData;
        private int selectedIndex1;
        private int selectedIndex2;

        public VehicleOvertimeDetailPage(OvertimeUtilization utilizationData, int datePickerIndex, int vehiclePickerIndex)
        {
            InitializeComponent();

            BackgroundColor = App.LightGray;

            selectedIndex1 = datePickerIndex;
            selectedIndex2 = vehiclePickerIndex;

            if (Device.Idiom == TargetIdiom.Phone)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                //set frame cols and rows
                Grid.SetColumn(PieChartView, 0);
                Grid.SetColumn(SummaryView, 0);
                Grid.SetColumn(MostUtilizedView, 0);
                Grid.SetColumn(WorstUtilizedView, 0);

                Grid.SetRow(PieChartView, 0);
                Grid.SetRow(SummaryView, 1);
                Grid.SetRow(MostUtilizedView, 2);
                Grid.SetRow(WorstUtilizedView, 3);
            }
            else
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                //set frame cols and rows
                Grid.SetColumn(SummaryView, 0);
                Grid.SetColumn(MostUtilizedView, 1);
                Grid.SetColumn(WorstUtilizedView, 1);
                Grid.SetColumn(PieChartView, 0);


                Grid.SetRow(SummaryView, 1);
                Grid.SetRow(MostUtilizedView, 0);
                Grid.SetRow(WorstUtilizedView, 1);
                Grid.SetRow(PieChartView, 0);
            }

            this.overtimeUtilizationData = utilizationData;

            vehicleGroups = App.VehicleGroups;


            //Populate Vehicle Picker
            foreach (VehicleGroup vehicleGroup in App.VehicleGroups.VehicleGroups)
            {
                VehicleGroupPicker.Items.Add(vehicleGroup.Description);
            }

            datePicker.SelectedIndex = datePickerIndex;
            VehicleGroupPicker.SelectedIndex = vehiclePickerIndex;

            selectedVehicleGroup = vehicleGroups.FindIdFromDescription(this.VehicleGroupPicker.Items[this.VehicleGroupPicker.SelectedIndex]);
            selectedDate = (ReportDateRange)datePicker.SelectedIndex;

            SetUpPickers();

            InitializeCharts();


            //Set up labels according to initial passed data
            SetUpPieChart();
            SetUpMostUtilized();
            SetUpWorstUtilized();
            SetUpOvertimeSummary();
        }

        public VehicleOvertimeDetailPage(TimeUtilization utilizationData, DrivingSummary drivingSummaryData, OvertimeUtilization overtimeUtilizationData, int datePickerIndex, int vehiclePickerIndex)
		{
			InitializeComponent();

			BackgroundColor = App.LightGray;

            selectedIndex1 = datePickerIndex;
            selectedIndex2 = vehiclePickerIndex;

            gestureFrame.SwipeRight += async (s, e) =>
            {
                var page = (new VehicleUtilizationDetailPage(utilizationData, drivingSummaryData, overtimeUtilizationData, datePicker.SelectedIndex, VehicleGroupPicker.SelectedIndex));
                Navigation.InsertPageBefore(page, this);

                await Navigation.PopAsync();
            };

			if (Device.Idiom == TargetIdiom.Phone)
			{
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

				MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

				//set frame cols and rows
				Grid.SetColumn(PieChartView, 0);
				Grid.SetColumn(SummaryView, 0);
				Grid.SetColumn(MostUtilizedView, 0);
				Grid.SetColumn(WorstUtilizedView, 0);

				Grid.SetRow(PieChartView, 0);
				Grid.SetRow(SummaryView, 1);
				Grid.SetRow(MostUtilizedView, 2);
				Grid.SetRow(WorstUtilizedView, 3);
			}
			else
			{
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

				MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
				MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

				//set frame cols and rows
				Grid.SetColumn(SummaryView, 0);
				Grid.SetColumn(MostUtilizedView, 1);
				Grid.SetColumn(WorstUtilizedView, 1);
				Grid.SetColumn(PieChartView, 0);


				Grid.SetRow(SummaryView, 1);
				Grid.SetRow(MostUtilizedView, 0);
				Grid.SetRow(WorstUtilizedView, 1);
				Grid.SetRow(PieChartView, 0);
			}

            this.utilizationData = utilizationData;
            this.drivingSummaryData = drivingSummaryData;
            this.overtimeUtilizationData = overtimeUtilizationData;

            vehicleGroups = App.VehicleGroups;


			//Populate Vehicle Picker
			foreach (VehicleGroup vehicleGroup in App.VehicleGroups.VehicleGroups)
			{
				VehicleGroupPicker.Items.Add(vehicleGroup.Description);
			}

			datePicker.SelectedIndex = datePickerIndex;
			VehicleGroupPicker.SelectedIndex = vehiclePickerIndex;

			selectedVehicleGroup = vehicleGroups.FindIdFromDescription(this.VehicleGroupPicker.Items[this.VehicleGroupPicker.SelectedIndex]);
			selectedDate = (ReportDateRange)datePicker.SelectedIndex;

			SetUpPickers();

			InitializeCharts();


			//Set up labels according to initial passed data
			SetUpPieChart();
			SetUpMostUtilized();
			SetUpWorstUtilized();
			SetUpOvertimeSummary();
		}


        void SetUpOvertimeSummary()
		{
			WorkTimeLabel.FormattedText = DateHelper.FormatTime(overtimeUtilizationData.TotalDrivingTimeInsideProfileTime);
			OvertimeLabel.FormattedText = DateHelper.FormatTime(overtimeUtilizationData.TotalDrivingTimeOutsideProfileTime);
			TotalDrivingLabel.FormattedText = DateHelper.FormatTime(overtimeUtilizationData.TotalDrivingTimeInsideProfileTime + overtimeUtilizationData.TotalDrivingTimeOutsideProfileTime);
		}

		private void InitializeCharts()
		{
			//Initializing chart
			pieChart = new SfChart();
			pieChart.Legend = new ChartLegend();
			pieChart.VerticalOptions = LayoutOptions.FillAndExpand;

			PieChartView.Children.Add(pieChart);

		}

		async void GetOvertimeDataAsync()
		{
			App.ShowLoading(true);

			overtimeUtilizationData = await RestService.GetOvertimeUtilizationDataAsync(selectedVehicleGroup, DateHelper.GetDateRangeStartDate(selectedDate).ToString(Constants.API_DATE_FORMAT), DateHelper.GetDateRangeEndDate(selectedDate).ToString(Constants.API_DATE_FORMAT));
			SetUpPieChart();
			SetUpMostUtilized();
			SetUpWorstUtilized();
			SetUpOvertimeSummary();

			App.ShowLoading(false);
		}

		private bool SetUpPieChart()
		{
			if (overtimeUtilizationData != null)
			{

				ChartDataModel overtimeUtilizationdataModel = new ChartDataModel(overtimeUtilizationData);

				pieChart.Series.Clear();
				pieChart.Series.Add(new PieSeries()
				{
					DataMarker = new ChartDataMarker { LabelContent = LabelContent.Percentage, ShowLabel = true },
					EnableSmartLabels = true,
					StartAngle = 75,
					EndAngle = 435,
					ItemsSource = overtimeUtilizationdataModel.Utilization,
					ColorModel = new ChartColorModel { Palette = ChartColorPalette.Custom, CustomBrushes = new List<Color> { Color.FromHex("#3F51B5"), Color.Gray } }
				});

				return true;
			}
			else
			{
				return false;
			}
		}

		private void SetUpMostUtilized()
		{
			int count = 1;

			foreach (BestUtilized vehicle in overtimeUtilizationData.BestUtilized)
			{
				//Process ratio string
				string ratio;
				if (vehicle.DrivingTimeInsideProfileTime == 0 || vehicle.DrivingTimeOutsideProfileTime == 0)
				{
					ratio = "-";
				}
				else
				{
					ratio = ((int)Math.Round(((double)vehicle.DrivingTimeInsideProfileTime * 100) / vehicle.DrivingTimeOutsideProfileTime, MidpointRounding.AwayFromZero)).ToString();
				}

				switch (count)
				{
					case ((int)RowLabel.First):
						DriverCountFirst.FormattedText = vehicle.NumberOfDrivers.ToString();
						RatioFirst.Text = ratio;
						VehicleFirst.Text = vehicle.Description;
						OvertimeFirst.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeOutsideProfileTime);
						WorkTimeFirst.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeInsideProfileTime);
						count++;
						break;
					case ((int)RowLabel.Second):
						DriverCountSecond.FormattedText = vehicle.NumberOfDrivers.ToString();
						RatioSecond.Text = ratio;
						VehicleSecond.Text = vehicle.Description;
						OvertimeSecond.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeOutsideProfileTime);
						WorkTimeSecond.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeInsideProfileTime);
						count++;
						break;
					case ((int)RowLabel.Third):
						DriverCountThird.FormattedText = vehicle.NumberOfDrivers.ToString();
						RatioThird.Text = ratio;
						VehicleThird.Text = vehicle.Description;
						OvertimeThird.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeOutsideProfileTime);
						WorkTimeThird.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeInsideProfileTime);
						count++;
						break;
					case ((int)RowLabel.Fourth):
						DriverCountFourth.FormattedText = vehicle.NumberOfDrivers.ToString();
						RatioFourth.Text = ratio;
						VehicleFourth.Text = vehicle.Description;
						OvertimeFourth.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeOutsideProfileTime);
						WorkTimeFourth.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeInsideProfileTime);
						count++;
						break;
					case ((int)RowLabel.Fifth):
						DriverCountFifth.FormattedText = vehicle.NumberOfDrivers.ToString();
						RatioFifth.Text = ratio;
						VehicleFifth.Text = vehicle.Description;
						OvertimeFifth.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeOutsideProfileTime);
						WorkTimeFifth.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeInsideProfileTime);
						count++;
						break;



				}
			}
		}

		void SetUpWorstUtilized()
		{
			int count = 1;



			foreach (WorstUtilized vehicle in overtimeUtilizationData.WorstUtilized)
			{
				string ratio;
				if (vehicle.DrivingTimeInsideProfileTime == 0 || vehicle.DrivingTimeOutsideProfileTime == 0)
				{
					ratio = "-";
				}
				else
				{
					ratio = ((int) Math.Round(((double) vehicle.DrivingTimeInsideProfileTime * 100) / vehicle.DrivingTimeOutsideProfileTime, MidpointRounding.AwayFromZero)).ToString();
				}

				switch (count)
				{
					case ((int)RowLabel.First):
						LeastDriverCountFirst.FormattedText = vehicle.NumberOfDrivers.ToString();
						LeastRatioFirst.Text = ratio;
						LeastVehicleFirst.Text = vehicle.Description;
						LeastOvertimeFirst.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeOutsideProfileTime);
						LeastWorkTimeFirst.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeInsideProfileTime);
						count++;
						break;
					case ((int)RowLabel.Second):
						LeastDriverCountSecond.FormattedText = vehicle.NumberOfDrivers.ToString();
						LeastRatioSecond.Text = ratio;
						LeastVehicleSecond.Text = vehicle.Description;
						LeastOvertimeSecond.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeOutsideProfileTime);
						LeastWorkTimeSecond.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeInsideProfileTime);
						count++;
						break;
					case ((int)RowLabel.Third):
						LeastDriverCountThird.FormattedText = vehicle.NumberOfDrivers.ToString();
						LeastRatioThird.Text = ratio;
						LeastVehicleThird.Text = vehicle.Description;
						LeastOvertimeThird.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeOutsideProfileTime);
						LeastWorkTimeThird.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeInsideProfileTime);
						count++;
						break;
					case ((int)RowLabel.Fourth):
						LeastDriverCountFourth.FormattedText = vehicle.NumberOfDrivers.ToString();
						LeastRatioFourth.Text = ratio;
						LeastVehicleFourth.Text = vehicle.Description;
						LeastOvertimeFourth.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeOutsideProfileTime);
						LeastWorkTimeFourth.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeInsideProfileTime);
						count++;
						break;
					case ((int)RowLabel.Fifth):
						LeastDriverCountFifth.FormattedText = vehicle.NumberOfDrivers.ToString();
						LeastRatioFifth.Text = ratio;
						LeastVehicleFifth.Text = vehicle.Description;
						LeastOvertimeFifth.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeOutsideProfileTime);
						LeastWorkTimeFifth.FormattedText = DateHelper.FormatTime(vehicle.DrivingTimeInsideProfileTime);
						count++;
						break;


				}
			}
		}

		private void SetUpPickers()
		{
			//datePicker.SelectedIndex = (int)App.SelectedDate;




			//VehicleGroupPicker.SelectedIndex = App.SelectedVehicleGroupIndex;

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

						GetOvertimeDataAsync();
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

					GetOvertimeDataAsync();

				}
			};



		}

	}
}

