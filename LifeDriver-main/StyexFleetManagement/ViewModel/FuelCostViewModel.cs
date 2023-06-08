using StyexFleetManagement.Helpers;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StyexFleetManagement.Models;

namespace StyexFleetManagement.ViewModel
{
    public class FuelCostInfo
    {
        public string VehicleId { get; set; }

        public string VehicleDescription { get; set; }
        public string FuelSource { get; set; }
        public string FuelConsumption { get; set; }
        public double FuelConsumptionValue { get; set; }
        public List<Event> FuelData { get; internal set; }
        
        public string Registration { get; set; }
        public string Consumption { get; set; }
        public string MaxSpeed { get; set; }
        public string Distance { get; set; }
        public string Duration { get; set; }
    }

    public class FuelCostViewModel
    {
        private List<AggregatedTripData> AggregatedTripData;

        public DateTime EndDate { get; internal set; }
        public List<Event> FuelData { get; internal set; }
        public DateTime StartDate { get; internal set; }
        public List<MonthlyFuelConsumption> GroupMonthlyConsumption { get; internal set; }
        public SemaphoreSlim Throttler { get; private set; }
        public List<ConsumptionItem> FuelVolumeData { get; private set; }
        public List<ConsumptionItem> FuelCostData { get; private set; }
        public ObservableCollection<FuelCostInfo> FuelCostInfoCollection { get; internal set; }
        public string SelectedVehicleGroup { get; private set; }

        public FuelCostViewModel()
        {
            //SelectedDate = App.SelectedDate;
            StartDate = App.StartDateSelected;
            EndDate = App.EndDateSelected;

            Throttler = new SemaphoreSlim(initialCount: 50);

            FuelData = new List<Event>();
            GroupMonthlyConsumption = new List<MonthlyFuelConsumption>();
            FuelVolumeData = new List<ConsumptionItem>();
            FuelCostInfoCollection = new ObservableCollection<FuelCostInfo>();
        }

        internal double CalculateConsumption(DateTime startDuration, DateTime endDuration)
        {
            double total = 0;
            if (FuelData == null)
                return total;

            var tempData = FuelData.Where(x => x.LocalTimestamp >= startDuration && x.LocalTimestamp <= endDuration).OrderBy(x => x.LocalTimestamp).ToList();

            for (int i =0; i < tempData.Count-1; i++)
            {
                
                if ((FuelEventReason)tempData[i].FuelData.Reason == FuelEventReason.End_of_trip)
                {
                    continue;
                }

                if ((FuelEventReason)tempData[i].FuelData.Reason == FuelEventReason.Start_of_trip)
                {
                    if ((FuelEventReason)tempData[i + 1].FuelData.Reason == FuelEventReason.End_of_trip || (FuelEventReason)tempData[i + 1].FuelData.Reason == FuelEventReason.Fuel_Theft)
                        total += tempData[i + 1].FuelData.FuelLevel.Value - tempData[i].FuelData.FuelLevel.Value;
                }
                if ((FuelEventReason)tempData[i].FuelData.Reason == FuelEventReason.Fuel_Theft)
                {
                    if ((FuelEventReason)tempData[i + 1].FuelData.Reason == FuelEventReason.End_of_trip)
                        total += tempData[i + 1].FuelData.FuelLevel.Value - tempData[i].FuelData.FuelLevel.Value;
                }
            }

            return total;
        }

