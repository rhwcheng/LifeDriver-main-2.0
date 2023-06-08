namespace StyexFleetManagement.Models
{
    public class ExtendedVehicle
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Registration { get; set; }
        public bool IsFavourite { get; set; }
        public string UtcLastUpdate { get; set; }
        public string Unit_Id { get; set; }
        public string VehicleType_Id { get; set; }
        public string UtcStartDate { get; set; }
        public object UtcEndDate { get; set; }
        public int ModelYear { get; set; }
        public string CofDueDate { get; set; }
        public string PurchaseDate { get; set; }
        public object Notes { get; set; }
        public bool IsMRMEnabled { get; set; }
        public bool IsFuelReportingEnabled { get; set; }
        public double MonthlyDistanceLimit { get; set; }
        public string UtcLastModified { get; set; }
        public int Make_Id { get; set; }
        public string Make { get; set; }
    }
}
