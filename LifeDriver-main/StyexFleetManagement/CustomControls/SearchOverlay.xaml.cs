using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages;
using StyexFleetManagement.ViewModel;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public partial class SearchOverlay : Frame
    {
        private MapViewModel viewModel;
        private ObservableCollection<VehicleItem> vehicleList;
        private ICommand _searchCommand;
        private ISearchPage basePage;

        public SearchOverlay(MapViewModel viewModel, ISearchPage basePage)
        {
            this.viewModel = viewModel;
            this.basePage = basePage;
            BindingContext = this;
            InitializeComponent();
            listView.ItemsSource = viewModel.AllVehicles;
            listView.ItemSelected += ListView_ItemSelected;
        }

        public SearchOverlay()
        {
            InitializeComponent();
        }

        async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            await basePage.OnSearchSelection(sender, e);
            HideSearchList();
        }

        private void ShowSearchList(int height)
        {
            if (!listView.IsVisible)
                listView.IsVisible = true;
            var rect = new Rectangle(this.X, this.Y, this.Width, 55 + height);

            this.LayoutTo(rect);
        }

        private void HideSearchList()
        {
            var rect = new Rectangle(this.X, this.Y, this.Width, 55);
            listView.IsVisible = false;
            this.LayoutTo(rect);
        }

        public ICommand SearchCommand

        {
            get
            {
                return _searchCommand ?? (_searchCommand = new Command<string>((text) =>
                {
                    if (text == null || text == string.Empty){
                        HideSearchList();
                        return;
                    }
                    int i = 0;
                    if (viewModel == null)
                    {
                        var vehicles = vehicleList.Where(v => v.Description.ToLower().Contains(text.ToLower())).AsEnumerable();
                        listView.ItemsSource = new ObservableCollection<VehicleItem>(vehicles);
                        i = vehicles.Count();
                    }
                    else
                    {
                        var vehicles = viewModel.AllVehicles.Where(v => v.Description.ToLower().Contains(text.ToLower()));
                        listView.ItemsSource = new ObservableCollection<Vehicle>(vehicles);
                        i = vehicles.Count();
                    }
                    int heightRowList = 90;
                    i = (i * heightRowList);
                    listView.HeightRequest = i;
                    ShowSearchList(i);
                }));
            }
        }

        internal void ClearSelection()
        {
            SearchBar.ClearValue(SearchBar.TextProperty);
        }

        internal void SetSelection(string value)
        {
            SearchBar.SetValue(SearchBar.TextProperty, value);
        }
    }
}
