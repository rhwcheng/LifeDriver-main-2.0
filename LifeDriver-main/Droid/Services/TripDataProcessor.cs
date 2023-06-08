//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using StyexFleetManagement.Abstractions;
//using System.Threading.Tasks;
//using Plugin.Connectivity;

//namespace StyexFleetManagement.Services
//{
//    public class TripDataProcessor
//    {
//        static TripDataProcessor _tripDataProcessor;
//        readonly object sendDataLock = new object();

//        //IOT Hub state
//        //IHubIOT iotHub = new IOTHub();
//        bool isConnectedToObd;
//        bool isInitialized;
//        bool isPollingObdDevice;
//        bool isSendingBufferData;
//        Timer obdConnectionTimer;

//        //OBD Device state
//        //IOBDDevice obdDevice;
//        TimeSpan obdEllapsedTime;
//        IStoreManager storeManager;

//        private TripDataProcessor()
//        {
//            // The mobile app sends data to both the IOT Hub and the backend Mobile App Service:
//            //1.)	The Mobile App Service provides authentication and offline syncing between local storage and the backend database; the backend database is used to store trip, user, and OBD data in addition to data streamed by machine learning(ML).
//            //2.)	OBD data is read from the OBD device and pushed to the IOT Hub which performs analysis on the data; the resulting  ML data is then sent to the Mobile App Service’s backend database.
//            // The OBD processor is responsible for reading data from the OBD device and pushing it to the IOT Hub.
//        }

//        public bool IsObdDeviceSimulated { get; set; }

//        public static TripDataProcessor GetProcessor()
//        {
//            return _tripDataProcessor ?? (_tripDataProcessor = new TripDataProcessor());
//        }

//        //Init must be called each time to connect and reconnect to the OBD device
//        public async Task Initialize(IStoreManager storeManager)
//        {
//            //Ensure that initialization is only performed once
//            if (!isInitialized)
//            {
//                isInitialized = true;
//                this.storeManager = storeManager;

                

//                //Start listening for connectivity change event so that we know if connection is restablished\dropped when pushing data to the IOT Hub
//                CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;

//                //Check right away if there is any trip data left in the buffer that needs to be sent to the IOT Hub - run this thread in the background
//                SendBufferedDataToHub();
//            }
//        }

//        public async Task SendBufferedDataToHub(string tripId, string userId, TripPoint tripDataPoint)
//        {
//            //Note: Each individual trip point is being serialized separately so that it can be sent over as an individual message
//            //This is the expected format by the IOT Hub\ML
//            var settings = new JsonSerializerSettings { ContractResolver = new CustomContractResolver() };
//            var tripDataPointBlob = JsonConvert.SerializeObject(tripDataPoint, settings);

//            var tripBlob = JsonConvert.SerializeObject(
//                new
//                {
//                    TripId = tripId,
//                    UserId = userId
//                });

//            tripBlob = tripBlob.TrimEnd('}');
//            string packagedBlob = $"{tripBlob},\"TripDataPoint\":{tripDataPointBlob}}}";

//            if (!CrossConnectivity.Current.IsConnected)
//            {
//                //If there is no network connection, save in buffer and try again
//                Logger.Instance.Track("Unable to push data to IOT Hub - no network connection.");
//                await AddTripPointToBuffer(packagedBlob);
//                return;
//            }

//            try
//            {
//                await iotHub.SendEvent(packagedBlob);
//            }
//            catch (Exception e)
//            {
//                //An exception will be thrown if the data isn't received by the IOT Hub; store data in buffer and try again
//                Logger.Instance.Track("Unable to send data to IOT Hub: " + e.Message);
//                AddTripPointToBuffer(packagedBlob);
//            }
//        }

//        private async Task AddTripPointToBuffer(string tripDataPointBlob)
//        {
//            IOTHubData iotHubData = new IOTHubData { Blob = tripDataPointBlob };

//            await storeManager.IOTHubStore.InsertAsync(iotHubData);

//            //Try to sending buffered data to the IOT Hub in the background
//            SendBufferedDataToIOTHub();
//        }

//        /*
//        This task loops through the buffered data and attempts to resend it to the IOT Hub until the entire buffer is empty.
//        There are 3 places in this app where this task gets kicked off:
//            (1) The first attempt that we make to send a trip point to the IOT Hub, if this fails, we add the trip point to the buffer and immediately call this task to retry
//                sending buffered data to the IOT Hub. 
//            (2) When network connectivity changes from being disconnected to connected, we kick this task off since it is highly likely that there may be data in the buffer
//                since data cannot be sent to the IOT Hub when there is no connection.
//            (3) When we first launch the app we kick this task off to see if there is any data remaining in the buffer from previous times that the app may have been run.
//        */
//        private async Task SendBufferedDataToHub()
//        {
//            //Make sure that this thread can't be kicked off concurrently
//            lock (sendDataLock)
//            {
//                if (isSendingBufferData)
//                {
//                    return;
//                }
//                else
//                {
//                    isSendingBufferData = true;
//                }
//            }

//            var iotHubDataBlobs = new List<IOTHubData>(await storeManager.IOTHubStore.GetItemsAsync());

//            while (CrossConnectivity.Current.IsConnected && iotHubDataBlobs.Any())
//            {
//                try
//                {
//                    //Once all the data is pushed to the IOT Hub, delete it from the buffer
//                    //Note: This could still be pushing a bunch of data at once, but running in the background should make the performance impact of this unnoticable
//                    await iotHub.SendEvent(iotHubDataBlobs[0].Blob);
//                    await storeManager.IOTHubStore.RemoveAsync(iotHubDataBlobs[0]);
//                }
//                catch (Exception e)
//                {
//                    //An exception will be thrown if the data isn't received by the IOT Hub - wait a few seconds and try again
//                    Logger.Instance.Track("Unable to send buffered data to IOT Hub: " + e.Message);
//                    await Task.Delay(3000);
//                }
//                finally
//                {
//                    iotHubDataBlobs = new List<IOTHubData>(await storeManager.IOTHubStore.GetItemsAsync());
//                }
//            }

//            lock (sendDataLock)
//            {
//                isSendingBufferData = false;
//            }
//        }

//        private void Current_ConnectivityChanged(object sender,
//            Plugin.Connectivity.Abstractions.ConnectivityChangedEventArgs e)
//        {
//            if (e.IsConnected)
//            {
//                //If connection is re-established, then kick of background thread to push buffered data to IOT Hub
//                SendBufferedDataToHub();
//            }
//        }

        
        
        
//    }
//}