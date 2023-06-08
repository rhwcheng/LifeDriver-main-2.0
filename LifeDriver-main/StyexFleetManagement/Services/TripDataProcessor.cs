using Acr.UserDialogs;
using Akavache;
using Plugin.Connectivity;
using StyexFleetManagement.Extensions;
using StyexFleetManagement.Models;
using StyexFleetManagement.Resx;
using StyexFleetManagement.TripLogging.Models.ProtoEvents;
using StyexFleetManagement.TripLogging.Models.ProtoEvents.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace StyexFleetManagement.Services
{
    public class TripDataProcessor
    {
        readonly object sendDataLock = new object();
        bool isSendingBufferData;
        /*
       This task loops through the buffered data and attempts to resend it to the IOT Hub until the entire buffer is empty.
       There are 3 places in this app where this task gets kicked off:
           (1) The first attempt that we make to send a trip point to the IOT Hub, if this fails, we add the trip point to the buffer and immediately call this task to retry
               sending buffered data to the IOT Hub. 
           (2) When network connectivity changes from being disconnected to connected, we kick this task off since it is highly likely that there may be data in the buffer
               since data cannot be sent to the IOT Hub when there is no connection.
           (3) When we first launch the app we kick this task off to see if there is any data remaining in the buffer from previous times that the app may have been run.
       */
        public async Task SendBufferedDataToHub()
        {
            //Make sure that this thread can't be kicked off concurrently
            lock (sendDataLock)
            {
                if (isSendingBufferData)
                {
                    return;
                }
                else
                {
                    isSendingBufferData = true;
                }
            }

            var hubDataBlobs = new List<TripBlob>(await BlobCache.LocalMachine.GetAllObjects<TripBlob>());

            while (Connectivity.NetworkAccess == NetworkAccess.Internet && hubDataBlobs.Any())
            {
                try
                {
                    //Once all the data is pushed to the IOT Hub, delete it from the buffer
                    //Note: This could still be pushing a bunch of data at once, but running in the background should make the performance impact of this unnoticable
                    await MProfiler.SendLogbookTrip(hubDataBlobs[0].Blob, hubDataBlobs[0].TemplateId, Settings.Current.DeviceId);

                    //await StyexFleetManagement.Services.Firebase.SaveData<byte[]>(hubDataBlobs[0].Blob, username);
                    await BlobCache.LocalMachine.InvalidateObject<TripBlob>(hubDataBlobs[0].Id.ToString());
                }
                catch (Exception e)
                {
                    //An exception will be thrown if the data isn't received by the IOT Hub - wait a few seconds and try again
                    //Logger.Instance.Track("Unable to send buffered data to Hub: " + e.Message);
                    await Task.Delay(3000);
                }
                finally
                {
                    hubDataBlobs = new List<TripBlob>(await BlobCache.LocalMachine.GetAllObjects<TripBlob>());
                }
            }

            lock (sendDataLock)
            {
                isSendingBufferData = false;
            }
        }

        public async Task AddTripPointToBuffer(byte[] tripDataPointBlob, uint templateId)
        {
            TripBlob hubData = new TripBlob { Blob = tripDataPointBlob, TemplateId = templateId };

            await BlobCache.LocalMachine.InsertObject<TripBlob>(hubData.Id, hubData);

            //Try to sending buffered data to the IOT Hub in the background
            SendBufferedDataToHub();
        }

        public async Task SendTripPointToHub(TripEventType eventType, TripPoint tripDataPoint, LogbookTrip logbookTrip = null, bool isIgnitionOn = true)
        {
            //Note: Each individual trip point is being serialized separately so that it can be sent over as an individual message

            //var settings = new JsonSerializerSettings { ContractResolver = new CustomContractResolver() };
            //var tripDataPointBlob = JsonConvert.SerializeObject(tripDataPoint, settings);
            object tripEvent = new object();
            EventHeader header;
            try
            {
                if (tripDataPoint != null)
                {
                    uint speed, direction, templateId, generalStatus;
                    speed = ConvertToUint(tripDataPoint.Speed);
                    direction = ConvertToUint(tripDataPoint.Bearing);
                    templateId = ConvertToUint((int)eventType);

                    if (isIgnitionOn)
                        generalStatus = (uint)GeneralStatusType.GENERALSTATUSIGNITION + (uint)GeneralStatusType.GENERALSTATUSGPSSTATUSVALID + (uint)GeneralStatusType.GENERALSTATUSGPSINFORMATIONAVAILABLE;
                    else
                        generalStatus = (uint)GeneralStatusType.GENERALSTATUSGPSSTATUSVALID + (uint)GeneralStatusType.GENERALSTATUSGPSINFORMATIONAVAILABLE;

                    header = new EventHeader
                    {
                        Description = eventType.ToString(),
                        Longitude = tripDataPoint.Longitude,
                        Latitude = tripDataPoint.Latitude,
                        UtcTimestampSeconds = tripDataPoint.RecordedTimeStamp.ToUnixSeconds(),
                        Speed = speed,
                        Direction = direction,
                        TemplateId = templateId,
                        GeneralStatus = generalStatus
                    };

                    if (logbookTrip != null)
                    {
                        header.DriverKeyCode = logbookTrip.DriverId;
                        header.UnitId = logbookTrip.UnitId;
                    }
                    else
                    {
                        header.DriverKeyCode = default(uint);
                        header.UnitId = Settings.Current.DeviceId;
                    }
                }

                else
                    header = new EventHeader();

            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, e.Message);
                ToastConfig cfg = new ToastConfig(AppResources.error_label);
                cfg.Duration = TimeSpan.FromSeconds(0.4);

                Device.BeginInvokeOnMainThread(() =>
                {
                    UserDialogs.Instance.Toast(cfg);

                });
                return;
            }


            switch (eventType)
            {
                case (TripEventType.TripStart):
                    tripEvent = new TripStartup { Header = header, TripId = logbookTrip.TripId };
                    break;
                case (TripEventType.TripEnd):
                    if (logbookTrip == null)
                    {
                        ToastConfig cfg = new ToastConfig(AppResources.error_label);
                        cfg.Duration = TimeSpan.FromSeconds(0.4);

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            UserDialogs.Instance.Toast(cfg);

                        });
                        return;
                    }
                    tripEvent = new TripShutdown { Header = header, TripId = logbookTrip.TripId, TripDistanceMeters = Convert.ToUInt32(logbookTrip.Distance * 1000), TripDurationSeconds = Convert.ToUInt32(logbookTrip.Duration.TotalSeconds) };
                    break;
                case (TripEventType.PolledPosition):
                    tripEvent = new PolledPosition { Header = header };
                    break;
                case (TripEventType.Tachograph):
                    tripEvent = new TachographData { Header = header, Rpm = 0 };
                    break;
                case (TripEventType.PeriodicPosition):
                    tripEvent = new PeriodicPosition { Header = header, TripDistanceMeters = Convert.ToUInt32(logbookTrip.Distance * 1000), TripDurationSeconds = Convert.ToUInt32(logbookTrip.Duration.TotalSeconds) };
                    break;
                case (TripEventType.TripSummary):
                    if (logbookTrip == null)
                    {
                        ToastConfig cfg = new ToastConfig(AppResources.error_label);
                        cfg.Duration = TimeSpan.FromSeconds(0.4);

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            UserDialogs.Instance.Toast(cfg);

                        });
                        return;
                    }
                    tripEvent = new TripSummary { Header = header, LatitudeStart = logbookTrip.StartLatitude, LongitudeStart = logbookTrip.StartLongitude, TripMaxSpeed = Convert.ToUInt32(logbookTrip.Points.Max(x => x.Speed)), TripDistance = Convert.ToUInt32(logbookTrip.Distance * 1000), TripDuration = Convert.ToUInt32(logbookTrip.Duration.TotalSeconds) };
                    break;
                case (TripEventType.DriverRegistered):
                    if (logbookTrip == null)
                    {
                        ToastConfig cfg = new ToastConfig(AppResources.error_label);
                        cfg.Duration = TimeSpan.FromSeconds(0.4);

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            UserDialogs.Instance.Toast(cfg);

                        });
                        return;
                    }
                    tripEvent = new DriverRegistered { Header = header, DriverId = logbookTrip.DriverId };
                    break;
            }

            //var tripBlob = JsonConvert.SerializeObject(
            //    new
            //    {
            //        TripId = tripId,
            //        UserId = userId
            //    });

            //var tripBlob = JsonConvert.SerializeObject(tripEvent);

            //tripBlob = tripBlob.TrimEnd('}');
            //string packagedBlob = $"{tripBlob},\"TripDataPoint\":{tripDataPointBlob}}}";

            // serialize event
            byte[] eventData;
            using (var ms = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(ms, tripEvent);
                eventData = ms.ToArray();
            }
            //string base64 = Convert.ToBase64String(eventData);

            //try
            //{
            //    TripStartup msgOut;

            //    using (var stream = new MemoryStream(eventData))
            //    {
            //        msgOut = Serializer.Deserialize<TripStartup>(stream);
            //    }
            //}
            //catch (Exception e)
            //{
            //    Debug.WriteLine(e);
            //}


            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                //If there is no network connection, save in buffer and try again
                //Logger.Instance.Track("Unable to push data to IOT Hub - no network connection.");
                await this.AddTripPointToBuffer(eventData, (uint)((int)eventType));
                return;
            }

            try
            {
                var username = DependencyService.Get<ICredentialsService>().MZoneUserName;
                var success = await MProfiler.SendLogbookTrip(eventData, (uint)((int)eventType), Settings.Current.DeviceId);
                if (!success)
                    this.AddTripPointToBuffer(eventData, (uint)((int)eventType));
            }
            catch (Exception e)
            {
                //An exception will be thrown if the data isn't received by the IOT Hub; store data in buffer and try again
                //Logger.Instance.Track("Unable to send data to IOT Hub: " + e.Message);
                this.AddTripPointToBuffer(eventData, (uint)((int)eventType));
            }
        }

        private uint ConvertToUint(object value)
        {
            uint converted;

            if (value == null)
                return default(uint);

            try
            {
                converted = Convert.ToUInt32(value);
            }
            catch (OverflowException overFlow)
            {
                Serilog.Log.Error(overFlow, overFlow.Message);
                converted = default(uint);
            }
            return converted;
        }
    }

    public class TripEndBlob
    {
        public TripEventType EventType { get; set; }
        public TripPoint Point { get; set; }
        public LogbookTrip Trip { get; set; }
    }
}