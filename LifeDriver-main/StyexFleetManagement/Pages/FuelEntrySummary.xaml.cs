using System;
using StyexFleetManagement.Helpers;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.Reports;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
	public partial class FuelEntrySummary : ContentPage
	{
		FuelSummary fuelSummary;

		string fuelVolumeUnit;
		string fuelCostUnit;
		string fuelConsumptionUnit;


		public FuelEntrySummary(FuelSummary summaryData, int selectedDateIndex, int selectedVehicleIndex)
		{
			InitializeComponent();

            if (Device.Idiom == TargetIdiom.Phone)
			{
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

				MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

				//set frame cols and rows
				Grid.SetColumn(UtilizationGrid, 0);
				Grid.SetColumn(TopFiveFuelVolume, 0);
				Grid.SetColumn(TopFiveFuelConsumption, 0);

				Grid.SetRow(UtilizationGrid, 0);
				Grid.SetRow(TopFiveFuelVolume, 1);
				Grid.SetRow(TopFiveFuelConsumption, 2);
			}
			else
			{
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = 300 });
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = 300 });

				MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
				MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

				//set frame cols and rows
				Grid.SetColumn(UtilizationGrid, 0);
				Grid.SetColumn(TopFiveFuelVolume, 0);
				Grid.SetColumn(TopFiveFuelConsumption, 2);

				Grid.SetColumnSpan(UtilizationGrid, 2);

				Grid.SetRow(UtilizationGrid, 0);
				Grid.SetRow(TopFiveFuelVolume, 1);
				Grid.SetRow(TopFiveFuelConsumption, 1);
			}

			//Set up units
			fuelVolumeUnit = FluidMeasurementUnit.GetAbbreviation(Settings.Current.FluidMeasurementUnit);
			fuelCostUnit = Settings.Current.Currency;
			fuelConsumptionUnit = fuelVolumeUnit + "/100" + DistanceMeasurementUnit.GetAbbreviation(Settings.Current.DistanceMeasurementUnit);

			BackgroundColor = App.LightGray;

			fuelSummary = summaryData;

			SetUpSummary();
			SetUpVolumeGrid();
			SetUpConsumptionGrid();

			//Populate Vehicle Picker
			foreach (VehicleGroup vehicleGroup in App.VehicleGroups.VehicleGroups)
			{
				VehicleGroupPicker.Items.Add(vehicleGroup.Description);
			}

			datePicker.SelectedIndex = selectedDateIndex;
			VehicleGroupPicker.SelectedIndex = selectedVehicleIndex;

			SetUpPickers();
		}

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            gestureFrame.SwipeLeft += async (s, e) =>
            {
                var page = (new FuelEntryConsumption());
                Navigation.InsertPageBefore(page, this);

                await Navigation.PopAsync();
            };

            this.Title = AppResources.report_title_fuel_summary;

            App.ShowLoading(true);
            if (fuelSummary == null)
            {
                var s = DateHelper.GetDateRangeStartDate(App.SelectedDate);
                var e = DateHelper.GetDateRangeEndDate(App.SelectedDate);

                fuelSummary = await RestService.GetFuelSummaryDataAsync(App.SelectedVehicleGroup, s.ToString(Constants.API_DATE_FORMAT), e.ToString(Constants.API_DATE_FORMAT));

            }


            SetUpSummary();
            SetUpVolumeGrid();
            SetUpConsumptionGrid();

            App.ShowLoading(false);
        }
        public FuelEntrySummary(FuelSummary summaryData = null)
		{
			InitializeComponent();

            
          
			if (Device.Idiom == TargetIdiom.Phone)
			{
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

				MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

				//set frame cols and rows
				Grid.SetColumn(UtilizationGrid, 0);
				Grid.SetColumn(TopFiveFuelVolume, 0);
				Grid.SetColumn(TopFiveFuelConsumption, 0);

				Grid.SetRow(UtilizationGrid, 0);
				Grid.SetRow(TopFiveFuelVolume, 1);
				Grid.SetRow(TopFiveFuelConsumption, 2);
			}
			else
			{
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = 300 });
				MainGrid.RowDefinitions.Add(new RowDefinition { Height = 300 });

				MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
				MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

				//set frame cols and rows
				Grid.SetColumn(UtilizationGrid, 0);
				Grid.SetColumn(TopFiveFuelVolume, 0);
				Grid.SetColumn(TopFiveFuelConsumption, 2);

				Grid.SetColumnSpan(UtilizationGrid, 2);

				Grid.SetRow(UtilizationGrid, 0);
				Grid.SetRow(TopFiveFuelVolume, 1);
				Grid.SetRow(TopFiveFuelConsumption, 1);
			}

			//Set up units
			fuelVolumeUnit = FluidMeasurementUnit.GetAbbreviation(Settings.Current.FluidMeasurementUnit);
			fuelCostUnit = Settings.Current.Currency;
			fuelConsumptionUnit = fuelVolumeUnit + "/100" + DistanceMeasurementUnit.GetAbbreviation(Settings.Current.DistanceMeasurementUnit);

			BackgroundColor = App.LightGray;


            fuelSummary = summaryData;

            //Populate Vehicle Picker
            foreach (VehicleGroup vehicleGroup in App.VehicleGroups.VehicleGroups)
			{
				VehicleGroupPicker.Items.Add(vehicleGroup.Description);
			}

			datePicker.SelectedIndex = (int)App.SelectedDate;
			VehicleGroupPicker.SelectedIndex = App.SelectedVehicleGroupIndex;

			SetUpPickers();
		}

		void SetUpSummary()
		{
			var vehicleCount = fuelSummary.VehicleCount;

			TotalVehicleCountLabel.Text = string.Format(AppResources.x_vehicles, vehicleCount);

			TotalVolume.FormattedText = FormatHelper.FormatVolume(((int)Math.Round(fuelSummary.TotalVolume, MidpointRounding.AwayFromZero)).ToString());
			TotalCost.FormattedText = FormatHelper.FormatCost(((int)Math.Round(fuelSummary.TotalCost, MidpointRounding.AwayFromZero)).ToString());

			AverageCost.FormattedText = FormatHelper.FormatCost(((int)Math.Round(fuelSummary.TotalCost/vehicleCount, MidpointRounding.AwayFromZero)).ToString());
			AverageVolume.FormattedText = FormatHelper.FormatVolume(((int)Math.Round(fuelSummary.TotalVolume / vehicleCount, MidpointRounding.AwayFromZero)).ToString());
			AverageConsumption.FormattedText = FormatHelper.FormatConsumption(((int)Math.Round(fuelSummary.AverageConsumption, MidpointRounding.AwayFromZero)).ToString());
		}

		void SetUpVolumeGrid()
		{
			int count = 1;

			foreach (TopFuelVolume vehicle in fuelSummary.TopFuelVolume)
			{
				switch (count)
				{
					case ((int)RowLabel.First):
						VehicleFirst.Text = vehicle.Description;
						VolumeFirst.FormattedText = FormatHelper.FormatVolume(((int) Math.Round(vehicle.Volume, MidpointRounding.AwayFromZero)).ToString());
						DistanceFirst.FormattedText = FormatHelper.FormatDistance((Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionFirst.FormattedText = FormatHelper.FormatConsumption(((int)Math.Round(vehicle.Consumption, MidpointRounding.AwayFromZero)).ToString());
						count++;
						break;
					case ((int)RowLabel.Second):
						VehicleSecond.Text = vehicle.Description;
						VolumeSecond.FormattedText = FormatHelper.FormatVolume(((int)Math.Round(vehicle.Volume, MidpointRounding.AwayFromZero)).ToString());
						DistanceSecond.FormattedText = FormatHelper.FormatDistance((Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionSecond.FormattedText = FormatHelper.FormatConsumption(((int)Math.Round(vehicle.Consumption, MidpointRounding.AwayFromZero)).ToString());
						count++;
						break;
					case ((int)RowLabel.Third):
						VehicleThird.Text = vehicle.Description;
						VolumeThird.FormattedText = FormatHelper.FormatVolume(((int)Math.Round(vehicle.Volume, MidpointRounding.AwayFromZero)).ToString());
						DistanceThird.FormattedText = FormatHelper.FormatDistance((Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionThird.FormattedText = FormatHelper.FormatConsumption(((int)Math.Round(vehicle.Consumption, MidpointRounding.AwayFromZero)).ToString());
						count++;
						break;
					case ((int)RowLabel.Fourth):
						VehicleFourth.Text = vehicle.Description;
						VolumeFourth.FormattedText = FormatHelper.FormatVolume(((int)Math.Round(vehicle.Volume, MidpointRounding.AwayFromZero)).ToString());
						DistanceFourth.FormattedText = FormatHelper.FormatDistance((Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionFourth.FormattedText = FormatHelper.FormatConsumption(((int)Math.Round(vehicle.Consumption, MidpointRounding.AwayFromZero)).ToString());
						count++;
						break;
					case ((int)RowLabel.Fifth):
						VehicleFifth.Text = vehicle.Description;
						VolumeFifth.FormattedText = FormatHelper.FormatVolume(((int)Math.Round(vehicle.Volume, MidpointRounding.AwayFromZero)).ToString());
						DistanceFifth.FormattedText = FormatHelper.FormatDistance((Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionFifth.FormattedText = FormatHelper.FormatConsumption(((int)Math.Round(vehicle.Consumption, MidpointRounding.AwayFromZero)).ToString());
						count++;
						break;


				}
			}
		}


		void SetUpConsumptionGrid()
		{
			int count = 1;

			foreach (TopFuelVolume vehicle in fuelSummary.TopFuelVolume)
			{
				switch (count)
				{
					case ((int)RowLabel.First):
						ConsumptionVehicleFirst.Text = vehicle.Description;
						ConsumptionVolumeFirst.FormattedText = FormatHelper.FormatVolume(((int)Math.Round(vehicle.Volume, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionDistanceFirst.FormattedText = FormatHelper.FormatDistance((Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionEntryConsumptionFirst.FormattedText = FormatHelper.FormatConsumption(((int)Math.Round(vehicle.Consumption, MidpointRounding.AwayFromZero)).ToString());
						count++;
						break;
					case ((int)RowLabel.Second):
						ConsumptionVehicleSecond.Text = vehicle.Description;
						ConsumptionVolumeSecond.FormattedText = FormatHelper.FormatVolume(((int)Math.Round(vehicle.Volume, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionDistanceSecond.FormattedText = FormatHelper.FormatDistance((Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionEntryConsumptionSecond.FormattedText = FormatHelper.FormatConsumption(((int)Math.Round(vehicle.Consumption, MidpointRounding.AwayFromZero)).ToString());
						count++;
						break;
					case ((int)RowLabel.Third):
						ConsumptionVehicleThird.Text = vehicle.Description;
						ConsumptionVolumeThird.FormattedText = FormatHelper.FormatVolume(((int)Math.Round(vehicle.Volume, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionDistanceThird.FormattedText = FormatHelper.FormatDistance((Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionEntryConsumptionThird.FormattedText = FormatHelper.FormatConsumption(((int)Math.Round(vehicle.Consumption, MidpointRounding.AwayFromZero)).ToString());
						count++;
						break;
					case ((int)RowLabel.Fourth):
						ConsumptionVehicleFourth.Text = vehicle.Description;
						ConsumptionVolumeFourth.FormattedText = FormatHelper.FormatVolume(((int)Math.Round(vehicle.Volume, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionDistanceFourth.FormattedText = FormatHelper.FormatDistance((Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionEntryConsumptionFourth.FormattedText = FormatHelper.FormatConsumption(((int)Math.Round(vehicle.Consumption, MidpointRounding.AwayFromZero)).ToString());
						count++;
						break;
					case ((int)RowLabel.Fifth):
						ConsumptionVehicleFifth.Text = vehicle.Description;
						ConsumptionVolumeFifth.FormattedText = FormatHelper.FormatVolume(((int)Math.Round(vehicle.Volume, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionDistanceFifth.FormattedText = FormatHelper.FormatDistance((Math.Round(vehicle.Distance, 1, MidpointRounding.AwayFromZero)).ToString());
						ConsumptionEntryConsumptionFifth.FormattedText = FormatHelper.FormatConsumption(((int)Math.Round(vehicle.Consumption, MidpointRounding.AwayFromZero)).ToString());
						count++;
						break;


				}
			}
		}

		private void SetUpPickers()
		{
			
			//datePicker.SelectedIndex = (int)App.SelectedDate;
			var s = App.SelectedVehicleGroup;

			//var e = VehicleGroupPicker.Items.IndexOf(App.SelectedVehicleGroup);
			//VehicleGroupPicker.SelectedIndex = App.SelectedVehicleGroupIndex;

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
		}

		async void GetFuelDataAsync()
		{
			App.ShowLoading(true);

			var s = DateHelper.GetDateRangeStartDate(App.SelectedDate);
			var e = DateHelper.GetDateRangeEndDate(App.SelectedDate);

			fuelSummary = await RestService.GetFuelSummaryDataAsync(App.SelectedVehicleGroup, s.ToString(Constants.API_DATE_FORMAT), e.ToString(Constants.API_DATE_FORMAT));


			//fuelConsumption = await RestService.GetFuelConsumptionAsync(App.SelectedVehicleGroup, DateHelper.GetGroupByPeriodStartDate(App.SelectedDate, s, e).ToString(Constants.API_DATE_FORMAT), e.ToString(Constants.API_DATE_FORMAT), DateHelper.GetGroupByPeriod(App.SelectedDate).ToString());

			SetUpSummary();
			SetUpVolumeGrid();
			SetUpConsumptionGrid();
			//SetUpGraph();

			App.ShowLoading(false);
		}
	}
}

