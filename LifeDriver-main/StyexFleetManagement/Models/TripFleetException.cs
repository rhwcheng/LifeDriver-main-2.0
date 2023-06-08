using System;
using System.Collections.Generic;

namespace StyexFleetManagement.Models
{
	public class TripFleetExceptionList
	{
		public int Page { get; set; }
		public int PageSize { get; set; }
		public int TotalResults { get; set; }
		public List<TripFleetException> Items { get; set; }
		public bool HasMoreResults { get; set; }

		public TripFleetExceptionList()
		{
			Items = new List<TripFleetException>();
		}
	}

	public class TripFleetException
	{
		public DateTime LocalTimestamp { get; set; }
		public List<float> Position { get; set; }
		public string Description { get; set; }
		public int EventTypeId { get; set; }

		public TripFleetException()
		{
			Position = new List<float>();
		}
	}

}