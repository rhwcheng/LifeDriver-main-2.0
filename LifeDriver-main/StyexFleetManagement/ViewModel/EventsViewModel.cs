using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using StyexFleetManagement.Services;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using StyexFleetManagement.Models;

namespace StyexFleetManagement.ViewModel
{
    public class EventsViewModel : BaseViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ReportDateRange SelectedDate { get; set; }
        public List<Event> Events { get; set; }
        private SemaphoreSlim throttler;


        public EventsViewModel()
        {
            SelectedDate = App.SelectedDate;
            StartDate = App.StartDateSelected;
            EndDate = App.EndDateSelected;
            Events = new List<Event>();
            throttler = new SemaphoreSlim(5);
        }

        public async Task LoadData()
        {
            var vehicles = await VehicleAPI.GetVehicleInGroupAsync(App.SelectedVehicleGroup);
            var allTasks = new List<Task>();

            foreach (var vehicle in vehicles)
            {
                await throttler.WaitAsync();
                allTasks.Add(
                                Task.Run(async () =>
                                {
                                    try
                                    {
                        var vehicleEvents = await EventAPI.GetEventsByArrayAsync(vehicle.Id, StartDate, EndDate, EventTypeGroup.AllExceptions, includeTripStartStop: true, includePeriodicPosition: true);
                                        foreach (var vehicleEvent in vehicleEvents)
                                        {
                                            vehicleEvent.VehicleDescription = vehicle.Description;
                                        }
                                        Events.AddRange(vehicleEvents);
                                    }
                                    finally
                                    {
                                        throttler.Release();
                                    }
                                }
                                        ));
            }

            await Task.WhenAll(allTasks);
            Events = Events.OrderByDescending(x => x.LocalTimestamp.DateTime)
                           .ThenBy(x => x.VehicleDescription).ToList();
            return;


        }
    }
}