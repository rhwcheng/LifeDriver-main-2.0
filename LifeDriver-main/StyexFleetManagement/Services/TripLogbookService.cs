
//using StyexFleetManagement.Entities.ProtoEvents;
//using StyexFleetManagement.Extensions;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace StyexFleetManagement.Services
//{
//    public static class TripLogbookService
//    {
//        public static async void SendTripStartEvent(DateTime startTime, double lattitude, double longitude, string unitId)
//        {
//            TripStartup tripStart = new TripStartup();

//            tripStart.Header.Latitude = lattitude;
//            tripStart.Header.Longitude = longitude;
//            tripStart.Header.UnitId = unitId;
//            tripStart.Header.UtcTimestampSeconds = startTime.ToUnixSeconds();

//            await Firebase.SaveData<TripStartup>(tripStart, "TripStart");
//        }
//    }
//}
