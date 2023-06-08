////using SQLite;
//using StyexFleetManagement.Entities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Xamarin.Forms;

//namespace StyexFleetManagement
//{
//    public class TripLogDatabase
//    {
//        //static object locker = new object();

//        //SQLiteConnection database;

//        //public TripLogDatabase()
//        //{
//        //    database = DependencyService.Get<ISQLite>().GetConnection();
//        //    database.CreateTable<LogbookTrip>();
//        //}

//        //public IEnumerable<LogbookTrip> GetItems()
//        //{
//        //    try
//        //    {

//        //        return (from i in database.Table<LogbookTrip>() select i).ToList();
//        //    }
//        //    catch (Exception e)
//        //    {
//        //        return null;
//        //    }
//        //}

//        //public LogbookTrip GetItem(int id)
//        //{
//        //    return database.Table<LogbookTrip>().FirstOrDefault(x => x.Id == id);
//        //}

        
//        //public int SaveItem(LogbookTrip item)
//        //{
//        //    lock (locker)
//        //    {
//        //        if (item.Id != 0)
//        //        {
//        //            database.Update(item);
//        //            return item.Id;
//        //        }
//        //        else
//        //        {
//        //            var id = database.Insert(item);
//        //            return id;
//        //        }
//        //    }
//        //}

//        //public int DeleteItem(int id)
//        //{
//        //    lock (locker)
//        //    {
//        //        return database.Delete<LogbookTrip>(id);
//        //    }
//        //}
//    }
//}
