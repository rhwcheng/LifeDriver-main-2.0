using System;
using System.Collections.Generic;
using Xamarin.Forms;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.ViewModel;
using System.Collections.ObjectModel;
using Syncfusion.SfChart.XForms;
using System.Linq;
using StyexFleetManagement.Models;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;

namespace StyexFleetManagement.Pages.VehicleSummary.Tiles
{
    public partial class VehicleExceptionsTile : ContentView
    {
        VehicleSummaryViewModel viewModel;
        public VehicleExceptionsTile(VehicleSummaryViewModel _viewModel)
        {
            InitializeComponent();
            this.titleLabel.Text = "Vehicle Exceptions";
            viewModel = _viewModel;
            SetUpExceptionLineGraphData();
        }


        public async void SetUpExceptionLineGraphData()
        {
            //Clear series
            exceptionBarGraph.Series.Clear();

            if (viewModel.VehicleEvents == null || viewModel.VehicleEvents.Count == 0)
                return;

            var tempStartDate = viewModel.StartDate;
            var tempEndDate = viewModel.EndDate;
            var groupByPeriod = DateHelper.GetGroupByPeriod(App.SelectedDate);

            var data = new ObservableCollection<ChartDataPoint>();
            //var breakingEventsSeries = new ObservableCollection<ChartDataPoint>();
            //var accelerationEventsSeries = new ObservableCollection<ChartDataPoint>();
            //var idleEventsSeries = new ObservableCollection<ChartDataPoint>();
            //var freewheelingEventsSeries = new ObservableCollection<ChartDataPoint>();
            //var recklessDrivingEventsSeries = new ObservableCollection<ChartDataPoint>();
            //var excessiveRPMEventsSeries = new ObservableCollection<ChartDataPoint>();
            //var accidentEventsSeries = new ObservableCollection<ChartDataPoint>();
            //var corneringEventsSeries = new ObservableCollection<ChartDataPoint>();
            //var laneChangeEventsSeries = new ObservableCollection<ChartDataPoint>();


         

                int totalSpeedingEvents;
                int totalBreakingEvents;
                int totalIdleEvents;
                int totalAccelerationEvents;
                //int totalFreewheelingEvents;
                //int totalRecklessDrivingEvents;
                //int totalExcessiveRPMEvents;
                //int totalAccidentEvents;
                //int totalCorneringEvents = 0;
                //int totalLaneChangeEvents = 0;


                if (Settings.Current.PlotFleetExceptions)
                {
                    totalSpeedingEvents = viewModel.VehicleEvents.Where(x => (FleetException)x.EventTypeId == FleetException.SPEEDING).Count();
                    totalBreakingEvents = viewModel.VehicleEvents.Where(x => (FleetException)x.EventTypeId == FleetException.HARSHBREAKING).Count();
                    totalAccelerationEvents = viewModel.VehicleEvents.Where(x => (FleetException)x.EventTypeId == FleetException.EXCESSIVEACCELERATION).Count();
                    totalIdleEvents = viewModel.VehicleEvents.Where(x => (FleetException)x.EventTypeId == FleetException.EXCESSIVEIDLE).Count();

                }
                else
                {
                    totalSpeedingEvents = viewModel.VehicleEvents.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.SPEEDING).Count();
                    totalBreakingEvents = viewModel.VehicleEvents.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.EXCESSIVEBREAKING).Count();
                    totalAccelerationEvents = viewModel.VehicleEvents.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.ACCELERATION).Count();
                    totalIdleEvents = viewModel.VehicleEvents.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.EXCESSIVEIDLE).Count();
                }
            data.Add(new ChartDataPoint(AppResources.speeding_label, totalSpeedingEvents));
            data.Add(new ChartDataPoint(AppResources.braking_label, totalBreakingEvents));
            data.Add(new ChartDataPoint(AppResources.acceleration_short_title, totalAccelerationEvents));
            data.Add(new ChartDataPoint(AppResources.idle_label, totalIdleEvents));

            exceptionBarGraph.Series.Add(new ColumnSeries
            {
                Label = AppResources.speeding_label,
                ItemsSource = data,
                EnableTooltip = true
            });

            //exceptionBarGraph.Series.Add(new ColumnSeries
            //{
            //    Label = AppResources.braking_label,
            //    ItemsSource = breakingEventsSeries,
            //    EnableTooltip = true
            //});


            //exceptionBarGraph.Series.Add(new ColumnSeries
            //{
            //    Label = AppResources.acceleration_label,
            //    ItemsSource = accelerationEventsSeries,
            //    EnableTooltip = true
            //});


            //exceptionBarGraph.Series.Add(new ColumnSeries
            //{
            //    Label = AppResources.idle_label,
            //    ItemsSource = idleEventsSeries,
            //    EnableTooltip = true
            //});

        }
    }
}
