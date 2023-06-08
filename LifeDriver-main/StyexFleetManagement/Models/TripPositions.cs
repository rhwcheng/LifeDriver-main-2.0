using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
	public class TripPositions
	{
		public int Page { get; set; }
		public int PageSize { get; set; }
		public int TotalResults { get; set; }
		public List<List<float>> Items { get; set; }
		public bool HasMoreResults { get; set; }
    }
}

