using Rg.Plugins.Popup.Extensions;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel.Base;
using System;
using System.IO;
using System.Threading.Tasks;
using Acr.UserDialogs;
using StyexFleetManagement.Resx;
using Xamarin.Essentials;
using Xamarin.Forms;
using EventType = StyexFleetManagement.Salus.Enums.EventType;

namespace StyexFleetManagement.ViewModel
{
    public class SosViewModel : ViewModelBase, ISosViewModel
    {
        private readonly ILocationUpdateService _locationUpdateService;

        private Command _driverIdPreviewCommand;
        private Command _sosCommand;
        private Command _covidHotlineCommand;
        private ImageSource _driverIdImageSource;

        public SosViewModel()
        {
            _locationUpdateService = ViewModelLocator.Resolve<ILocationUpdateService>();

            DriverIdPreviewCommand = new Command(async () =>
            {
                var imagePreviewPage = new ImagePreviewPage { ImageSource = DriverIdImageSource };
                await App.MainDetailPage.Navigation.PushPopupAsync(imagePreviewPage);
            });

            SosCommand = new Command(async () =>
            {
                await _locationUpdateService.GetLocationAndSendEvent(EventType.Sos, true);
                try
                {
                    //PhoneDialer.Open(Settings.Current.SalusCovidHotline);
                    if (!string.IsNullOrEmpty(Settings.Current.SalusUserSosNumber))
                        PhoneDialer.Open(Settings.Current.SalusUserSosNumber);
                    else
                        UserDialogs.Instance.Toast(AppResources.phone_number_not_exist);
                }
                catch (FeatureNotSupportedException e)
                {
                    UserDialogs.Instance.Toast(AppResources.not_supported);
                    Serilog.Log.Error(e, e.Message);
                }
            });

            CovidHotlineCommand = new Command(() =>
            {
                try
                {
                    if (!string.IsNullOrEmpty(Settings.Current.SalusUserPriorityNumber))
                        PhoneDialer.Open(Settings.Current.SalusUserPriorityNumber);
                    else
                        UserDialogs.Instance.Toast(AppResources.phone_number_not_exist);
                }
                catch (FeatureNotSupportedException e)
                {
                    UserDialogs.Instance.Toast(AppResources.not_supported);
                    Serilog.Log.Error(e, e.Message);
                }
            });

            var driverIdImage = Settings.Current.SalusDriverIdImage;
            if (driverIdImage != null)
            {
                DriverIdImageSource = ImageSource.FromStream(() => new MemoryStream(driverIdImage));
            }
        }

        public Command CaptureDriverIdCommand { get; set; }

        async Task CaptureDriverId()
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();

                await LoadPhotoAsync(photo);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, ex.Message);
            }
        }

        public async Task CheckIn_OnClicked(object sender, EventArgs e)
        {
            await _locationUpdateService.GetLocationAndSendEvent(EventType.CheckIn, true);
        }

        public async Task CheckOut_OnClicked(object sender, EventArgs e)
        {
            await _locationUpdateService.GetLocationAndSendEvent(EventType.CheckOut, true);
        }

        public async Task DriverId_OnClicked(object sender, EventArgs e)
        {
            await CaptureDriverId();
        }

        public ImageSource DriverIdImageSource
        {
            get => _driverIdImageSource;
            set
            {
                _driverIdImageSource = value;
                RaisePropertyChanged(() => DriverIdImageSource);
            }
        }

        public Command DriverIdPreviewCommand
        {
            get => _driverIdPreviewCommand;
            set
            {
                _driverIdPreviewCommand = value;
                RaisePropertyChanged(() => DriverIdPreviewCommand);
            }
        }

        public Command SosCommand
        {
            get => _sosCommand;
            set
            {
                _sosCommand = value;
                RaisePropertyChanged(() => SosCommand);
            }
        }

        public Command CovidHotlineCommand
        {
            get => _covidHotlineCommand;
            set
            {
                _covidHotlineCommand = value;
                RaisePropertyChanged(() => CovidHotlineCommand);
            }
        }

        private async Task LoadPhotoAsync(FileBase photo)
        {
            // Cancelled
            if (photo == null)
            {
                return;
            }

            byte[] imageAsBytes;
            using (var stream = await photo.OpenReadAsync())
            {
                imageAsBytes = GetBytesFromStream(stream);
            }

            Settings.Current.SalusDriverIdImage = imageAsBytes;
            DriverIdImageSource = ImageSource.FromStream(() => new MemoryStream(imageAsBytes));
        }

        private static byte[] GetBytesFromStream(Stream fileContentStream)
        {
            using (var memoryStreamHandler = new MemoryStream())
            {
                fileContentStream.CopyTo(memoryStreamHandler);
                return memoryStreamHandler.ToArray();
            }
        }
    }
}