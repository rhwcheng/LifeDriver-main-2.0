using System;

namespace StyexFleetManagement.Models
{
    public class FuelLevel
    {
        public Source Source { get; set; }
        public FuelLevelUnitOfMeasure UnitOfMeasure { get; set; }
        public double Value { get; set; }
    }
    public class InstantaneousFuelConsumption
    {
        public Source Source { get; set; }
        public FuelConsumptionUnitOfMeasure UnitOfMeasure { get; set; }
        public double Value { get; set; }
    }

    public class AverageFuelConsumption
    {
        public Source Source { get; set; }
        public FuelConsumptionUnitOfMeasure UnitOfMeasure { get; set; }
        public double Value { get; set; }
    }
    public class FuelData
    {
        public int Reason { get; set; }
        public FuelLevel FuelLevel { get; set; }
        public int AmbientTemperature { get; set; }
        public double Altitude { get; set; }
        public double TripDistance { get; set; }
        public int TripDuration { get; set; }
        public InstantaneousFuelConsumption InstantaneousFuelConsumption { get; set; }
        public AverageFuelConsumption AverageFuelConsumption { get; set; }
    }

    public static class FuelHelper
    {
        public static double CalculateFuelConsumption(double fuelUsed, double distance)
        {
            if (distance <= 0)
                return 0;

            return Math.Round((((fuelUsed / 1000) / Math.Round(distance/1000,2)) * 100),2);
        }

        public static double CalculateFuelUsed(double fuelUsed)
        {
            return Math.Round(fuelUsed / 1000, 2);
        }
    }

    public enum Source
    {
        None = 0,
        CANBus = 1,
        OBDII = 2,
        RFE = 3,
        Fuel_Meter = 4,
        CALC_CO2 = 5,
        Mass_Airflow = 6
    }
    public enum FuelLevelUnitOfMeasure
    {
        Percentage = 0,
        Volume = 1,
        Mass = 2
    }
    public enum FuelConsumptionUnitOfMeasure
    {
        Volume_per_distance_unit = 0,
        Volume_per_hour = 1,
        Mass_per_distance_unit = 2,
        Mass_per_hour = 3,
        Kilowatts_per_hour = 4
    }

    public enum FuelEventReason
    {
        Periodic = 0,
        Start_of_trip = 1,
        End_of_trip = 2,
        Fuel_Theft = 3,
        Fuel_Level_low = 4,
        Polled = 5,
        Consumption_High = 6
    }
}
