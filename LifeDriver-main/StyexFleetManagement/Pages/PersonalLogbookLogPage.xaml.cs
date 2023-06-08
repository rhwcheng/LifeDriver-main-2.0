using Akavache;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using StyexFleetManagement.Models;

namespace StyexFleetManagement.Pages
{
    public partial class PersonalLogbookLogPage : PopupPage
    {
        ObservableCollection<LogbookEventItem> logEvents;
        public PersonalLogbookLogPage()
        {
            InitializeComponent();

            HasSystemPadding = true;

            double sidePadding = 0;
            if (Device.Idiom == TargetIdiom.Phone)
            {
                sidePadding = 10;
            }
            else
            {
                sidePadding = (App.ScreenWidth) / 5;
            }
            Padding = new Thickness(sidePadding, 10, sidePadding, 10);

            var closeGestureRecognizer = new TapGestureRecognizer();
            closeGestureRecognizer.Tapped += CloseGestureRecognizer_Tapped;

            backButton.GestureRecognizers.Add(closeGestureRecognizer);

            GetData();
        }

        private async void CloseGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await PopupNavigation.PopAsync();
        }

        private async void GetData()
        {
            var events = await BlobCache.LocalMachine.GetAllObjects<LogbookEventItem>();
            events = events.OrderByDescending(x => x.Date);
            logEvents = new ObservableCollection<LogbookEventItem>(events);
            LogView.ItemsSource = logEvents;
        }
    }
}