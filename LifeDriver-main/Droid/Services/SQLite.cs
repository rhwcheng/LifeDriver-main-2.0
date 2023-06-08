//using System;
//using System.IO;
//using StyexFleetManagement.Droid;

//[assembly: Xamarin.Forms.Dependency(typeof(SQLite_Android))]
//namespace StyexFleetManagement.Droid
//{
	
//	public class SQLite_Android : ISQLite
//	{
//		public SQLite_Android() { }
//		public SQLite.SQLiteConnection GetConnection()
//		{
//			var sqliteFilename = "FavouriteReports.db3";
//			string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
//			var path = Path.Combine(documentsPath, sqliteFilename);
//			// Create the connection
//			var conn = new SQLite.SQLiteConnection(path);
//			// Return the database connection
//			return conn;
//		}

//	}
//}
