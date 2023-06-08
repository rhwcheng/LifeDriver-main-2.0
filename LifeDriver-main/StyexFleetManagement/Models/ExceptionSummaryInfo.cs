namespace StyexFleetManagement.Models
{
    public class ExceptionSummaryInfo
    {
        
        public string VehicleId
        {
            get => vehicleId;
            set => this.vehicleId = value;
        }

        public string VehicleDescription
        {
            get => vehicleDescription;
            set => this.vehicleDescription = value;
        }

        public string UnitId
        {
            get; set;
        }

        public string Registration
        {
            get => registration;
            set => this.registration = value;
        }

        public string Count
        {
            get => this.count;
            set => this.count = value;
        }

        public string MaxSpeed
        {
            get => maxSpeed;
            set => this.maxSpeed = value;
        }
        public string MaxRPM
        {
            get => maxRPM;
            set => this.maxRPM = value;
        }
        public string IdleDuration
        {
            get => idleDuration;
            set => this.idleDuration = value;
        }
        public string Distance
        {
            get => distance;
            set => this.distance = value;
        }
        public string Duration
        {
            get => duration;
            set => this.duration = value;
        }
        private string vehicleId;
        private string vehicleDescription;
        private string registration;
        private string count;
        private string maxSpeed;
        private string maxRPM;
        private string idleDuration;
        private string distance;
        private string duration;
    }
}
