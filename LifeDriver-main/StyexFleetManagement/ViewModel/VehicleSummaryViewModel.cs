using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using StyexFleetManagement.Models;
using StyexFleetManagement.Services;

namespace StyexFleetManagement.ViewModel
{
    public class VehicleSummaryViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<VehicleItem> AllVehicles { get; set; }
        ExtendedVehicle selectedVehicle;
        UnitLocation lastKnownPosition;
        public List<Event> VehicleEvents { get; set; }
        public List<Trip> Trips { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        int exceptionCount;
        int totalDistance;
        int tripCount;
        int totalDuration;
        int maxSpeed;
        int maxRPM;
        Event recentViolationOne;
        Event recentViolationTwo;
        Event recentViolationThree;
        Event recentViolationFour;
        TripFuelConsumption latestTripOne;
        TripFuelConsumption latestTripTwo;
        TripFuelConsumption latestTripThree;

        public VehicleSummaryViewModel()
        {
            VehicleEvents = new List<Event>();

            StartDate = DateHelper.GetDateRangeStartDate(App.SelectedDate);
            EndDate = DateHelper.GetDateRangeEndDate(App.SelectedDate);
            LoadData();
        }


        public async Task PopulateAllVehicles()
        {
            AllVehicles = new ObservableCollection<VehicleItem>(await VehicleAPI.GetVehicles());
        }

        public async Task PopulateVehicleEventData()
        {
            if (Settings.Current.PlotFleetExceptions)
                VehicleEvents = await EventAPI.GetEventsByArrayAsync(selectedVehicle.Id, StartDate, EndDate, EventTypeGroup.FleetExceptions);
            else
                VehicleEvents = await EventAPI.GetEventsByArrayAsync(selectedVehicle.Id, StartDate, EndDate, EventTypeGroup.UBIExceptions);

            if (VehicleEvents == null || VehicleEvents.Count == 0)
            {
                ExceptionCount = MaxSpeed = MaxRPM = 0;
            }
            else
            {
                VehicleEvents = VehicleEvents.OrderByDescending(e => e.LocalTimestamp).ToList();
                ExceptionCount = VehicleEvents.Count();
                MaxSpeed = (int)VehicleEvents.Max(e => e.Speed);
                MaxRPM = (int)VehicleEvents.Max(e => e.RPM);

                RecentViolationOne = VehicleEvents.ElementAtOrDefault(0);
                RecentViolationTwo = VehicleEvents.ElementAtOrDefault(1);
                RecentViolationThree = VehicleEvents.ElementAtOrDefault(2);
                RecentViolationFour = VehicleEvents.ElementAtOrDefault(3);
            }
        }

        public async Task PopulateVehicleTripData()
        {
            Trips = await TripsAPI.GetTripsWithStats(selectedVehicle.Id, StartDate, EndDate);

            TotalDistance = (int)Math.Round(Trips.Sum(t => t.Distance));
            TotalDuration = (int)Math.Round(Trips.Sum(t => t.Duration) / 60 / 60);
            TripCount = Trips.Count();
        }

        public async Task FetchSelectedVehicle(string vehicleId)
        {
            SelectedVehicle = await VehicleAPI.GetVehicleById(vehicleId);
            LastKnownPosition = await VehicleAPI.GetLastKnownLocationById(vehicleId);
        }

        internal async Task LoadData()
        {
            await PopulateVehicleTripData();
            await PopulateFuelData();
            await PopulateVehicleEventData();
        }

        private async Task PopulateFuelData()
        {
            if (Trips.Count > 0 && Trips[0] != null)
            {
                LatestTripOne = await PopulateTripFuelConsumption(Trips[0]);
            }
            if (Trips.Count > 1 && Trips[1] != null)
            {
                LatestTripTwo = await PopulateTripFuelConsumption(Trips[1]);
            }
            if (Trips.Count > 2 && Trips[2] != null)
            {
                LatestTripThree = await PopulateTripFuelConsumption(Trips[2]);
            }
        }

        private async Task<TripFuelConsumption> PopulateTripFuelConsumption(Trip trip)
        {
            var tripFuelConsumption = new TripFuelConsumption();
            tripFuelConsumption.TimeStamp = trip.StartLocalTimestamp.LocalDateTime.ToString("HH:mm tt, dd MMM");

            var consumptionEvent = (await EventAPI.GetEventsById(trip.VehicleId, 172, trip.EndLocalTimestamp.AddMinutes(-10).UtcDateTime, trip.EndLocalTimestamp.AddMinutes(10).UtcDateTime, true))
                .FirstOrDefault(x => x.FuelData != null && x.FuelData.Reason == 2);

            if (consumptionEvent != default(Event))
            {
                tripFuelConsumption.FuelUsed =
                    $"{FuelHelper.CalculateFuelUsed(consumptionEvent.FuelData.AverageFuelConsumption.Value).ToString()} l";
            }
            else
            {
                tripFuelConsumption.FuelUsed = "-";
            }
            return tripFuelConsumption;
        }

        public ExtendedVehicle SelectedVehicle
        {
            set
            {
                if (selectedVehicle != value)
                {
                    selectedVehicle = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedVehicle"));
                }
            }
            get => selectedVehicle;
        }

        public UnitLocation LastKnownPosition
        {
            set
            {
                if (lastKnownPosition != value)
                {
                    lastKnownPosition = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LastKnownPosition"));
                }
            }
            get => lastKnownPosition;
        }

        public int ExceptionCount
        {
            set
            {
                if (exceptionCount != value)
                {
                    exceptionCount = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ExceptionCount"));
                }
            }
            get => exceptionCount;
        }

        public int TotalDistance
        {
            set
            {
                if (totalDistance != value)
                {
                    totalDistance = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalDistance"));
                }
            }
            get => totalDistance;
        }

        public int TripCount
        {
            set
            {
                if (tripCount != value)
                {
                    tripCount = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TripCount"));
                }
            }
            get => tripCount;
        }

        public int TotalDuration
        {
            set
            {
                if (totalDuration != value)
                {
                    totalDuration = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TotalDuration"));
                }
            }
            get => totalDuration;
        }

        public int MaxSpeed
        {
            set
            {
                if (maxSpeed != value)
                {
                    maxSpeed = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxSpeed"));
                }
            }
            get => maxSpeed;
        }

        public int MaxRPM
        {
            set
            {
                if (maxRPM != value)
                {
                    maxRPM = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MaxRPM"));
                }
            }
            get => maxRPM;
        }


        public Event RecentViolationOne
        {
            set
            {
                if (recentViolationOne != value)
                {
                    recentViolationOne = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RecentViolationOne"));
                }
            }
            get => recentViolationOne;
        }

        public Event RecentViolationTwo
        {
            set
            {
                if (recentViolationTwo != value)
                {
                    recentViolationTwo = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RecentViolationTwo"));
                }
            }
            get => recentViolationTwo;
        }


        public Event RecentViolationThree
        {
            set
            {
                if (recentViolationThree != value)
                {
                    recentViolationThree = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RecentViolationThree"));
                }
            }
            get => recentViolationThree;
        }


        public Event RecentViolationFour
        {
            set
            {
                if (recentViolationFour != value)
                {
                    recentViolationFour = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RecentViolationFour"));
                }
            }
            get => recentViolationFour;
        }


        public TripFuelConsumption LatestTripOne
        {
            set
            {
                if (latestTripOne != value)
                {
                    latestTripOne = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LatestTripOne"));
                }
            }
            get => latestTripOne;
        }
        public TripFuelConsumption LatestTripTwo
        {
            set
            {
                if (latestTripTwo != value)
                {
                    latestTripTwo = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LatestTripTwo"));
                }
            }
            get => latestTripTwo;
        }
        public TripFuelConsumption LatestTripThree
        {
            set
            {
                if (latestTripThree != value)
                {
                    latestTripThree = value;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LatestTripThree"));
                }
            }
            get => latestTripThree;
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}
