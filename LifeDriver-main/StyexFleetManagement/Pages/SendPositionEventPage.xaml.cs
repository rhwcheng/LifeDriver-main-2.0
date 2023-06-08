using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StyexFleetManagement.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SendPositionEventPage : ContentPage
    {
        public SendPositionEventPage()
        {
            InitializeComponent();

            Title = AppResources.send_position_event;

            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });

        }
    }
}