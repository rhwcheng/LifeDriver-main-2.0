using System;
using StyexFleetManagement.Services;

namespace StyexFleetManagement.Models
{
	public class ReportParameters
	{
		private static long serialVersionUID = 1L;

		public ReportDateRange DateRange { get; set;}
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string VehicleGroupId { get; set; }

		public ReportParameters()
		{
		}

		public ReportParameters(App app)
		{
			/*DateRange = app.getReportDateRange();
			StartDate = app.getActualReportStartDate();
			EndDate = app.getActualReportEndDate();
			VehicleGroupId = app.getReportVehicleGroupId();
			*/
		}
	}
}