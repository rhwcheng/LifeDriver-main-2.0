//using System;
//using Xamarin.Forms;
//using System.Collections.ObjectModel;
//using System.Linq;
//using StyexFleetManagement.Statics;
//using StyexFleetManagement.Resx;
//using System.Threading.Tasks;
//using Syncfusion.SfChart.XForms;

//namespace StyexFleetManagement
//{
//	public partial class FavouriteReportsPage : ContentPage
//	{
//		Collection<FavouriteReport> favouriteReports;

//		public FavouriteReportsPage ()
//		{
//            this.SizeChanged += FavouriteReportsPage_SizeChanged;
//			InitializeComponent ();

//            this.Title = AppResources.dashboard_title;

//            if (Device.Idiom == TargetIdiom.Phone)
//            {
//                TileGrid.IsVisible = false;
//            }

//            ToolbarItems.Add(new ToolbarItem
//			{
//				Icon = "ic_action_settings.png",
//				Order = ToolbarItemOrder.Primary,
//				Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
//			});

//            aveDurationLabel.FormattedText = FormatHelper.FormatTime("95", 26);
//            aveDistanceLabel.FormattedText = FormatHelper.FormatDistance("67", 26);

//            SetUpGraph();

//        }

//        private void FavouriteReportsPage_SizeChanged(object sender, EventArgs e)
//        {
//            var page = sender as ContentPage;
//            App.PageWidth = page.Width;
//            App.PageHeight = page.Height;
//        }

//        private void SetUpGraph()
//        {
//            SfChart chart = new SfChart { HeightRequest=200};

//            //Initializing Primary Axis
//            CategoryAxis primaryAxis = new CategoryAxis();

//            chart.PrimaryAxis = primaryAxis;

//            //Initializing Secondary Axis
//            NumericalAxis secondaryAxis = new NumericalAxis();
            
//            chart.SecondaryAxis = secondaryAxis;

//            var dataModel = new ChartDataModel();

            

//            chart.Series.Add(new ColumnSeries()
//            {
//                ItemsSource = dataModel.Utilization,
//                Color = Color.FromHex("#FF7F27")
//            });

//            chart.Series.Add(new ColumnSeries()
//            {
//                ItemsSource = dataModel.MockColumn,
//                Color = Palette.MainAccent
//            });


//            chart.Series.Add(new SplineSeries()
//            {
//                ItemsSource = dataModel.MockSpline

//            });

//            GraphContainer.Children.Add(chart);
//        }

//        protected override void OnAppearing()
//		{
//			base.OnAppearing();
            
//			//var reportsList = App.FavouriteReports.GetItems().ToList();

//			//if (reportsList != null || reportsList.Count > 0)
//			//	favouriteReports = new Collection<FavouriteReport>(reportsList);

//			//foreach (FavouriteReport report in favouriteReports)
//			//{
//			//	var reportStack = new StackLayout { Padding = 7 };

//			//	var titleLabel = new Label { TextColor = Palette.MainAccent };
//			//	titleLabel.Text = report.Title;
//			//	reportStack.Children.Add(titleLabel);

//			//	var startDateLabel = new Label();
//			//	startDateLabel.Text = AppResources.start_date + ": " + report.StartDate.ToString("d");
//			//	reportStack.Children.Add(startDateLabel);

//			//	var endDateLabel = new Label();
//			//	endDateLabel.Text = AppResources.end_date + ": " + report.StartDate.ToString("d");
//			//	reportStack.Children.Add(endDateLabel);

//			//	var vehicleLabel = new Label();
//			//	string vehicleDescription = App.VehicleGroups.FindDescriptionFromId(report.VehicleGroupId);
//			//	vehicleLabel.Text = vehicleDescription;
//			//	reportStack.Children.Add(vehicleLabel);

//			//	var gesture = new TapGestureRecognizer();
//			//	gesture.Tapped += async (sender, e) =>
//			//	{
//			//		await Navigation.PushAsync(await GetPageType(report.ReportType, report.DateRange, vehicleDescription, report.SelectedDateIndex, report.SelectVehicleIndex));
//			//	};

//			//	reportStack.GestureRecognizers.Add(gesture);
                
//			//}
//		}

//		//async Task<Page> GetPageType(ReportType reportType, ReportDateRange selectedDate, string selectedVehicleDescription, int selectedDateIndex, int selectVehicleIndex)
//		//{
//		//	var s = DateHelper.GetDateRangeStartDate(selectedDate).ToString(Constants.API_DATE_FORMAT);
//		//	var e = DateHelper.GetDateRangeEndDate(selectedDate).ToString(Constants.API_DATE_FORMAT);

//		//	var selectedVehicleGroup = App.VehicleGroups.FindIdFromDescription(selectedVehicleDescription);

//		//	switch (reportType)
//		//	{
//		//		case ReportType.TIME_PROFILE_UTILIZATION:
//		//			App.ShowLoading(true);


//		//			var utilizationData = await RestService.GetUtilizationDataAsync(App.SelectedVehicleGroup, s, e);
//		//			utilizationData.TotalDrivingTime = Math.Max(utilizationData.TotalDrivingTime - utilizationData.TotalIdleDuration, 0);

//		//			App.ShowLoading(false);
//		//			return (new VehicleUtilizationDetailPage(utilizationData, selectedDateIndex, selectVehicleIndex));
//		//		case ReportType.OVERTIME_UTILIZATION:
//		//			App.ShowLoading(true);
//		//			var overtimeUtilizationData = await RestService.GetOvertimeUtilizationDataAsync(selectedVehicleGroup, s, e);

//		//			App.ShowLoading(false);
//		//			return (new VehicleOvertimeDetailPage(overtimeUtilizationData, selectedDateIndex, selectVehicleIndex));
//		//		case ReportType.DRIVING_SUMMARY:
//		//			App.ShowLoading(true);
//		//			var drivingSummaryData = await RestService.GetDrivingSummaryAsync(selectedVehicleGroup, DateHelper.GetDateRangeStartDate(selectedDate).ToString(Constants.API_DATE_FORMAT), DateHelper.GetDateRangeEndDate(selectedDate).ToString(Constants.API_DATE_FORMAT));

//		//			App.ShowLoading(false);
//		//			if (drivingSummaryData != null)
//		//				return (new VehicleSummaryDetailPage(drivingSummaryData, selectedDateIndex, selectVehicleIndex));
//		//			else
//		//				return (Page)Activator.CreateInstance(typeof(FavouriteReportsPage));
//		//		case ReportType.FUEL_SUMMARY:
//		//			App.ShowLoading(true);
//		//			var fuelSummary = await RestService.GetFuelSummaryDataAsync(App.SelectedVehicleGroup, s, e);
//		//			App.ShowLoading(false);
//		//			if (fuelSummary != null)
//		//				return (new FuelEntrySummary(fuelSummary, selectedDateIndex, selectVehicleIndex));
//		//			else
//		//				return (Page)Activator.CreateInstance(typeof(FavouriteReportsPage));
//		//		case ReportType.FUEL_CONSUMPTION:
					
//		//			return (new FuelEntryConsumption(selectedDateIndex, selectVehicleIndex));
//		//		default:
//		//			return (Page)Activator.CreateInstance(typeof(FavouriteReportsPage));
//		//	}
//		//}


//}
//}

