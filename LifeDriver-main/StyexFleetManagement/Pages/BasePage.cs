using System;
using StyexFleetManagement.Pages.AppSettings;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
    public class BasePage : ContentPage
    {
        public BasePage()
		{
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });
        }
    }
}

