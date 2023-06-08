using System;
using System.Linq;
using Shiny.Beacons;
using StyexFleetManagement.ViewModel.Base;
using UniversalBeacon.Library.Core.Entities;
using Beacon = UniversalBeacon.Library.Core.Entities.Beacon;

namespace StyexFleetManagement.ViewModel
{
    public class BeaconViewModel : ViewModelBase
    {
        private string _proximity;

        public string Address { get; set; }
        public string Identifier { get; set; }
        public string Proximity
        {
            get => _proximity;
            set
            {
                _proximity = value;
                RaisePropertyChanged(() => Proximity);
            }
        }

        public BeaconViewModel(Beacon beacon)
        {
            var proximityFrame = beacon.BeaconFrames.OfType<ProximityBeaconFrame>().FirstOrDefault();
            Address = beacon.BluetoothAddressAsString;
            Identifier = $"ID: {proximityFrame?.Minor.ToString() ?? "Unknown"}";
            SetProximity(beacon);
        }

        public static string CalculateProximity(int txPower, double rssi)
        {
            var distance = Math.Pow(10d, (txPower * -1 - rssi * -1) / 20);

            if (distance >= 6E-6d)
                return "Far";

            if (distance > 0.5E-6d)
                return "Near";

            return "Immediate";
        }

        public void SetProximity(Beacon beacon)
        {
            var proximityFrame = beacon.BeaconFrames.OfType<ProximityBeaconFrame>().FirstOrDefault();
            Proximity = proximityFrame != default && proximityFrame.TxPower != default ? $"Proximity: {CalculateProximity(Convert.ToInt32(proximityFrame.TxPower), beacon.Rssi)}" : string.Empty;
        }

        public string Detail => $"{Identifier} {Proximity}";
    }
}