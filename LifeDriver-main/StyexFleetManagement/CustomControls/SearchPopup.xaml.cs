using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Rg.Plugins.Popup.Services;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages;
using StyexFleetManagement.ViewModel;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public partial class SearchPopup : Rg.Plugins.Popup.Pages.PopupPage
    {


        private MapViewModel viewModel;
        private ObservableCollection<VehicleItem> vehicleList;
        private ICommand _searchCommand;
        private ISearchPage basePage;

        public SearchPopup(ObservableCollection<VehicleItem> list, ISearchPage basePage)
        {
            vehicleList = list;
            this.basePage = basePage;
            BindingContext = this;
            InitializeComponent();
            listView.ItemsSource = list;
            listView.ItemSelected += ListView_ItemSelected;
            SetupPadding();
            SearchBar.Focus();
        }

        public SearchPopup(){
            InitializeComponent();
        }

        private void SetupPadding()
        {
            HasSystemPadding = false;

            double sidePadding = 0;
            if (Device.Idiom == TargetIdiom.Phone)
            {
                sidePadding = 10;
            }
            else
            {
                sidePadding = (App.ScreenWidth) / 5;
            }
            Padding = new Thickness(sidePadding, (App.ScreenHeight) / 5, sidePadding, (App.ScreenHeight) / 3);

        }

        void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            basePage.OnSearchSelection(sender, e);
            PopupNavigation.PopAsync();
        }


        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }


        public ICommand SearchCommand

        {
            get
            {
                return _searchCommand ?? (_searchCommand = new Command<string>((text) =>
                {
                    int i = 0;
                    if (viewModel == null){
                        var vehicles = vehicleList.Where(v => v.Description.ToLower().Contains(text.ToLower())).AsEnumerable();
                        listView.ItemsSource = new ObservableCollection<VehicleItem>(vehicles);
                        i = vehicles.Count();
                    }
                    else{
                        var vehicles = viewModel.AllVehicles.Where(v => v.Description.ToLower().Contains(text.ToLower()));
                        listView.ItemsSource =  new ObservableCollection<Vehicle>(vehicles);
                        i = vehicles.Count();
                    }
                    int heightRowList = 90;
                    i = (i * heightRowList);
                    listView.HeightRequest = i;
                }));
            }
        }

        // ### Overrided methods which can prevent closing a popup page ###

        // Invoked when a hardware back button is pressed
        protected override bool OnBackButtonPressed()
        {
            // Return true if you don't want to close this popup page when a back button is pressed
            return base.OnBackButtonPressed();
        }

        // Invoked when background is clicked
        protected override bool OnBackgroundClicked()
        {
            // Return false if you don't want to close this popup page when a background of the popup page is clicked
            return base.OnBackgroundClicked();
        }
    }
}
