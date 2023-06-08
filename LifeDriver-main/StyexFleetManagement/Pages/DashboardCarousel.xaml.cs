using StyexFleetManagement.Resx;
using StyexFleetManagement.Statics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Models;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
    public partial class DashboardCarousel : CarouselPage
    {

        //Collection<FavouriteReport> favouriteReports;
        private Thickness padding;
        public DashboardCarousel()
        {
            InitializeComponent();

            this.Title = AppResources.dashboard_title_short;

            padding = new Thickness(0, Device.OnPlatform(0, 0, 0), 0, 0);

            //var dashboardPage = new FavouriteReportsPage { Padding = padding };
            var dashboardPage = new DashboardPage { Padding = padding };

            Children.Add(dashboardPage);

            if (Settings.Current.FavouriteReportOne != string.Empty)
            {
                Type elementType = Type.GetType(Settings.Current.FavouriteReportOne);
                object page = Activator.CreateInstance(elementType);
                
                Children.Add((ContentPage) page);
            }
            if (Settings.Current.FavouriteReportTwo != string.Empty)
            {
                Type elementType = Type.GetType(Settings.Current.FavouriteReportTwo);
                object page = Activator.CreateInstance(elementType);

                Children.Add((ContentPage)page);
            }
            if (Settings.Current.FavouriteReportThree != string.Empty)
            {
                Type elementType = Type.GetType(Settings.Current.FavouriteReportThree);
                object page = Activator.CreateInstance(elementType);

                Children.Add((ContentPage)page);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            /*var reportsList = App.FavouriteReports.GetItems().ToList();

            if (reportsList != null || reportsList.Count > 0)
                favouriteReports = new Collection<FavouriteReport>(reportsList);

            foreach (FavouriteReport report in favouriteReports)
            {
               
                string vehicleDescription = App.VehicleGroups.FindDescriptionFromId(report.VehicleGroupId);
                var page = await GetPageType(report.ReportType, report.DateRange, vehicleDescription, report.SelectedDateIndex, report.SelectVehicleIndex);

                Children.Add((ContentPage) page);
            }*/
        }


        //async Task<Page> GetPageType(ReportType reportType, ReportDateRange selectedDate, string selectedVehicleDescription, int selectedDateIndex, int selectVehicleIndex)
        //{
        //    var s = DateHelper.GetDateRangeStartDate(selectedDate).ToString(Constants.API_DATE_FORMAT);
        //    var e = DateHelper.GetDateRangeEndDate(selectedDate).ToString(Constants.API_DATE_FORMAT);

        //    var selectedVehicleGroup = App.VehicleGroups.FindIdFromDescription(selectedVehicleDescription);

        //    switch (reportType)
        //    {
        //        case ReportType.TIME_PROFILE_UTILIZATION:
        //            App.ShowLoading(true);


        //            var utilizationData = await RestService.GetUtilizationDataAsync(App.SelectedVehicleGroup, s, e);
        //            utilizationData.TotalDrivingTime = Math.Max(utilizationData.TotalDrivingTime - utilizationData.TotalIdleDuration, 0);

        //            App.ShowLoading(false);
        //            return (new VehicleUtilizationDetailPage(utilizationData, selectedDateIndex, selectVehicleIndex));
        //        case ReportType.OVERTIME_UTILIZATION:
        //            App.ShowLoading(true);
        //            var overtimeUtilizationData = await RestService.GetOvertimeUtilizationDataAsync(selectedVehicleGroup, s, e);

        //            App.ShowLoading(false);
        //            return (new VehicleOvertimeDetailPage(overtimeUtilizationData, selectedDateIndex, selectVehicleIndex));
        //        case ReportType.DRIVING_SUMMARY:
        //            App.ShowLoading(true);
        //            var drivingSummaryData = await RestService.GetDrivingSummaryAsync(selectedVehicleGroup, DateHelper.GetDateRangeStartDate(selectedDate).ToString(Constants.API_DATE_FORMAT), DateHelper.GetDateRangeEndDate(selectedDate).ToString(Constants.API_DATE_FORMAT));

        //            App.ShowLoading(false);
        //            if (drivingSummaryData != null)
        //                return (new VehicleSummaryDetailPage(drivingSummaryData, selectedDateIndex, selectVehicleIndex));
        //            else
        //                return (Page)Activator.CreateInstance(typeof(FavouriteReportsPage));
        //        case ReportType.FUEL_SUMMARY:
        //            App.ShowLoading(true);
        //            var fuelSummary = await RestService.GetFuelSummaryDataAsync(App.SelectedVehicleGroup, s, e);
        //            App.ShowLoading(false);
        //            if (fuelSummary != null)
        //                return (new FuelEntrySummary(fuelSummary, selectedDateIndex, selectVehicleIndex));
        //            else
        //                return (Page)Activator.CreateInstance(typeof(FavouriteReportsPage));
        //        case ReportType.FUEL_CONSUMPTION:

        //            return (new FuelEntryConsumption(selectedDateIndex, selectVehicleIndex));
        //        default:
        //            return (Page)Activator.CreateInstance(typeof(FavouriteReportsPage));
        //    }
        //}
    }
}
