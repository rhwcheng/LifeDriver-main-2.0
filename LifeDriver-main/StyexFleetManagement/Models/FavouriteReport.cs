//using SQLite;
//using System;
//namespace StyexFleetManagement
//{
//    [Table("Favourites")]
//    public class FavouriteReport
//	{
//        [PrimaryKey, AutoIncrement, Column("_id")]
//        public int Id { get; set; }
//		public String Title { get; set; }
//		public int OrderNo { get; set; }
//		public ReportType ReportType { get; set; }
//		public int SelectVehicleIndex { get; set; }
//		public int SelectedDateIndex { get; set; }
//		public ReportDateRange DateRange { get; set; }
//		public DateTime StartDate { get; set; }
//		public DateTime EndDate { get; set; }
//		public string VehicleGroupId { get; set; }

//		public FavouriteReport()
//		{

//		}

//		public FavouriteReport(ReportParameters parameters)
//		{
//			DateRange = parameters.DateRange;
//			StartDate = parameters.StartDate;
//			EndDate = parameters.EndDate;
//			VehicleGroupId = parameters.VehicleGroupId;
//		}
//	}

//}