        internal async Task SetTableData(bool orderByMostEfficient)
        {
            FuelCostInfoCollection.Clear();

            var result = FuelData.GroupBy(x => x.UnitId).Select(grp => new FuelCostInfo { VehicleId = grp.Where(x=>x.VehicleId != null).FirstOrDefault().VehicleId, VehicleDescription = grp.First().UnitId, FuelData = grp.Where(x => x.FuelData != null && x.FuelData.Reason == 2).ToList()}); //Reason code 2 - trip shutdown

            foreach (var vehicle in result)
            {
                if (vehicle.VehicleDescription != default(string))
                    vehicle.VehicleDescription = await VehicleHelper.GetVehicleDescriptionFromIdAsync(vehicle.VehicleId, SelectedVehicleGroup);

                var consumptionData = vehicle.FuelData.Where(x => x.FuelData.AverageFuelConsumption != null).ToList();

                //double totalConsumption = consumptionData.Sum(x => x.FuelData.AverageFuelConsumption.Value * x.FuelData.TripDistance)/100000000;
                //double totalDistance = consumptionData.Sum(x => x.FuelData.TripDistance)/1000;
                //double consumption = totalConsumption / (totalDistance/100);
                var fuelUsed = consumptionData.Sum(x => x.FuelData.AverageFuelConsumption.Value);
                var totalDistance = consumptionData.Sum(x => x.FuelData.TripDistance);
                double consumption = FuelHelper.CalculateFuelConsumption(fuelUsed, totalDistance);

                //foreach (Event item in consumptionData)
                //{
                //    var itemConsumption = (item.FuelData.InstantaneousFuelConsumption.Value / item.FuelData.TripDistance);
                //    consumption += itemConsumption;
                //}

                var temp = DateTime.Today.AddMonths(-6);

                for (int i = 0; i < 6; i++)
                {
                    var ty = consumptionData.Where(x => x.LocalTimestamp > temp.AddMonths(i) && x.LocalTimestamp < temp.AddMonths(i + 1)).ToList();
                    double atotalConsumption = consumptionData.Where(x=>x.LocalTimestamp > temp.AddMonths(i) && x.LocalTimestamp < temp.AddMonths(i+1)).Sum(x => x.FuelData.AverageFuelConsumption.Value) / 1000;
                    double atotalDistance = consumptionData.Where(x => x.LocalTimestamp > temp.AddMonths(i) && x.LocalTimestamp < temp.AddMonths(i+1)).Sum(x => x.FuelData.TripDistance) / 1000;
                    double aconsumption = atotalConsumption / (atotalDistance / 100);

                    Debug.WriteLine(vehicle.VehicleDescription + ": Consump: " + temp.AddMonths(i).ToString("MMM") +" - " +
                                    $"{Math.Round(aconsumption, 2).ToString()} l/100km");
                }

                if (double.IsNaN(consumption))
                {
                    vehicle.FuelConsumption = "N/A";
                    vehicle.FuelConsumptionValue = 0;
                }
                else
                {
                    vehicle.FuelConsumption = $"{consumption.ToString()} l/100km";
                    vehicle.FuelConsumptionValue = consumption;
                }

                var fuelSourceList = consumptionData.OrderByDescending(x=>x.LocalTimestamp);

                if (fuelSourceList.FirstOrDefault() != default(Event))
                {
                    var fuelSource = fuelSourceList.FirstOrDefault().FuelData.AverageFuelConsumption.Source;

                    switch (fuelSource)
                    {
                        case (Source.CALC_CO2):
                            vehicle.FuelSource = AppResources.label_co2;
                            break;
                        case (Source.CANBus):
                            vehicle.FuelSource = AppResources.label_canbus;
                            break;
                        case (Source.Fuel_Meter):
                            vehicle.FuelSource = AppResources.label_fuel_meter;
                            break;
                        case (Source.Mass_Airflow):
                            vehicle.FuelSource = AppResources.label_mass_airflow;
                            break;
                        case (Source.None):
                            vehicle.FuelSource = AppResources.label_none;
                            break;
                        case (Source.OBDII):
                            vehicle.FuelSource = AppResources.label_obdii;
                            break;
                        case (Source.RFE):
                            vehicle.FuelSource = AppResources.label_rfe;
                            break;
                        default:
                            vehicle.FuelSource = AppResources.label_none;
                            break;
                    }
                }
                else
                {
                    vehicle.FuelSource = AppResources.entries;
                }

                var extendedVehicle = await VehicleAPI.GetVehicleById(vehicle.VehicleId);
                if (extendedVehicle != null)
                    vehicle.Registration = extendedVehicle.Registration;

                vehicle.Distance =
                    $"{Math.Round(AggregatedTripData.Where(x => x.VehicleId == vehicle.VehicleId).First().TotalDistance, 2).ToString()} {App.GetDistanceAbbreviation()}";
                vehicle.Duration = FormatHelper.ToShortForm(TimeSpan.FromSeconds(AggregatedTripData.Where(x => x.VehicleId == vehicle.VehicleId).First().TotalDuration));

                //Get Max speed
                var trips = await TripsAPI.GetTripsWithStats(vehicle.VehicleId, StartDate, EndDate);
                var maxSpeed = trips.Max(x => x.MaxSpeed);
                vehicle.MaxSpeed = $"{Math.Round(maxSpeed, 2).ToString()} {App.GetDistanceAbbreviation() + "/h"}";
                FuelCostInfoCollection.Add(vehicle);
            }

            if (orderByMostEfficient)
                FuelCostInfoCollection = new ObservableCollection<FuelCostInfo>(FuelCostInfoCollection.OrderBy(x => x.FuelConsumptionValue).ThenBy(x => x.VehicleDescription));
            else
                FuelCostInfoCollection = new ObservableCollection<FuelCostInfo>(FuelCostInfoCollection.OrderByDescending(x => x.FuelConsumptionValue).ThenBy(x => x.VehicleDescription));
            return;
        }

