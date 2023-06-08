using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using StyexFleetManagement.Models;
using StyexFleetManagement.Services;
using Syncfusion.SfChart.XForms;

namespace StyexFleetManagement.Data_Models
{
	public class ChartDataModel
	{
        private List<Event> eventData;

        public ObservableCollection<ChartDataPoint> Utilization { get; set; }
        public ObservableCollection<ChartDataPoint> UtilizationTwo { get; set; }
        public ObservableCollection<ChartDataPoint> MockSpline { get; set; }
        public ObservableCollection<ChartDataPoint> MockColumn { get; set; }

        public ChartDataModel(TimeUtilization data)
		{
			Utilization = new ObservableCollection<ChartDataPoint>();

			int drivingTime = data.TotalDrivingTime;
			int idleTime = data.TotalIdleDuration;
			int parkedTime = data.TotalTimeProfileTime;

			if (drivingTime + idleTime + parkedTime == 0)
			{
				Utilization.Add(new ChartDataPoint("No Activity", 1));
			}
			else
			{
				Utilization.Add(new ChartDataPoint("Driving", data.TotalDrivingTime));
				Utilization.Add(new ChartDataPoint("Idle", data.TotalIdleDuration));
				Utilization.Add(new ChartDataPoint("Parked", data.TotalTimeProfileTime));
			}
		}

        public ChartDataModel()
        {
            Utilization = new ObservableCollection<ChartDataPoint>();
            Utilization.Add(new ChartDataPoint("09/01", 42));
            Utilization.Add(new ChartDataPoint("09/02", 44));
            Utilization.Add(new ChartDataPoint("09/03", 53));
            Utilization.Add(new ChartDataPoint("09/04", 64));
            Utilization.Add(new ChartDataPoint("09/05", 75));
            Utilization.Add(new ChartDataPoint("09/06", 83));
            Utilization.Add(new ChartDataPoint("09/07", 87));
            Utilization.Add(new ChartDataPoint("09/08", 84));
            Utilization.Add(new ChartDataPoint("09/09", 78));


            MockColumn = new ObservableCollection<ChartDataPoint>();
            foreach (ChartDataPoint point in Utilization)
            {
                MockColumn.Add(new ChartDataPoint(point.XValue, point.YValue * 0.8));
            }

            MockSpline = new ObservableCollection<ChartDataPoint>();
            MockSpline.Add(new ChartDataPoint("09/01", 32));
            MockSpline.Add(new ChartDataPoint("09/02", 32));
            MockSpline.Add(new ChartDataPoint("09/03", 43));
            MockSpline.Add(new ChartDataPoint("09/04", 53));
            MockSpline.Add(new ChartDataPoint("09/05", 35));
            MockSpline.Add(new ChartDataPoint("09/06", 75));
            MockSpline.Add(new ChartDataPoint("09/07", 56));
            MockSpline.Add(new ChartDataPoint("09/08", 67));
            MockSpline.Add(new ChartDataPoint("09/09", 62));
        }

        public ChartDataModel(FuelConsumption data)
		{
			Utilization = new ObservableCollection<ChartDataPoint>();
			GroupByPeriod periodType = (GroupByPeriod) Enum.Parse(typeof(GroupByPeriod), data.PeriodType);

			foreach (ConsumptionItem dataPoint in data.Items)
			{
				Utilization.Add(new ChartDataPoint(dataPoint.PeriodStart, dataPoint.Value));
			}
		}

		public ChartDataModel(FuelEntryVolume data)
		{
			Utilization = new ObservableCollection<ChartDataPoint>();
			GroupByPeriod periodType = (GroupByPeriod)Enum.Parse(typeof(GroupByPeriod), data.PeriodType);

			foreach (VolumeItem dataPoint in data.Items)
			{
				Utilization.Add(new ChartDataPoint(DateHelper.GetPeriodLabel(dataPoint.PeriodStart, periodType), dataPoint.Value));
			}
		}

