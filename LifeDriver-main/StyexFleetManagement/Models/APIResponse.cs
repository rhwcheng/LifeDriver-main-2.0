using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
    public class APIResponse<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalResults { get; set; }
        public List<T> Items { get; set; }
        public bool HasMoreResults { get; set; }
    }
}
