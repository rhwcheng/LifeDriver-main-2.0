using System;

namespace StyexFleetManagement.Models
{
    public class LocationArgs
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Bearing { get; set; }
        public double Speed { get; set; }
        public double BatteryLevel { get; set; }
        public double Altitude { get; set; }
        public float Accuracy { get; set; }
        public long Time { get; set; }
        public bool IsPeriodicUpdate { get; set; }
    }

    public static class CoordinatesDistanceExtensions
    {
        public static double DistanceTo(this LocationArgs baseCoordinates, LocationArgs targetCoordinates)
        {
            return DistanceTo(baseCoordinates, targetCoordinates, UnitOfLength.Kilometers);
        }

        public static double DistanceTo(this LocationArgs baseCoordinates, LocationArgs targetCoordinates, UnitOfLength unitOfLength)
        {
            var baseRad = Math.PI * baseCoordinates.Latitude / 180;
            var targetRad = Math.PI * targetCoordinates.Latitude / 180;
            var theta = baseCoordinates.Longitude - targetCoordinates.Longitude;
            var thetaRad = Math.PI * theta / 180;

            double dist =
                Math.Sin(baseRad) * Math.Sin(targetRad) + Math.Cos(baseRad) *
                Math.Cos(targetRad) * Math.Cos(thetaRad);
            dist = Math.Acos(dist);

            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            return unitOfLength.ConvertFromMiles(dist);
        }
    }

    public static class LocationArgsSpeedExtensions
    {
        public static double ConvertSpeed(this LocationArgs baseLocation)
        {
            return ConvertSpeed(baseLocation, UnitofSpeed.KmPerHour);
        }

        public static double ConvertSpeed(this LocationArgs baseLocation, UnitofSpeed unitOfSpeed)
        {
            return unitOfSpeed.ConvertFromMetersPerSecond(baseLocation.Speed);
        }
    }

    public class UnitOfLength
    {
        public static UnitOfLength Kilometers = new UnitOfLength(1.609344);
        public static UnitOfLength NauticalMiles = new UnitOfLength(0.8684);
        public static UnitOfLength Miles = new UnitOfLength(1);

        private readonly double _fromMilesFactor;

        private UnitOfLength(double fromMilesFactor)
        {
            _fromMilesFactor = fromMilesFactor;
        }

        public double ConvertFromMiles(double input)
        {
            return input * _fromMilesFactor;
        }
    }
    public class UnitofSpeed
    {
        public static UnitofSpeed KmPerHour = new UnitofSpeed(3.6);
        public static UnitofSpeed MilesPerHour = new UnitofSpeed(2.2369363);

        private readonly double _fromMetersPerSecondFactor;

        private UnitofSpeed(double fromMetersPerSecondFactor)
        {
            _fromMetersPerSecondFactor = fromMetersPerSecondFactor;
        }

        public double ConvertFromMetersPerSecond(double input)
        {
            return input * _fromMetersPerSecondFactor;
        }
    }
}
