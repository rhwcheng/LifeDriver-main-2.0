using System;
using StyexFleetManagement.Helpers;
using StyexFleetManagement.Models;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
	public partial class VehicleSummaryDetailPage : ContentPage
	{
		private DrivingSummary drivingSummaryData;

		VehicleGroupCollection vehicleGroups;

		string selectedVehicleGroup;
		ReportDateRange selectedDate;

		string distanceUnit;
        private TimeUtilization utilizationData;
        private OvertimeUtilization overtimeUtilizationData;
        private int selectedIndex1;
        private int selectedIndex2;

        public VehicleSummaryDetailPage(DrivingSummary data, int datePickerIndex, int vehiclePickerIndex)
		{
			InitializeComponent();

			distanceUnit = DistanceMeasurementUnit.GetAbbreviation(Settings.Current.DistanceMeasurementUnit);

			BackgroundColor = App.LightGray;

			drivingSummaryData = data;
			vehicleGroups = App.VehicleGroups;

			//Populate Vehicle Picker
			foreach (VehicleGroup vehicleGroup in vehicleGroups.VehicleGroups)
			{
				VehicleGroupPicker.Items.Add(vehicleGroup.Description);
			}

			datePicker.SelectedIndex = datePickerIndex;
			VehicleGroupPicker.SelectedIndex = vehiclePickerIndex;

			selectedVehicleGroup = vehicleGroups.FindIdFromDescription(this.VehicleGroupPicker.Items[this.VehicleGroupPicker.SelectedIndex]);
			selectedDate = (ReportDateRange)datePicker.SelectedIndex;

			SetUpPickers();

			//Set up labels according to initial passed data
			SetUpSummaryGrid();
			SetUpTopFiveDistanceGrid();
			SetUpTopFiveTimeGrid();


		}

        public VehicleSummaryDetailPage(TimeUtilization utilizationData, DrivingSummary drivingSummaryData, OvertimeUtilization overtimeUtilizationData, int datePickerIndex, int vehiclePickerIndex)
        {
            InitializeComponent();
            this.utilizationData = utilizationData;
            this.drivingSummaryData = drivingSummaryData;
            this.overtimeUtilizationData = overtimeUtilizationData;

            distanceUnit = DistanceMeasurementUnit.GetAbbreviation(Settings.Current.DistanceMeasurementUnit);

            BackgroundColor = App.LightGray;
            
            vehicleGroups = App.VehicleGroups;

            //Populate Vehicle Picker
            foreach (VehicleGroup vehicleGroup in vehicleGroups.VehicleGroups)
            {
                VehicleGroupPicker.Items.Add(vehicleGroup.Description);
            }

            datePicker.SelectedIndex = datePickerIndex;
            VehicleGroupPicker.SelectedIndex = vehiclePickerIndex;

            selectedVehicleGroup = vehicleGroups.FindIdFromDescription(this.VehicleGroupPicker.Items[this.VehicleGroupPicker.SelectedIndex]);
            selectedDate = (ReportDateRange)datePicker.SelectedIndex;

            SetUpPickers();

            //Set up labels according to initial passed data
            SetUpSummaryGrid();
            SetUpTopFiveDistanceGrid();
            SetUpTopFiveTimeGrid();

            gestureFrame.SwipeLeft += async (s, e) =>
            {
                var page = new VehicleUtilizationDetailPage(utilizationData, drivingSummaryData, overtimeUtilizationData, datePicker.SelectedIndex, VehicleGroupPicker.SelectedIndex);

                Navigation.InsertPageBefore(page, this);

                await Navigation.PopAsync();


            };
        }

        void SetUpSummaryGrid()
		{
			if (drivingSummaryData != null)
			{
				int vehicleCount = drivingSummaryData.VehicleCount;

				TotalVehicleCountLabel.Text = string.Format(AppResources.x_vehicles, vehicleCount);

				double averageSpeed = 0;

				//Calculate average speed
				if (drivingSummaryData.TotalDrivingTime > 0)
				{
					averageSpeed = drivingSummaryData.TotalDistance / (drivingSummaryData.TotalDrivingTime / 3600f);
				}
				else
				{
					averageSpeed = 0;
				}

				TotalDistance.FormattedText = FormatHelper.FormatDistance(Math.Round(drivingSummaryData.TotalDistance, 1, MidpointRounding.AwayFromZero).ToString());
				AverageDistance.FormattedText = FormatHelper.FormatDistance(Math.Round(drivingSummaryData.TotalDistance/vehicleCount, 1, MidpointRounding.AwayFromZero).ToString());

				TotalTime.FormattedText = DateHelper.FormatTime(drivingSummaryData.TotalDrivingTime);
				AverageTime.FormattedText = DateHelper.FormatTime(drivingSummaryData.TotalDrivingTime/vehicleCount);

				AverageSpeed.FormattedText = FormatHelper.FormatSpeed(Math.Round(averageSpeed, 1, MidpointRounding.AwayFromZero).ToString());
			}
		}

		void SetUpTopFiveDistanceGrid()
		{
			int position = 1;

			var s = DateHelper.GetDateRangeStartDate(App.SelectedDate);
			var e = DateHelper.GetDateRangeEndDate(App.SelectedDate);

			var numberOfDays = DateHelper.GetNumberOfDays(s, e);

			bool averageIsVisible;

			/*if (selectedDate == ReportDateRange.LAST_SEVEN_DAYS || selectedDate == ReportDateRange.YESTERDAY)
			{
				averageIsVisible = false;
			}
			else {*/
				averageIsVisible = true;
			//}

			dailyAverageHeader.IsVisible = averageIsVisible;
			FirstDistanceAvgLabel.IsVisible = averageIsVisible;
			SecondDistanceAvgLabel.IsVisible = averageIsVisible;
			ThirdDistanceAvgLabel.IsVisible = averageIsVisible;
			FourthDistanceAvgLabel.IsVisible = averageIsVisible;
			FifthDistanceAvgLabel.IsVisible = averageIsVisible;



			foreach (TopDrivingDistanceVehicle vehicle in drivingSummaryData.TopDrivingDistanceVehicles)
			{
				switch (position)
				{
					case ((int)RowLabel.First):

						FirstDistanceVehicleLabel.Text = vehicle.Description;
						FirstDistanceTimeLabel.FormattedText = FormatHelper.FormatDistance(Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero).ToString());
						FirstDistanceAvgLabel.FormattedText = FormatHelper.FormatDistance(Math.Round(vehicle.Distance/numberOfDays, 1, MidpointRounding.AwayFromZero).ToString());
						position++;
						break;
					case ((int)RowLabel.Second):
						SecondDistanceVehicleLabel.Text = vehicle.Description;
						SecondDistanceTimeLabel.FormattedText = FormatHelper.FormatDistance(Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero).ToString());
						SecondDistanceAvgLabel.FormattedText = FormatHelper.FormatDistance(Math.Round(vehicle.Distance / numberOfDays, 1, MidpointRounding.AwayFromZero).ToString());
						position++;
						break;
					case ((int)RowLabel.Third):
						ThirdDistanceVehicleLabel.Text = vehicle.Description;
						ThirdDistanceTimeLabel.FormattedText = FormatHelper.FormatDistance(Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero).ToString());
						ThirdDistanceAvgLabel.FormattedText = FormatHelper.FormatDistance(Math.Round(vehicle.Distance / numberOfDays, 1, MidpointRounding.AwayFromZero).ToString());
						position++;
						break;
					case ((int)RowLabel.Fourth):
						FourthDistanceVehicleLabel.Text = vehicle.Description;
						FourthDistanceTimeLabel.FormattedText = FormatHelper.FormatDistance(Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero).ToString());
						FourthDistanceAvgLabel.FormattedText = FormatHelper.FormatDistance(Math.Round(vehicle.Distance / numberOfDays, 1, MidpointRounding.AwayFromZero).ToString());
						position++;
						break;
					case ((int)RowLabel.Fifth):
						FifthDistanceVehicleLabel.Text = vehicle.Description;
						FifthDistanceTimeLabel.FormattedText = FormatHelper.FormatDistance(Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero).ToString());
						FifthDistanceAvgLabel.FormattedText = FormatHelper.FormatDistance(Math.Round(vehicle.Distance / numberOfDays, 1, MidpointRounding.AwayFromZero).ToString());
						position++;
						break;

				}
			}


		}

		void SetUpTopFiveTimeGrid()
		{
			var s = DateHelper.GetDateRangeStartDate(App.SelectedDate);
			var e = DateHelper.GetDateRangeEndDate(App.SelectedDate);

			var numberOfDays = DateHelper.GetNumberOfDays(s, e);

			bool averageIsVisible;

			
			averageIsVisible = true;
			

			timeDailyAverageHeader.IsVisible = averageIsVisible;
			FirstTimeAvgLabel.IsVisible = averageIsVisible;
			SecondTimeAvgLabel.IsVisible = averageIsVisible;
			ThirdTimeAvgLabel.IsVisible = averageIsVisible;
			FourthTimeAvgLabel.IsVisible = averageIsVisible;
			FifthTimeAvgLabel.IsVisible = averageIsVisible;
			
			int position = 1;
			foreach (TopDrivingTimeVehicle vehicle in drivingSummaryData.TopDrivingTimeVehicles)
			{
				switch (position)
				{
					case ((int)RowLabel.First):

						FirstTimeVehicleLabel.Text = vehicle.Description;
						FirstTimeTimeLabel.FormattedText = DateHelper.FormatTime(vehicle.DrivingTime);
						FirstTimeAvgLabel.FormattedText = DateHelper.FormatTime(vehicle.DrivingTime/numberOfDays);
						position++;
						break;
					case ((int)RowLabel.Second):
						SecondTimeVehicleLabel.Text = vehicle.Description;
						SecondTimeTimeLabel.FormattedText = DateHelper.FormatTime(vehicle.DrivingTime);
						SecondTimeAvgLabel.FormattedText = DateHelper.FormatTime(vehicle.DrivingTime / numberOfDays);
						position++;
						break;
					case ((int)RowLabel.Third):
						ThirdTimeVehicleLabel.Text = vehicle.Description;
						ThirdTimeTimeLabel.FormattedText = DateHelper.FormatTime(vehicle.DrivingTime);
						ThirdTimeAvgLabel.FormattedText = DateHelper.FormatTime(vehicle.DrivingTime / numberOfDays);
						position++;
						break;
					case ((int)RowLabel.Fourth):
						FourthTimeVehicleLabel.Text = vehicle.Description;
						FourthTimeTimeLabel.FormattedText = DateHelper.FormatTime(vehicle.DrivingTime);
						FourthTimeAvgLabel.FormattedText = DateHelper.FormatTime(vehicle.DrivingTime / numberOfDays);
						position++;
						break;
					case ((int)RowLabel.Fifth):
						FifthTimeVehicleLabel.Text = vehicle.Description;
						FifthTimeTimeLabel.FormattedText = DateHelper.FormatTime(vehicle.DrivingTime);
						FifthTimeAvgLabel.FormattedText = DateHelper.FormatTime(vehicle.DrivingTime / numberOfDays);
						position++;
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

						GetDrivingDataAsync();
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

					GetDrivingDataAsync();

				}
			};



		}


		async void GetDrivingDataAsync()
		{
			App.ShowLoading(true);

			drivingSummaryData = await RestService.GetDrivingSummaryAsync(selectedVehicleGroup, DateHelper.GetDateRangeStartDate(selectedDate).ToString(Constants.API_DATE_FORMAT), DateHelper.GetDateRangeEndDate(selectedDate).ToString(Constants.API_DATE_FORMAT));
			SetUpSummaryGrid();
			SetUpTopFiveDistanceGrid();
			SetUpTopFiveTimeGrid();

			App.ShowLoading(false);
		}
}
}

