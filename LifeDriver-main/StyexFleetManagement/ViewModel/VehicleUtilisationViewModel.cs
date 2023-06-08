using StyexFleetManagement.Helpers;
using StyexFleetManagement.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StyexFleetManagement.Models;

namespace StyexFleetManagement.ViewModel
{
    public class VehicleUtilisationViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<Trip> TripData { get; set; }
        public List<Trip> CurrentPeriodTripData { get; set; }
        //    get
        //    {
        //        return TripData.Where(x => x.StartLocalTimestamp >= StartDate && x.StartLocalTimestamp <= EndDate).ToList();

        //    }
        //}
        public List<Trip> PreviousPeriodTripData { get; set; }
        //{
        //    get
        //    {
        //        DateTime prevStartDate;
        //        var currentDuration = Math.Round((EndDate - StartDate).TotalDays - 1);
        //        prevStartDate = StartDate.AddDays(-1 * currentDuration);

        //        return TripData.Where(x => x.StartLocalTimestamp >= prevStartDate && x.StartLocalTimestamp <= StartDate).ToList();

        //    }
        //}

        public ObservableCollection<VehicleUtilisationInfo> UtilisationInfoCollection { get; internal set; }

        public VehicleUtilisationViewModel()
        {
            UtilisationInfoCollection = new ObservableCollection<VehicleUtilisationInfo>();
            TripData = new List<Trip>();
            TopFiveVehicles = new List<VehicleUtilisationInfo>();
            Throttler = new SemaphoreSlim(initialCount: 10);
        }

        public List<VehicleUtilisationInfo> TopFiveVehicles { get; set; }
        public SemaphoreSlim Throttler { get; set; }

        public async Task SetTableData(bool orderByDescending)
        {

            List<VehicleUtilisationInfo> result;
            if (orderByDescending){

                result = CurrentPeriodTripData.GroupBy(x => x.VehicleId)
                                              .OrderByDescending(grp => grp.Sum(x => x.Duration))
                                              .Take(5)
                                              .Select(grp => new VehicleUtilisationInfo
                                              {
                                                  VehicleId = grp.First().VehicleId,
                                                  UnitId = grp.First().UnitId,
                                                  Duration = grp.Sum(x => x.Duration),
                                                  Distance = grp.Sum(x => x.Distance)
                                              })
                                              .ToList();
            }
            
            else{

                result = CurrentPeriodTripData.GroupBy(x => x.VehicleId)
                                              .OrderBy(grp => grp.Sum(x => x.Duration))
                                              .Take(5)
                                              .Select(grp => new VehicleUtilisationInfo
                                              {
                                                  VehicleId = grp.First().VehicleId,
                                                  UnitId = grp.First().UnitId,
                                                  Duration = grp.Sum(x => x.Duration),
                                                  Distance = grp.Sum(x => x.Distance)
                                              })
                                              .ToList();
            }

            //Clear existing list
            TopFiveVehicles.Clear();

            var currentTime = DateTime.Now;
            var vehicles = await VehicleAPI.GetVehicleInGroupAsync(App.SelectedVehicleGroup);

            foreach (var vehicle in result)
            {
                if (vehicle != null)
                {
                    vehicle.VehicleDescription = vehicles.Where(x => x.Id == vehicle.VehicleId).First().Description;

                    var totalDistance = vehicle.Distance;
                    var averageDistance = CurrentPeriodTripData.Where(x => x.VehicleId == vehicle.VehicleId).Average(x => x.Distance);
                    var totalDuration = vehicle.Duration;
                    var averageDuration = CurrentPeriodTripData.Where(x => x.VehicleId == vehicle.VehicleId).Average(x => x.Duration);
                    var totalTimespan = TimeSpan.FromSeconds(totalDuration);
                    var averageTimespan = TimeSpan.FromSeconds(averageDuration);

                    vehicle.DistanceTotalAndAverage =
                        $"{FormatHelper.FormatDistance(Math.Round(totalDistance, 0).ToString())} / {FormatHelper.FormatDistance(Math.Round(averageDistance, 0).ToString())}";
                    vehicle.DurationTotalAndAverage =
                        $"{FormatHelper.ToShortForm(totalTimespan).ToString()} / {FormatHelper.ToShortForm(averageTimespan).ToString()}";

                    var currentUtilisation = GetUtilizationPercent(vehicle.VehicleId, CurrentPeriodTripData);
                    var previousUtilisation = GetUtilizationPercent(vehicle.VehicleId, PreviousPeriodTripData);

                    vehicle.UtilisationPercentage =
                        $"{currentUtilisation.ToString()}% / {previousUtilisation.ToString()}%";

                    if (previousUtilisation > currentUtilisation)
                    {
                        vehicle.Result = Result.Decreased;
                    }
                    else
                    {
                        if (previousUtilisation < currentUtilisation)
                        {
                            vehicle.Result = Result.Increased;
                        }
                        else
                        {
                            vehicle.Result = Result.Same;
                        }
                    }

                    //var lastEvent = await GetLatestEvent(vehicle.VehicleId);
                    //vehicle.LastReportedTime = currentTime - lastEvent;

                    TopFiveVehicles.Add(vehicle);
                }


            }

        }

        public int GetUtilizationPercent(string vehicleId, List<Trip> tripData)
        {
            if (tripData == null || tripData.Count == 0)
                return 0;

            var totalSeconds = tripData.Sum(x => x.Duration);

            var drivingDuration = tripData.Where(x => x.VehicleId == vehicleId).Sum(x => x.Duration);

            return (int) Math.Round((drivingDuration * 100 / totalSeconds));
        }

        private async Task<DateTimeOffset> GetLatestEvent(string vehicleId)
        {
            
            List<Event> events = new List<Event>();

            var tempStartDate = DateTime.Now.AddDays(-1);
            var tempEndDate = DateTime.Now;

            var plotFleetException = Settings.Current.PlotFleetExceptions;
            List<Event> data;

            if (plotFleetException)
                data = await EventAPI.GetEventsByArrayAsync(vehicleId, tempStartDate, tempEndDate, EventTypeGroup.FleetExceptions, true, true);
            else
                data = await EventAPI.GetEventsByArrayAsync(vehicleId, tempStartDate, tempEndDate, EventTypeGroup.UBIExceptions, true, true);

            if (data == null || data.Count == 0)
                return default(DateTimeOffset) ;

            return data.Max(x => x.LocalTimestamp);

        }

        internal double GetAverageDistance()
        {
            return CurrentPeriodTripData.GroupBy(x => x.StartLocalTimestamp.Day).Average(g => g.Sum(y => y.Distance));
        }
        internal double GetAverageDuration()
        {
            return CurrentPeriodTripData.GroupBy(x => x.StartLocalTimestamp.Day).Average(g => g.Sum(y => y.Duration))/60;
        }
    }
}
