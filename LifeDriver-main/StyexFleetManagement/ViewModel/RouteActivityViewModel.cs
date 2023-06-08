using StyexFleetManagement.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StyexFleetManagement.Models;
using static StyexFleetManagement.ViewModel.ExceptionSummaryViewModel;

namespace StyexFleetManagement.ViewModel
{
    internal class RouteActivityViewModel : BaseViewModel
    {
        List<Event> eventData;

        private List<VehicleRowItem> vehicleRowItems;
        private List<Trip> tripData;

        public RouteActivityViewModel()
        {
            eventData = new List<Event>();
            vehicleRowItems = new List<VehicleRowItem>();
            exceptionInfo = new ObservableCollection<ExceptionSummaryInfo>();
            tripData = new List<Trip>();
            FuelConsumption = new List<Event>();
            SelectedDate = App.SelectedDate;

            StartDate = App.StartDateSelected;
            EndDate = App.EndDateSelected;
        }
        public List<Trip> TripData
        {
            get => tripData;
            set
            {
                if (tripData == value)
                    return;
                tripData = value;

                OnPropertyChanged();
            }
        }

        public List<Event> EventData
        {
            get => eventData;
            set
            {
                if (eventData == value)
                    return;
                eventData = value;

                OnPropertyChanged();
            }
        }
        
        private ObservableCollection<ExceptionSummaryInfo> exceptionInfo;
        public ObservableCollection<ExceptionSummaryInfo> ExceptionInfoCollection
        {
            get => exceptionInfo;
            set => this.exceptionInfo = value;
        }
        public List<VehicleRowItem> VehicleRowItems
        {
            get => vehicleRowItems;
            set
            {
                vehicleRowItems = value;
                OnPropertyChanged();
            }
        }

        public async Task SetVehicleData(bool orderByDescending)
        {
            IOrderedEnumerable<IGrouping<string, Trip>> orderedList;
            //if (orderByDescending)
            //    orderedList = tripData.GroupBy(x => x.UnitId).OrderByDescending(g => g.Sum(x => x.NumberOfExceptions)).ThenBy(g=>g.First().UnitId);
            //else
            //    orderedList = tripData.GroupBy(x => x.UnitId).OrderBy(g => g.Sum(x=>x.NumberOfExceptions)).ThenBy(g => g.First().UnitId);

            var result = tripData.GroupBy(x => x.UnitId).Select(grp => new VehicleRowItem {
                GroupID = grp.Key,
                VehicleId = grp.First().VehicleId,
                VehicleDescription = grp.First().UnitId,
                Registration = "", Count = grp.Sum(x => x.NumberOfExceptions),
                MaxSpeed = grp.Max(x => x.MaxSpeed),
                Distance = 0, Duration = 0 }).Take(5);

            //Clear existing list
            vehicleRowItems.Clear();

            var throttler = new SemaphoreSlim(initialCount: 10);
            var allTasks = new List<Task>();


            foreach (var vehicle in result)
            {
                await throttler.WaitAsync();
                allTasks.Add(
                            Task.Run(async () =>
                            {
                                try
                                {
                                    if (vehicle != null)
                                    {
                                        vehicle.Distance = TripData.Where(x => x.VehicleId == vehicle.VehicleId).Sum(y => y.Distance);
                                        vehicle.Duration = (int) TripData.Where(x => x.VehicleId == vehicle.VehicleId).Sum(y => (y.EndLocalTimestamp - y.StartLocalTimestamp).TotalSeconds);
                                        var extendedVehicle = await VehicleAPI.GetVehicleById(vehicle.VehicleId);
                                        if (extendedVehicle != null)
                                            vehicle.Registration = extendedVehicle.Registration;

                                        //vehicle.IdleDuration = FuelConsumption.Where(x => x.ExcessiveIdleData != null && x.VehicleId == vehicle.VehicleId && x.LocalTimestamp >= StartDate && x.LocalTimestamp <= EndDate).Sum(x => x.ExcessiveIdleData.Duration);
                                        //vehicle.Count = FuelConsumption.Where(x => x.UnitId == vehicle.VehicleDescription && x.LocalTimestamp >= StartDate && x.LocalTimestamp <= EndDate).Count();

                                        var tripData = TripData.Where(x => x.UnitId == vehicle.VehicleDescription);
                                        if (tripData != null || tripData.Count() > 0)
                                        {
                                            if (Settings.Current.PlotFleetExceptions)
                                            {
                                                vehicle.IdleDuration = tripData.Sum(x => x.FleetEvents.Where(e => e.ExcessiveIdleData != null).Sum(e => e.ExcessiveIdleData.Duration));
                                                vehicle.Count = tripData.Sum(x => x.FleetEvents.Count);
                                            }
                                            else
                                            {
                                                vehicle.IdleDuration = tripData.Sum(x => x.UBIEvents.Where(e => e.ExcessiveIdleData != null).Sum(e => e.ExcessiveIdleData.Duration));
                                                vehicle.Count = tripData.Sum(x => x.UBIEvents.Count);

                                            }
                                        }
                                    
                                        
                                        /*if (vehicle.IdleDuration == 0)
                                        {
                                            EventTypeGroup eventGroup;
                                            if (Settings.Current.PlotFleetExceptions)
                                                eventGroup = EventTypeGroup.FleetExceptions;
                                            else
                                                eventGroup = EventTypeGroup.UBIExceptions;

                                            var events = await EventAPI.GetEventsByArrayAsync(vehicle.VehicleId, this.StartDate, this.EndDate, eventGroup, true, true);
                                            var idleEvents = events.Where(x => x.Speed == 0).ToList();
                                            var LinqList = idleEvents.Select(
                                                       (idleEvent, index) =>
                                                          new
                                                          {
                                                              ID = idleEvent.Id,
                                                              Date = idleEvent.LocalTimestamp,
                                                              DiffToPrev = (index > 0 ? idleEvent.LocalTimestamp - idleEvents[index - 1].LocalTimestamp : default(TimeSpan))
                                                          }
                                                   );

                                            vehicle.IdleDuration = (int)LinqList.Where(x => x.DiffToPrev > TimeSpan.FromSeconds(5)).Sum(x => x.DiffToPrev.TotalSeconds);
                                        }*/
                                        vehicleRowItems.Add(vehicle);
                                    }
                                }
                                finally
                                {
                                    throttler.Release();
                                }
                            }));


            }

            await Task.WhenAll(allTasks);

            if (orderByDescending)
                vehicleRowItems = vehicleRowItems.OrderByDescending(g => g.Count).ThenBy(g => g.VehicleDescription).ToList();
            else
                vehicleRowItems = vehicleRowItems.OrderBy(g => g.Count).ThenBy(g => g.VehicleDescription).ToList();

            return;
            
        }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public List<AggregatedTripData> AggregatedTripData { get; internal set; }
        public List<Event> FuelConsumption { get; internal set; }
        public ReportDateRange SelectedDate { get; set; }

        internal double GetAverageFuelLevel()
        {
            var eventsWithAverageFuelData = FuelConsumption.Where(x => x.FuelData?.AverageFuelConsumption != null).ToList();
            if (eventsWithAverageFuelData.Any())
            {
                return eventsWithAverageFuelData.GroupBy(x => x.LocalTimestamp.Day).Average(group => group.Sum(x => x.FuelData.AverageFuelConsumption.Value)) / 1000;
            }

            return 0;
        }
    }
}