using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StyexFleetManagement.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NfcPage : ContentPage
    {
        public NfcPage()
        {
            InitializeComponent();

            Title = AppResources.nfc;

            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });

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

    }
}