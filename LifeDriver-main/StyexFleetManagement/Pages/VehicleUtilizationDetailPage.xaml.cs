using System;
using StyexFleetManagement.Data_Models;
using StyexFleetManagement.Models;
using StyexFleetManagement.Services;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
	public partial class VehicleUtilizationDetailPage : ContentPage
	{
		TimeUtilization utilizationData;
		SfChart pieChart;
		SfChart histogram;

		ChartDataModel histogramData;
		ChartDataModel pieChartData;

		VehicleGroupCollection vehicleGroups;

		string selectedVehicleGroup;
		ReportDateRange selectedDate;
        private DrivingSummary drivingSummaryData;
        private OvertimeUtilization overtimeUtilizationData;
        private int selectedIndex1;
        private int selectedIndex2;

        public VehicleUtilizationDetailPage(TimeUtilization utilizationData, DrivingSummary drivingSummaryData, OvertimeUtilization overtimeUtilizationData, int datePickerIndex, int vehiclePickerIndex )
		{
			InitializeComponent();

        
            this.utilizationData = utilizationData;
            this.drivingSummaryData = drivingSummaryData;
            this.overtimeUtilizationData = overtimeUtilizationData;


            vehicleGroups = App.VehicleGroups;

            //Populate Vehicle Picker
            foreach (VehicleGroup vehicleGroup in vehicleGroups.VehicleGroups)
            {
                VehicleGroupPicker.Items.Add(vehicleGroup.Description);
            }

            //Set initial selection from passed values
            datePicker.SelectedIndex = datePickerIndex;
            VehicleGroupPicker.SelectedIndex = vehiclePickerIndex;

            gestureFrame.SwipeRight += async (s, e) =>
            {

                var page = (new VehicleSummaryDetailPage(utilizationData, drivingSummaryData, overtimeUtilizationData, datePicker.SelectedIndex, VehicleGroupPicker.SelectedIndex));
                Navigation.InsertPageBefore(page, this);

                await Navigation.PopAsync();
            };

            gestureFrame.SwipeLeft += async (s, e) =>
            {
                var page = (new VehicleOvertimeDetailPage(utilizationData, drivingSummaryData, overtimeUtilizationData, datePicker.SelectedIndex, VehicleGroupPicker.SelectedIndex));
                Navigation.InsertPageBefore(page, this);

                await Navigation.PopAsync();

            };

            SetUpReport();

		}
        public VehicleUtilizationDetailPage(TimeUtilization utilizationData, int datePickerIndex, int vehiclePickerIndex)
        {
            InitializeComponent();

            this.utilizationData = utilizationData;

            vehicleGroups = App.VehicleGroups;

            //Populate Vehicle Picker
            foreach (VehicleGroup vehicleGroup in vehicleGroups.VehicleGroups)
            {
                VehicleGroupPicker.Items.Add(vehicleGroup.Description);
            }

            //Set initial selection from passed values
            datePicker.SelectedIndex = datePickerIndex;
            VehicleGroupPicker.SelectedIndex = vehiclePickerIndex;

            SetUpReport();
        }

        private void SetUpReport()
        {

            BackgroundColor = App.LightGray;


            InitializeCharts();

            if (Device.Idiom == TargetIdiom.Phone)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                //set frame cols and rows
                Grid.SetColumn(PieChart, 0);
                Grid.SetColumn(UtilizationGrid, 0);
                Grid.SetColumn(Histogram, 0);
                Grid.SetColumn(TopUtilization, 0);
                Grid.SetColumn(LeastUtilized, 0);

                Grid.SetRow(PieChart, 0);
                Grid.SetRow(UtilizationGrid, 1);
                Grid.SetRow(Histogram, 2);
                Grid.SetRow(TopUtilization, 3);
                Grid.SetRow(LeastUtilized, 4);
            }
            else
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = 300 });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = 300 });

                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                //set frame cols and rows
                Grid.SetColumn(PieChart, 0);
                Grid.SetColumn(UtilizationGrid, 1);
                Grid.SetColumn(Histogram, 2);

                Grid.SetColumn(TopUtilization, 0);
                Grid.SetColumn(LeastUtilized, 0);

                Grid.SetColumnSpan(TopUtilization, 3);
                Grid.SetColumnSpan(LeastUtilized, 3);

                Grid.SetRow(PieChart, 0);
                Grid.SetRow(UtilizationGrid, 0);
                Grid.SetRow(Histogram, 0);
                Grid.SetRow(TopUtilization, 1);
                Grid.SetRow(LeastUtilized, 2);
            }



            //Set date and vehicle group selections
            selectedDate = (ReportDateRange)datePicker.SelectedIndex;
            if (vehicleGroups != null)
            {
                selectedVehicleGroup = vehicleGroups.FindIdFromDescription(this.VehicleGroupPicker.Items[this.VehicleGroupPicker.SelectedIndex]);

            }


            SetUpPickers();

            //set up labels, tables and graphs
            SetUpLabels();

            SetUpMostUtilized();
            SetUpLeastUtilized();

            SetUpGraphs();

        }


        void SetUpMostUtilized()
		{
			int count = 1;

			foreach (MostUtilized vehicle in utilizationData.MostUtilized)
			{
				double utilization = 0.00;
				string utilizationText;
				if (vehicle.TimeProfileTime != 0 && vehicle.DrivingTime != 0)
				{
					utilization = Math.Round((double)(vehicle.DrivingTime * 100 / vehicle.TimeProfileTime), MidpointRounding.AwayFromZero);
					utilizationText = utilization.ToString() + " %";
				}
				else
				{
					utilizationText = "-";
				}
				switch (count)
				{
					case ((int)RowLabel.First):
						DrivingFirst.FormattedText = FormatTime(vehicle.DrivingTime);
						IdleFirst.FormattedText = FormatTime(vehicle.IdleDuration);
						VehicleFirst.Text = vehicle.Description;
						WorkHoursFirst.FormattedText = FormatTime(vehicle.TimeProfileTime);
						UtilizationFirst.Text = utilizationText;
						count++;
						break;
					case ((int)RowLabel.Second):
						DrivingSecond.FormattedText = FormatTime(vehicle.DrivingTime);
						IdleSecond.FormattedText = FormatTime(vehicle.IdleDuration);
						VehicleSecond.Text = vehicle.Description;
						WorkHoursSecond.FormattedText = FormatTime(vehicle.TimeProfileTime);
						UtilizationSecond.Text = utilizationText;
						count++;
						break;
					case ((int)RowLabel.Third):
						DrivingThird.FormattedText = FormatTime(vehicle.DrivingTime);
						IdleThird.FormattedText = FormatTime(vehicle.IdleDuration);
						VehicleThird.Text = vehicle.Description;
						WorkHoursThird.FormattedText = FormatTime(vehicle.TimeProfileTime);
						UtilizationThird.Text = utilizationText;
						count++;
						break;
					case ((int)RowLabel.Fourth):
						DrivingFourth.FormattedText = FormatTime(vehicle.DrivingTime);
						IdleFourth.FormattedText = FormatTime(vehicle.IdleDuration);
						VehicleFourth.Text = vehicle.Description;
						WorkHoursFourth.FormattedText = FormatTime(vehicle.TimeProfileTime);
						UtilizationFourth.Text = utilizationText;
						count++;
						break;
					case ((int)RowLabel.Fifth):
						DrivingFifth.FormattedText = FormatTime(vehicle.DrivingTime);
						IdleFifth.FormattedText = FormatTime(vehicle.IdleDuration);
						VehicleFifth.Text = vehicle.Description;
						WorkHoursFifth.FormattedText = FormatTime(vehicle.TimeProfileTime);
						UtilizationFifth.Text = utilizationText;
						count++;
						break;	

						
				}
			}
		}

		void SetUpLeastUtilized()
		{
			int count = 1;



			foreach (LeastUtilized vehicle in utilizationData.LeastUtilized)
			{
				double utilization = 0.00;
				string utilizationText;
				if (vehicle.TimeProfileTime != 0 && vehicle.DrivingTime != 0)
				{
					utilization = Math.Round((double)(vehicle.DrivingTime * 100 / vehicle.TimeProfileTime), MidpointRounding.AwayFromZero);
					utilizationText = utilization.ToString() + " %";
				}
				else
				{
					utilizationText = "-";
				}
				switch (count)
				{
					case ((int)RowLabel.First):
						LeastDrivingFirst.FormattedText = FormatTime(vehicle.DrivingTime);
						LeastIdleFirst.FormattedText = FormatTime(vehicle.IdleDuration);
						LeastVehicleFirst.Text = vehicle.Description;
						LeastWorkHoursFirst.FormattedText = FormatTime(vehicle.TimeProfileTime);
						LeastUtilizationFirst.Text = utilizationText;
						count++;
						break;
					case ((int)RowLabel.Second):
						LeastDrivingSecond.FormattedText = FormatTime(vehicle.DrivingTime);
						LeastIdleSecond.FormattedText = FormatTime(vehicle.IdleDuration);
						LeastVehicleSecond.Text = vehicle.Description;
						LeastWorkHoursSecond.FormattedText = FormatTime(vehicle.TimeProfileTime);
						LeastUtilizationSecond.Text = utilizationText;
						count++;
						break;
					case ((int)RowLabel.Third):
						LeastDrivingThird.FormattedText = FormatTime(vehicle.DrivingTime);
						LeastIdleThird.FormattedText = FormatTime(vehicle.IdleDuration);
						LeastVehicleThird.Text = vehicle.Description;
						LeastWorkHoursThird.FormattedText = FormatTime(vehicle.TimeProfileTime);
						LeastUtilizationThird.Text = utilizationText;
						count++;
						break;
					case ((int)RowLabel.Fourth):
						LeastDrivingFourth.FormattedText = FormatTime(vehicle.DrivingTime);
						LeastIdleFourth.FormattedText = FormatTime(vehicle.IdleDuration);
						LeastVehicleFourth.Text = vehicle.Description;
						LeastWorkHoursFourth.FormattedText = FormatTime(vehicle.TimeProfileTime);
						LeastUtilizationFourth.Text = utilizationText;
						count++;
						break;
					case ((int)RowLabel.Fifth):
						LeastDrivingFifth.FormattedText = FormatTime(vehicle.DrivingTime);
						LeastIdleFifth.FormattedText = FormatTime(vehicle.IdleDuration);
						LeastVehicleFifth.Text = vehicle.Description;
						LeastWorkHoursFifth.FormattedText = FormatTime(vehicle.TimeProfileTime);
						LeastUtilizationFifth.Text = utilizationText;
						count++;
						break;


				}
			}
		}

		private void SetUpPickers()
		{
			
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


		async void GetUtilizationDataAsync()
		{
			App.ShowLoading(true);
			var s = DateHelper.GetDateRangeStartDate(selectedDate).ToString(Constants.API_DATE_FORMAT);
			var e = DateHelper.GetDateRangeEndDate(selectedDate).ToString(Constants.API_DATE_FORMAT);

			utilizationData = await RestService.GetUtilizationDataAsync(selectedVehicleGroup, s, e);
			utilizationData.TotalDrivingTime = Math.Max(utilizationData.TotalDrivingTime - utilizationData.TotalIdleDuration, 0);

			SetUpLabels();
			SetUpMostUtilized();
			SetUpLeastUtilized();
			var complete = SetUpGraphs();

			App.ShowLoading(false);
		}


		private void SetUpLabels()
		{
			TotalWork.FormattedText = FormatTime(utilizationData.TotalTimeProfileTime);
			TotalDriving.FormattedText = FormatTime(utilizationData.TotalDrivingTime);
			TotalIdle.FormattedText = FormatTime(utilizationData.TotalIdleDuration);

			var vehicleCount = utilizationData.VehicleCount;
			var averageWork = utilizationData.TotalTimeProfileTime / vehicleCount;
			var averageDriving = utilizationData.TotalDrivingTime / vehicleCount;
			var averageIdle = utilizationData.TotalIdleDuration / vehicleCount;

			AverageWork.FormattedText = FormatTime(averageWork);
			AverageDriving.FormattedText = FormatTime(averageDriving);
			AverageIdle.FormattedText = FormatTime(averageIdle);

			var timePeriod = DateTime.Parse(utilizationData.DateRangeEnd) - DateTime.Parse(utilizationData.DateRangeBegin);
			var days = (int) Math.Round(timePeriod.TotalDays, MidpointRounding.AwayFromZero);

			DailyWork.FormattedText = FormatTime(averageWork / days);
			DailyIdle.FormattedText = FormatTime(averageIdle / days);
			DailyDriving.FormattedText = FormatTime(averageDriving / days);


		}

		private bool SetUpGraphs()
		{
			if (utilizationData != null)
			{
				histogramData = new ChartDataModel(utilizationData.UtilizationHistogram);
				pieChartData = new ChartDataModel(utilizationData);



				//set up pie chart
				pieChart.Series.Clear();
				pieChart.Series.Add(new PieSeries()
				{
					DataMarker = new ChartDataMarker { LabelContent = LabelContent.Percentage, ShowLabel = true },
					EnableSmartLabels = true,
					StartAngle = 75,
					EndAngle = 435,
					ItemsSource = pieChartData.Utilization,
					Color = Color.FromHex(Constants.ACCENT_COLOUR)
				});

				//set up histogram
				histogram.Series.Clear();
				histogram.Series.Add(new ColumnSeries()
				{
					ItemsSource = histogramData.Utilization,
					Color = Color.FromHex(Constants.ACCENT_COLOUR)
				});

				return true;
			}
			return false;
		}

		private void InitializeCharts()
		{
			//Initializing chart
			pieChart = new SfChart();
			pieChart.Legend = new ChartLegend();
			histogram = new SfChart();

			histogram.Title = new ChartTitle() { Text = "Utilization Histogram" };
			//Initializing Primary Axis
			CategoryAxis primaryAxis = new CategoryAxis();
			histogram.PrimaryAxis = primaryAxis;

			//Initializing Secondary Axis
			NumericalAxis secondaryAxis = new NumericalAxis();
			secondaryAxis.Title = new ChartAxisTitle() { Text = "Number of Vehicles" };
			histogram.SecondaryAxis = secondaryAxis;

			pieChart.VerticalOptions = LayoutOptions.FillAndExpand;
			histogram.VerticalOptions = LayoutOptions.FillAndExpand;


			PieChart.Children.Add(pieChart);
			Histogram.Children.Add(histogram);

		}

		private FormattedString FormatTime(int seconds)
		{
			TimeSpan span = TimeSpan.FromSeconds(seconds);
			var fs = new FormattedString();
			var hours = (int)span.TotalHours;
			var minutes = (int)span.Minutes;
			if (hours != 0)
			{
				fs.Spans.Add(new Span { Text = hours.ToString(), FontSize = 16, FontAttributes = FontAttributes.Bold });
				fs.Spans.Add(new Span { Text = "h", FontSize = 11 });
			}
			fs.Spans.Add(new Span { Text = minutes.ToString(), FontSize = 16, FontAttributes=FontAttributes.Bold });
			fs.Spans.Add(new Span { Text = "min", FontSize = 11 });

			return fs;
		}
	}




}


