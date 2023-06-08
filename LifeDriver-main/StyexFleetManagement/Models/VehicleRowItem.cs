namespace StyexFleetManagement.Models
{
    public class VehicleRowItem
    {
        public int Count { get; internal set; }
        public double Distance { get; internal set; }
        public int Duration { get; internal set; }
        public string GroupID { get; internal set; }
        public int IdleDuration { get; internal set; }
        public int? MaxRPM { get; internal set; }
        public double MaxSpeed { get; internal set; }
        public string Registration { get; internal set; }
        public string VehicleDescription { get; internal set; }
        public string VehicleId { get; internal set; }
    }
}
