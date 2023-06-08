using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel;
using StyexFleetManagement.ViewModel.Base;
using System;
using System.Net.Security;
using System.Threading.Tasks;
using StyexFleetManagement.Enums;
using StyexFleetManagement.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StyexFleetManagement.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CountdownTimerPage
    {
        public virtual ICountdownTimerViewModel ViewModel => ViewModelLocator.Resolve<IGuardianAngelViewModel>();

        public CountdownTimerPage()
        {
            InitializeComponent();

            Title = AppResources.guardian_angel_title;

            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });

            BindingContext = ViewModel;
            ViewModel.Initialize();

            PerformLocationCheck();

            PerformAuthenticationCheck();
        }

        private void PerformAuthenticationCheck()
        {
            if (Settings.Current.SalusUser == null)
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    Children =
                    {
                        new Label()
                        {
                            Text = "Please add your Salus credentials to use this feature.",
                            VerticalOptions = LayoutOptions.CenterAndExpand,
                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                            Padding = 5
                        }
                    }
                };
            }
        }

        private async Task PerformLocationCheck()
        {
            if (!(await ViewModelLocator.Resolve<IPermissionsService>().GetLocationPermissionAsync()))
            {
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.CenterAndExpand,
                    Children =
                    {
                        new Label()
                        {
                            Text = AppResources.location_permission_required,
                            VerticalOptions = LayoutOptions.CenterAndExpand
                        }
                    }
                };
            }
        }

        private void OnStartTimerClicked(object sender, EventArgs e)
        {
            ViewModel.HandleStartTimer();
        }

        private void OnStopTimerClicked(object sender, EventArgs e)
        {
            ViewModel.HandleStopTimer();
        }
    }
}