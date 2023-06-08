using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
    public class VehiclesResponse
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
        public List<VehicleItem> Items { get; set; }
        public bool HasMoreResults { get; set; }
    }

    public class VehicleItem
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string UtcLastModified { get; set; }
    }

}
