using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using StyexFleetManagement.ViewModel;
using StyexFleetManagement.ViewModel.Base;
using StyexFleetManagement.Salus.Enums;
using System;
using System.IO;
using System.Threading.Tasks;
using Acr.UserDialogs;
using FFImageLoading.Forms;
using Rg.Plugins.Popup.Extensions;
using StyexFleetManagement.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StyexFleetManagement.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SosPage : ContentPage
    {
        private readonly ISosViewModel _viewModel;

        public SosPage()
        {
            _viewModel = ViewModelLocator.Resolve<ISosViewModel>();
            BindingContext = _viewModel;

            InitializeComponent();

            ToolbarItems.Add(new ToolbarItem
            {
                IconImageSource = ImageSource.FromFile("ic_qr_code.png"),
                Order = ToolbarItemOrder.Primary,
                Command = new Command(async () =>
                {
                    if (string.IsNullOrWhiteSpace(Settings.Current.SalusUserFirstName) &&
                        string.IsNullOrWhiteSpace(Settings.Current.SalusUserLastName) &&
                        string.IsNullOrWhiteSpace(Settings.Current.SalusUserEmailAddress) &&
                        string.IsNullOrWhiteSpace(Settings.Current.SalusUserPhoneNumber))
                    {
                        UserDialogs.Instance.Alert(
                            "Contact details are required to use this feature. \n\nTo set up contact details, go to Settings > User Details.",
                            "Contact Information Required.", 
                            AppResources.button_ok);
                        return;
                    }
                    await App.MainDetailPage.Navigation.PushPopupAsync(new QrContactPage());
                })
            });

            ToolbarItems.Add(new ToolbarItem
            {
                IconImageSource = ImageSource.FromFile("ic_action_settings.png"),
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => App.MainDetailPage.NavigateTo(typeof(SettingsPage)))
            });

            Title = AppResources.sos;

            DriverIdImage.GestureRecognizers.Add(new TapGestureRecognizer { Command = _viewModel.DriverIdPreviewCommand });

            SosImage.GestureRecognizers.Add(new TapGestureRecognizer {Command = _viewModel.SosCommand});

            CovidHotlineImage.GestureRecognizers.Add(new TapGestureRecognizer { Command = _viewModel.CovidHotlineCommand });
        }

        private async void CheckIn_OnClicked(object sender, EventArgs e)
        {
            await _viewModel.CheckIn_OnClicked(sender, e);
        }

        private async void CheckOut_OnClicked(object sender, EventArgs e)
        {
            await _viewModel.CheckOut_OnClicked(sender, e);
        }

        private async void DriverId_OnClicked(object sender, EventArgs e)
        {
            await _viewModel.DriverId_OnClicked(sender, e);
        }
    }
}