		public ChartDataModel(FuelEntryCost data)
		{
			Utilization = new ObservableCollection<ChartDataPoint>();
			GroupByPeriod periodType = (GroupByPeriod)Enum.Parse(typeof(GroupByPeriod), data.PeriodType);

			foreach (CostItem dataPoint in data.Items)
			{
				Utilization.Add(new ChartDataPoint(DateHelper.GetPeriodLabel(dataPoint.PeriodStart, periodType), dataPoint.Value));
			}
		}
        public ChartDataModel(List<DriverEvent> data, LicenseCategory category)
        {
            Utilization = new ObservableCollection<ChartDataPoint>();
            switch (category)
            {
                case LicenseCategory.B:
                    Utilization.Add(new ChartDataPoint("B1", data.Where(x => x.DriverData.LicenseType == (int)LicenseType.B1).Count()));
                    Utilization.Add(new ChartDataPoint("B2", data.Where(x => x.DriverData.LicenseType == (int)LicenseType.B2).Count()));
                    Utilization.Add(new ChartDataPoint("B3", data.Where(x => x.DriverData.LicenseType == (int)LicenseType.B3).Count()));
                    Utilization.Add(new ChartDataPoint("B4", data.Where(x => x.DriverData.LicenseType == (int)LicenseType.B4).Count()));
                    break;
                case LicenseCategory.T:
                    Utilization.Add(new ChartDataPoint("T1", data.Where(x => x.DriverData.LicenseType == (int)LicenseType.T1).Count()));
                    Utilization.Add(new ChartDataPoint("T2", data.Where(x => x.DriverData.LicenseType == (int)LicenseType.T2).Count()));
                    Utilization.Add(new ChartDataPoint("T3", data.Where(x => x.DriverData.LicenseType == (int)LicenseType.T3).Count()));
                    Utilization.Add(new ChartDataPoint("T4", data.Where(x => x.DriverData.LicenseType == (int)LicenseType.T4).Count()));
                    break;
                case LicenseCategory.Personal:
                    var groups = data.Where(l => l.DriverData.LicenseType.ToString().Length > 2).GroupBy(x => x.DriverData.LicenseType).Select(group => new {
                        Metric = group.Key,
                        Count = group.Count()
                    }).OrderBy(y => y.Metric);

                    foreach (var group in groups)
                    {
                        Utilization.Add(new ChartDataPoint(group.Metric, group.Count));
                    }
                    
                    break;
            }
            

           
           
            
            /*
            foreach (DriverEvent dataPoint in data)
            {
                switch ((LicenseType) dataPoint.DriverData.LicenseType)
                {
                    case (LicenseType.B1):
                        Utilization.Add(new ChartDataPoint("B1", 1));
                        break;
                    case (LicenseType.B2):
                        Utilization.Add(new ChartDataPoint("B2", 1));
                        break;
                    case (LicenseType.B3):
                        Utilization.Add(new ChartDataPoint("B3", 1));
                        break;
                    case (LicenseType.B4):
                        Utilization.Add(new ChartDataPoint("B4", 1));
                        break;
                    case (LicenseType.T1):
                        Utilization.Add(new ChartDataPoint("T1", 1));
                        break;
                    case (LicenseType.T2):
                        Utilization.Add(new ChartDataPoint("T2", 1));
                        break;
                    case (LicenseType.T3):
                        Utilization.Add(new ChartDataPoint("T3", 1));
                        break;
                    case (LicenseType.T4):
                        Utilization.Add(new ChartDataPoint("T4", 1));
                        break;
                    default:
                        Utilization.Add(new ChartDataPoint("Personal", 1));
                        break;
                }
                
            }*/
        }



        public ChartDataModel(OvertimeUtilization overtimeData)
		{
			Utilization = new ObservableCollection<ChartDataPoint>();

			long drivingIn = overtimeData.TotalDrivingTimeInsideProfileTime;
			long drivingOut = overtimeData.TotalDrivingTimeOutsideProfileTime;

			// if both are equal to 0, set drivingIn as 100%
			if (drivingIn == 0 && drivingOut == 0)
			{
				drivingIn = 1;
			}


			Utilization.Add(new ChartDataPoint("Work Hours", drivingIn));
			Utilization.Add(new ChartDataPoint("Overtime", drivingOut));

		}



		public ChartDataModel(List<UtilizationHistogram> histogramValues)
		{
			Utilization = new ObservableCollection<ChartDataPoint>();
			foreach (UtilizationHistogram dataPoint in histogramValues)
			{
				Utilization.Add(new ChartDataPoint(dataPoint.Bin.ToString(), dataPoint.Value));
			}
		}

        public ChartDataModel(List<Event> eventData)
        {
            if (eventData == null || eventData.Count == 0)
                return;
            this.eventData = eventData;
            var startDate = App.StartDateSelected;
            var endDate = App.EndDateSelected;
            var groupByPeriod = DateHelper.GetGroupByPeriod(App.SelectedDate);

            Utilization = new ObservableCollection<ChartDataPoint>();
            UtilizationTwo = new ObservableCollection<ChartDataPoint>();

            if (startDate > endDate)
                return;

            switch (groupByPeriod)
            {
                case (GroupByPeriod.byday):
                    while (startDate < endDate)
                    {
                        var startDuration = startDate;
                        var endDuration = startDuration.AddDays(1);

                        int totalSpeedingEvents;
                        int totalBreakingEvents;

                        if (Settings.Current.PlotFleetExceptions)
                        {
                            totalSpeedingEvents = eventData.Where(x => (FleetException)x.EventTypeId == FleetException.SPEEDING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                            totalBreakingEvents = eventData.Where(x => (FleetException)x.EventTypeId == FleetException.SPEEDING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();

                        }
                        else
                        {
                            totalSpeedingEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.SPEEDING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                            totalBreakingEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.SPEEDING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                        }
                        Utilization.Add(new ChartDataPoint(DateHelper.GetPeriodLabel(startDate, groupByPeriod), totalSpeedingEvents));
                        UtilizationTwo.Add(new ChartDataPoint(DateHelper.GetPeriodLabel(startDate, groupByPeriod), totalBreakingEvents));

                        startDate = startDate.AddDays(1);
                    }
                    break;
                case (GroupByPeriod.byweek):
                    while (startDate < endDate)
                    {
                        var startDuration = startDate;
                        var endDuration = startDuration.AddDays(7);

                        int totalSpeedingEvents;
                        int totalBreakingEvents;

                        if (Settings.Current.PlotFleetExceptions)
                        {
                            totalSpeedingEvents = eventData.Where(x => (FleetException)x.EventTypeId == FleetException.SPEEDING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                            totalBreakingEvents = eventData.Where(x => (FleetException)x.EventTypeId == FleetException.SPEEDING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();

                        }
                        else
                        {
                            totalSpeedingEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.SPEEDING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                            totalBreakingEvents = eventData.Where(x => (DriverBehaviourException)x.EventTypeId == DriverBehaviourException.SPEEDING && x.LocalTimestamp > startDuration && x.LocalTimestamp < endDuration).Count();
                        }
                        Utilization.Add(new ChartDataPoint(DateHelper.GetPeriodLabel(startDate, groupByPeriod), totalSpeedingEvents));
                        UtilizationTwo.Add(new ChartDataPoint(DateHelper.GetPeriodLabel(startDate, groupByPeriod), totalBreakingEvents));

                        startDate = startDate.AddDays(7);
                    }
                    break;
            }
        }
    }
}