        internal async Task GetData(string selectedVehicleGroup)
        {
            this.SelectedVehicleGroup = selectedVehicleGroup;
            var vehicles = await VehicleAPI.GetVehicleInGroupAsync(selectedVehicleGroup);
            var allTasks = new List<Task>();

            List<Event> events = new List<Event>();
            List<MonthlyFuelConsumption> groupMonthlyConsumption = new List<MonthlyFuelConsumption>();
            foreach (VehicleItem vehicle in vehicles)
            {
                await Throttler.WaitAsync();
                allTasks.Add(
                            Task.Run(async () =>
                            {
                                try
                                {
                                    List<Event> data;

                                    data = await EventAPI.GetEventsById(vehicle.Id.ToString(), 172, this.StartDate, this.EndDate);
                                    
                                    foreach (var point in data) //Fix bad Json data
                                    {
                                        point.VehicleId = vehicle.Id.ToString();
                                    }

                                    events = events.Concat(data.Where(x => x.FuelData != null &&x.FuelData.AverageFuelConsumption != null && x.FuelData.Reason == 2)).ToList();


                                }
                                finally
                                {
                                    Throttler.Release();
                                }
                            }));

            }


            await Task.WhenAll(allTasks);

            this.FuelData = events;
            this.GroupMonthlyConsumption = groupMonthlyConsumption;
            
            //Get Fuel Volume data
            //FuelVolumeData = await FuelAPI.GetFuelVolumeAsync(selectedVehicleGroup, StartDate, EndDate, GroupByPeriod.bymonth.ToString());

            //Get Fuel Consumption Data
            FuelCostData = await FuelAPI.GetFuelCostAsync(selectedVehicleGroup, StartDate, EndDate, GroupByPeriod.bymonth.ToString());


            //Get Aggregated trip data
            AggregatedTripData = await TripsAPI.GetAggregatedTripDataAsync(selectedVehicleGroup, StartDate, EndDate);
        }

        public double GetTotalMonthlyFuelCost(DateTime date)
        {
            if (FuelCostData == null)
                return 0;

            var value = FuelCostData.Where(x => x.PeriodStart.Year == date.Year && x.PeriodStart.Month == date.Month).FirstOrDefault();

            if (value != null)
                return value.Value;

            else
                return 0;

        }
        public double GetTotalMonthlyFuelConsumption(DateTime date)
        {
            var monthStartDate = date;
            var monthEndDate = date.AddMonths(1);

            if (FuelData == null)
                return 0;

            var value = FuelData.Where(x => x.LocalTimestamp >= monthStartDate && x.LocalTimestamp < monthEndDate);

            if (value != null && value.Count() > 0)
                return FuelHelper.CalculateFuelUsed(value.Sum(x => x.FuelData.AverageFuelConsumption.Value));
            else
                return 0;
        }
    }

}
