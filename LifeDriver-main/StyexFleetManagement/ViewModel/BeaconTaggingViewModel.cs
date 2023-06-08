
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel.Base;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interfaces;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace StyexFleetManagement.ViewModel
{
    public class BeaconTaggingViewModel : ViewModelBase, IBeaconTaggingViewModel
    {
        private IDisposable _scanner;
        private bool _scanning;
        private bool _showStartScan;
        private bool _showStopScan;
        private bool _noBeaconsFound;

        private Task _timeoutTask;
        private CancellationTokenSource _timeoutTaskCancellationTokenSource;

        private static Guid BeaconUuid => Guid.Parse("e2c56db5-dffb-48d2-b060-d0f5a71096e0");

        public bool ScanningStartedSuccessfully { get; set; }

        private void OnBeaconAdded(object sender, Beacon e)
        {
            var beacon = Beacons.FirstOrDefault(t => t.Address == e.BluetoothAddressAsString);

            Device.BeginInvokeOnMainThread(() =>
            {
                if (beacon != null)
                {
                    beacon.SetProximity(e);
                }
                else
                {
                    Beacons.Add(new BeaconViewModel(e));
                }
            });
        }

        public void StartScan()
        {
            Beacons.Clear();

            Scanning = true;
            ShowStopScan = true;
            ShowStartScan = false;
            NoBeaconsFound = false;

            var beaconService = new BeaconService { OnBeaconAdded = OnBeaconAdded };
            _scanner = beaconService;
            ScanningStartedSuccessfully = true;

            _timeoutTaskCancellationTokenSource = new CancellationTokenSource();
            _timeoutTask = Task.Delay(new TimeSpan(0, 0, 30), _timeoutTaskCancellationTokenSource.Token).ContinueWith(o => { StopScan(); }, _timeoutTaskCancellationTokenSource.Token);
        }

        public override void OnDisappearing()
        {
            StopScan();
        }

        public void StopScan()
        {
            Scanning = false;
            ShowStopScan = false;
            ShowStartScan = true;
            NoBeaconsFound = Beacons == null || !Beacons.Any();

            _timeoutTaskCancellationTokenSource?.Cancel();
            _timeoutTask?.Dispose();
            _timeoutTask = null;

            this._scanner?.Dispose();
            this._scanner = null;
        }

        public ObservableCollection<BeaconViewModel> Beacons { get; set; } = new ObservableCollection<BeaconViewModel>();

        public bool Scanning
        {
            get => _scanning;
            set
            {
                _scanning = value;
                RaisePropertyChanged(() => Scanning);
            }
        }

        public bool ShowStartScan
        {
            get => _showStartScan;
            set
            {
                _showStartScan = value;
                RaisePropertyChanged(() => ShowStartScan);
            }
        }


        public bool ShowStopScan
        {
            get => _showStopScan;
            set
            {
                _showStopScan = value;
                RaisePropertyChanged(() => ShowStopScan);
            }
        }

        public bool NoBeaconsFound
        {
            get => _noBeaconsFound;
            set
            {
                _noBeaconsFound = value;
                RaisePropertyChanged(() => NoBeaconsFound);
            }
        }

        public async Task SendEmail()
        {
            try
            {
                var message = new EmailMessage
                {
                    Subject = $"Salus: Scanned Beacons (User: { DependencyService.Get<ICredentialsService>().SalusUserName})",
                    Body = FormatEmailBody()
                };
                await Email.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException)
            {
                await Device.InvokeOnMainThreadAsync(() => UserDialogs.Instance.Toast("Email is not supported"));
            }
            catch (Exception ex)
            {
                Serilog.Log.Information(ex, ex.Message);
                // Some other exception occurred
            }
        }

        private string FormatEmailBody()
        {
            var stringBuilder = new StringBuilder();
            foreach (var beacon in Beacons)
            {
                stringBuilder.Append($"* {beacon.Identifier}. Address: {beacon.Address}");
                stringBuilder.Append("\n\n");
            }
            return stringBuilder.ToString();
        }
    }

    internal class BeaconService : IDisposable
    {
        private readonly BeaconManager _manager;

        public BeaconService()
        {
            var provider = ViewModelLocator.Resolve<IBluetoothPacketProvider>();

            if (null == provider) return;

            _manager = new BeaconManager(provider, Device.BeginInvokeOnMainThread);
            _manager.Start();
            _manager.BeaconAdded += _manager_BeaconAdded;
            provider.AdvertisementPacketReceived += Provider_AdvertisementPacketReceived;
        }

        public void Dispose()
        {
            _manager?.Stop();
        }

        public EventHandler<Beacon> OnBeaconAdded { get; set; }

        void _manager_BeaconAdded(object sender, UniversalBeacon.Library.Core.Entities.Beacon e)
        {
            if (e.BeaconType == Beacon.BeaconTypeEnum.iBeacon)
            {
                Debug.WriteLine($"_manager_BeaconAdded {sender} Beacon {e}");
                OnBeaconAdded?.Invoke(sender, e);
            }
        }

        static void Provider_AdvertisementPacketReceived(object sender,
            UniversalBeacon.Library.Core.Interop.BLEAdvertisementPacketArgs e)
        {
            Debug.WriteLine($"Provider_AdvertisementPacketReceived {sender} Beacon {e}");
        }
    }
}