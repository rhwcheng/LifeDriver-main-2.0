using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StyexFleetManagement.Models;

namespace StyexFleetManagement.ViewModel
{
    internal class ExceptionSummaryViewModel : BaseViewModel
    {
        List<Event> eventData;
        private bool totalExceptionOverlayIsVisible;
        private bool exceptionVsDistanceOverlayIsVisible;

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

        public bool TotalExceptionOverlayIsVisible
        {
            get => totalExceptionOverlayIsVisible;
            set
            {
                if (totalExceptionOverlayIsVisible == value)
                    return;
                totalExceptionOverlayIsVisible = value;
                OnPropertyChanged();
            }
        }
        private ObservableCollection<ExceptionSummaryInfo> exceptionInfo;
        public ObservableCollection<ExceptionSummaryInfo> ExceptionInfoCollection
        {
            get => exceptionInfo;
            set => this.exceptionInfo = value;
        }

        public bool ExceptionVsDistanceOverlayIsVisible
        {
            get => exceptionVsDistanceOverlayIsVisible;
            set
            {
                if (exceptionVsDistanceOverlayIsVisible == value)
                    return;
                exceptionVsDistanceOverlayIsVisible = value;
                OnPropertyChanged();
            }
        }


        private List<VehicleRowItem> topFiveVehicles;
        internal DateTime StartDate { get; set; }
        internal DateTime EndDate { get; set; }

        public List<VehicleRowItem> TopFiveVehicles
        {
            get => topFiveVehicles;
            set
            {
                topFiveVehicles = value;
                OnPropertyChanged();
            }
        }
        public async Task SetOrderedVehicles(bool orderByDescending)
        {
            List<VehicleRowItem> result;

            if (orderByDescending){
                result = eventData.GroupBy(x => x.UnitId)
                                  .OrderByDescending(group => group.Count())
                                  .Take(5)
                                  .Select(grp =>
                    new VehicleRowItem
                    {
                        GroupID = grp.Key,
                        VehicleId = grp.First().VehicleId,
                        VehicleDescription = grp.First().UnitId,
                        Registration = "",
                        MaxSpeed = grp.Max(x => x.Speed),
                        MaxRPM = grp.Max(x => x.RPM),
                        IdleDuration = grp.Where(x => x.ExcessiveIdleData != null).Sum(x => x.ExcessiveIdleData.Duration),
                        Distance = 0,
                        Duration = 0
                    }).ToList();
            }
                
            else{
                result = eventData.GroupBy(x => x.UnitId)
                                  .OrderBy(group => group.Count())
                                  .Take(5)
                                  .Select(grp =>
                    new VehicleRowItem
                    {
                        GroupID = grp.Key,
                        VehicleId = grp.First().VehicleId,
                        VehicleDescription = grp.First().UnitId,
                        Registration = "",
                        MaxSpeed = grp.Max(x => x.Speed),
                        MaxRPM = grp.Max(x => x.RPM),
                        IdleDuration = grp.Where(x => x.ExcessiveIdleData != null).Sum(x => x.ExcessiveIdleData.Duration),
                        Distance = 0,
                        Duration = 0
                    }).ToList();
            }

            //Clear existing list
            topFiveVehicles.Clear();
            
            //var throttler = new SemaphoreSlim(initialCount: 10);
            //var allTasks = new List<Task>();


            foreach (var vehicle in result)
            {
                //await throttler.WaitAsync();
                //allTasks.Add(
                            //Task.Run(async () =>
                            //{
                            try
                            {
                                if (vehicle != null)
                                {
                                    if (AggregatedTripData != null)
                                    {
                                        var tripData = AggregatedTripData.Where(x => x.VehicleId == vehicle.VehicleId).FirstOrDefault();
                                        if (tripData != null)
                                        {

                                            vehicle.Distance = tripData.TotalDistance;
                                            vehicle.Duration = tripData.TotalDuration;
                                        }
                                    }
                                        var extendedVehicle = await VehicleAPI.GetVehicleById(vehicle.VehicleId);
                                        if (extendedVehicle != null)
                                            vehicle.Registration = extendedVehicle.Registration;
                                        if (vehicle.IdleDuration == 0)
                                        {
                                            EventTypeGroup eventGroup;
                                            if (Settings.Current.PlotFleetExceptions)
                                                eventGroup = EventTypeGroup.FleetExceptions;
                                            else
                                                eventGroup = EventTypeGroup.UBIExceptions;

                                            var events = await EventAPI.GetEventsByArrayAsync(vehicle.VehicleId, this.StartDate, this.EndDate, eventGroup, true, true);
                                                        var idleEvents = events.Where(x => Math.Abs(x.Speed) < float.Epsilon).ToList();
                                            var linqList = idleEvents.Select(
                                                       (idleEvent, index) =>
                                                          new
                                                          {
                                                              ID = idleEvent.Id,
                                                              Date = idleEvent.LocalTimestamp,
                                                              DiffToPrev = (index > 0 ? idleEvent.LocalTimestamp - idleEvents[index - 1].LocalTimestamp : default(TimeSpan))
                                                          }
                                                   );

                                                    vehicle.IdleDuration = (int) linqList.Where(x => x.DiffToPrev > TimeSpan.FromSeconds(5)).Sum(x => x.DiffToPrev.TotalSeconds);
                                        }
                                        topFiveVehicles.Add(vehicle);
                                    }
                                }
                catch (Exception e)
                {
                    Serilog.Log.Debug(e.Message);
                    await App.MainDetailPage.DisplayAlert(AppResources.error_label, AppResources.error_label, AppResources.button_ok);
                    break;
                }
                            //    finally
                            //    {
                            //        throttler.Release();
                            //    }
                            //}));


            }

            //await Task.WhenAll(allTasks);


           
        }
        

        public List<AggregatedTripData> AggregatedTripData { get; internal set; }
        public ReportDateRange SelectedDate { get; private set; }

        public ExceptionSummaryViewModel()
        {
            eventData = new List<Event>();
            topFiveVehicles = new List<VehicleRowItem>();
            exceptionInfo = new ObservableCollection<ExceptionSummaryInfo>();

            SelectedDate = App.SelectedDate;
            StartDate = App.StartDateSelected;
            EndDate = App.EndDateSelected;

        }


    }
}