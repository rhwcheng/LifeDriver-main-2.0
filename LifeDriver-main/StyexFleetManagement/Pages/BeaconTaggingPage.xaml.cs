using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Acr.UserDialogs;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel;
using StyexFleetManagement.ViewModel.Base;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StyexFleetManagement.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BeaconTaggingPage : ContentPage
    {
        private readonly IBeaconTaggingViewModel _viewModel;

        public BeaconTaggingPage()
        {
            BindingContext = _viewModel = ViewModelLocator.Resolve<IBeaconTaggingViewModel>();

            InitializeComponent();

            Title = AppResources.beacon_tagging;

            ToolbarItems.Add(new ToolbarItem
            {
                IconImageSource = ImageSource.FromFile("ic_action_settings.png"),
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => (App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });

            try
            {
                _viewModel.StartScan();
            }
            catch (Exception e)
            {
                Serilog.Log.Information(e, e.Message);
                Content = new StackLayout()
                {
                    Children =
                    {
                        new Label
                        {
                            HorizontalTextAlignment = TextAlignment.Center,
                            Margin = 10,
                            Text =
                                "Beacon Tagging not supported. \n\nEither Bluetooth permissions have not been granted or your device does not support Bluetooth.",
                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                            VerticalOptions = LayoutOptions.CenterAndExpand
                        }
                    }
                };
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            var viewModel = BindingContext as ViewModelBase;

            viewModel?.OnDisappearing();
        }

        private void OnSendEmailClicked(object sender, EventArgs e)
        {
            _viewModel.SendEmail();
        }

        private void OnStartScanClicked(object sender, EventArgs e)
        {
            _viewModel.StartScan();
        }

        private void OnStopScanClicked(object sender, EventArgs e)
        {
            _viewModel.StopScan();
        }
    }
}