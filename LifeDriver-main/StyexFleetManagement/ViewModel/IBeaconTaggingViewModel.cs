using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UniversalBeacon.Library.Core.Entities;

namespace StyexFleetManagement.ViewModel
{
    public interface IBeaconTaggingViewModel
    {
        ObservableCollection<BeaconViewModel> Beacons { get; set; }
        void StartScan();
        void StopScan();
        Task SendEmail();
    }
}