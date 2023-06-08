using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StyexFleetManagement.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CovidPage : ContentPage
    {
        public CovidPage()
        {
            InitializeComponent();

            Title = AppResources.covid_19;

            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage) App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });
        }
    }
}