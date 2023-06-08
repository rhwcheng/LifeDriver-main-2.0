//using System;
//using System.Collections.Generic;
//using System.Linq;
//using SQLite;
//using Xamarin.Forms;

//namespace StyexFleetManagement
//{
//	public class FavouriteReportDatabase
//	{
//		static object locker = new object();

//		SQLiteConnection database;

//		public FavouriteReportDatabase()
//		{
//			database = DependencyService.Get<ISQLite>().GetConnection();
//			database.CreateTable<FavouriteReport>();
//		}

//		public IEnumerable<FavouriteReport> GetItems()
//		{
//            try
//            {

//                return (from i in database.Table<FavouriteReport>() select i).ToList();
//            }
//            catch(Exception e)
//            {
//                return null;
//            }
//		}

//		public FavouriteReport GetItem(int id)
//		{
//			return database.Table<FavouriteReport>().FirstOrDefault(x => x.Id == id);
//		}

//        public int IsFavourite(ReportDateRange selectedDate, ReportType reportType, string vehicleGroup)
//        {
//            var item = database.Table<FavouriteReport>().FirstOrDefault(x => x.ReportType == reportType && x.VehicleGroupId == vehicleGroup && x.DateRange == selectedDate);

//            if (item == null)
//                return -1;
//            else
//                return item.Id;
//        }

//        public int SaveItem(FavouriteReport item)
//		{
//			lock (locker)
//			{
//				if (item.Id != 0)
//				{
//					database.Update(item);
//					return item.Id;
//				}
//				else {
//					var id= database.Insert(item);
//                    return id;
//				}
//			}
//		}

//		public int DeleteItem(int id)
//		{
//			lock (locker)
//			{
//				return database.Delete<FavouriteReport>(id);
//			}
//		}
//	}
//}
