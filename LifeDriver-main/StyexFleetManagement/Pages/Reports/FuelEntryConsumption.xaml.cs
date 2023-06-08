using System;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Data_Models;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages.Reports
{
	public partial class FuelEntryConsumption : ReportPage
	{
		SfChart consumptionGraph;

		FuelEntryVolume fuelVolume;
		FuelEntryCost fuelCost;
		FuelConsumption fuelConsumption;

		ChartDataModel dataModel;
        private ReportDateRange reportDateRange;

        public DateTime StartDate { get; private set; }
        public DateTime EndDate { get; private set; }
        public ReportHeaderLayout ReportHeader { get; private set; }

        //      public FuelEntryConsumption(int selectedDate, int selectedVehicle)
        //{
        //	InitializeComponent();

        //          ToolbarItem favouriteButton = new ToolbarItem();
        //          App.ConfigureFavouriteButton(this.GetType().ToString(), favouriteButton);
        //          ToolbarItems.Add(favouriteButton);

        //          BackgroundColor = App.LightGray;


        //          //set initial picker selection
        //          AxisPicker.SelectedIndex = 0;

        //          SetUpUI();

        //	SetUpPickers();


        //          InitializeGraph();

        //      }

        public FuelEntryConsumption() : base()
        {
            InitializeComponent();

            this.Title = AppResources.report_title_fuel_consumption;

            SetUpUI();
            
            this.ReportHeader.SearchTapped += OnSearch_Tapped;

            InitializeGraph();

            reportDateRange = ReportDateRange.PREVIOUS_MONTH;
            var tempDate = DateTime.Now.AddMonths(-6);
            this.StartDate = new DateTime(tempDate.Year, tempDate.Month, 1);
            this.EndDate = this.StartDate.AddMonths(6);

            GetFuelData();

        }

        private void SetUpUI()
        {
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });

            ReportHeader = new ReportHeaderLayout(showDatePicker:false);
            Container.Children.Insert(0, ReportHeader);

            BackgroundColor = App.LightGray;

            AxisPicker.Items.Add(AppResources.consumption);
            AxisPicker.Items.Add(AppResources.volume);
            AxisPicker.Items.Add(AppResources.cost);
            //set initial picker selection
            AxisPicker.SelectedIndex = 0;

            SetUpPickers();

        }

		protected override void OnAppearing()
		{
			base.OnAppearing();
            

		}

		private void InitializeGraph()
		{
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

            //Initializing chart
            consumptionGraph = new SfChart()
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				Title = new ChartTitle() { Text = AppResources.report_title_fuel_entry_consumption                },
                HeightRequest = height
			};


            //Initializing Primary Axis
            DateTimeAxis primaryAxis = new DateTimeAxis { IntervalType = DateTimeIntervalType.Months, Interval = 1, LabelStyle = new ChartAxisLabelStyle { LabelFormat = "MMM" } };
			consumptionGraph.PrimaryAxis = primaryAxis;

			//Initializing Secondary Axis
			NumericalAxis secondaryAxis = new NumericalAxis();
			consumptionGraph.SecondaryAxis = secondaryAxis;


			Container.Children.Add(consumptionGraph);

		}

		bool SetUpGraph()
		{
			switch (AxisPicker.SelectedIndex)
			{
				case 0:
					{
						if (fuelConsumption != null)
						{

							dataModel = new ChartDataModel(fuelConsumption);

							//set up graph series
							consumptionGraph.Series.Clear();
							consumptionGraph.Series.Add(new ColumnSeries()
							{
								ItemsSource = dataModel.Utilization,
								Color = Color.FromHex(Constants.ACCENT_COLOUR)
							});
							consumptionGraph.SecondaryAxis.Title = new ChartAxisTitle { Text =
                                $"{AppResources.consumption}"
                            };
							return true;
						}

						return false;
					}
				case 1:
					{
						if (fuelVolume != null)
						{

							dataModel = new ChartDataModel(fuelVolume);

							//set up graph series
							consumptionGraph.Series.Clear();
							consumptionGraph.Series.Add(new ColumnSeries()
							{
								ItemsSource = dataModel.Utilization,
								Color = Color.FromHex(Constants.ACCENT_COLOUR)
							});
							consumptionGraph.SecondaryAxis.Title = new ChartAxisTitle { Text =
                                $"{AppResources.volume} ({AppResources.volume_litres_abbr})"
                            };
							return true;
						}

						return false;
					}
				case 2:
					{
						if (fuelCost != null)
						{

							dataModel = new ChartDataModel(fuelCost);

							//set up graph series
							consumptionGraph.Series.Clear();
							consumptionGraph.Series.Add(new ColumnSeries()
							{
								ItemsSource = dataModel.Utilization,
								Color = Color.FromHex(Constants.ACCENT_COLOUR)
							});
							consumptionGraph.SecondaryAxis.Title = new ChartAxisTitle { Text = AppResources.cost };
							return true;
						}

						return false;
					}
				default:
					return false;

			}
		}

		async void GetFuelData()
		{
			App.ShowLoading(true);

			var s = StartDate;
			var e = EndDate;


			fuelVolume = await RestService.GetFuelVolumeAsync(App.SelectedVehicleGroup, StartDate.ToString(Constants.API_DATE_FORMAT), EndDate.ToString(Constants.API_DATE_FORMAT), GroupByPeriod.bymonth.ToString());
			fuelCost = await RestService.GetFuelCostAsync(App.SelectedVehicleGroup, StartDate.ToString(Constants.API_DATE_FORMAT), EndDate.ToString(Constants.API_DATE_FORMAT), GroupByPeriod.bymonth.ToString());
			fuelConsumption = await RestService.GetFuelConsumptionAsync(App.SelectedVehicleGroup, StartDate.ToString(Constants.API_DATE_FORMAT), EndDate.ToString(Constants.API_DATE_FORMAT), GroupByPeriod.bymonth.ToString());


			SetUpGraph();

			App.ShowLoading(false);
		}

		private void SetUpPickers()
		{
			this.AxisPicker.SelectedIndexChanged += (sender, e) =>
			{
				SetUpGraph();
			};
		}

        private void Date_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StartDate")
            {
                var value = (sender as DatePickerLayout).StartDate;
                if (StartDate != value)
                {
                    StartDate = value;
                }
                else
                    return;
            }
            if (e.PropertyName == "EndDate")
            {
                var value = (sender as DatePickerLayout).EndDate;
                if (EndDate != value)
                {
                    EndDate = value;
                    //await GetDriverEventData();
                    //SetUpGraphs();
                }
                else
                    return;
            }
            if (e.PropertyName == "SelectedDate")
            {
                var value = (sender as DatePickerLayout).SelectedDate;
                if (reportDateRange != value)
                {
                    reportDateRange = value;
                }
                else
                    return;
            }
        }
        private async void OnSearch_Tapped(object sender, EventArgs e)
        {
            GetFuelData();
        }

    }
}

