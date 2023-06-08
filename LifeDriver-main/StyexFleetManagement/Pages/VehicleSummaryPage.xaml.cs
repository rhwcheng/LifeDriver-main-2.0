using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.VehicleSummary.Tiles;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
    public partial class VehicleSummaryPage : BasePage, ISearchPage
    {
        private VehicleInfoTile vehicleInfoTile;
        private FMSFuelTile fmsFuelTile;
        private FuelConsumptionTile fuelConsTile;
        private VehicleExceptionsTile vehicleExceptionsTile;
        private TripSummaryTile tripsSummaryTile;
        private DispatchTile dispatchTile;
        private RecentViolationsTile recentViolationsTile;
        private Collection<ContentView> tiles;

        private VehicleSummaryViewModel viewModel;
        private DatePickerLayout datePickerLayout;

        public VehicleSummaryPage()
        {
            InitializeComponent();
            this.Title = "Vehicle Summary";
            InitDatePicker();
            if (Device.Idiom == TargetIdiom.Tablet)
            {
                headerStack.Orientation = StackOrientation.Horizontal;
            }
            viewModel = new VehicleSummaryViewModel();
            this.BindingContext = viewModel;
            Init();
        }

        void DatePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            viewModel.StartDate = DateHelper.GetDateRangeStartDate(App.SelectedDate);
            viewModel.EndDate = DateHelper.GetDateRangeEndDate(App.SelectedDate);

            viewModel.LoadData();
        }

        private void InitDatePicker()
        {
            datePickerLayout = new DatePickerLayout();
            headerStack.Children.Insert(1, datePickerLayout);
            datePickerLayout.DatePicker.SelectedIndexChanged += DatePicker_SelectedIndexChanged;
        }

        private async Task Init()
        {
            App.ShowLoading(true);

            await viewModel.PopulateAllVehicles();
            var defaultVehicle = viewModel.AllVehicles.FirstOrDefault();
            searchEntry.Text = defaultVehicle.Description;
            if (defaultVehicle != null)
            {
                await viewModel.FetchSelectedVehicle(defaultVehicle.Id);
            }
            App.ShowLoading(false);

            CreateTiles();
            SetUpGrid();
        }

        void Handle_Search_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            ShowSearchPopup();
            if (e.IsFocused){
                ((SearchBar)sender).Unfocus();
            }

        }

        private void ShowSearchPopup()
        {
            var searchPopup = new SearchPopup(viewModel.AllVehicles, this);
            Rg.Plugins.Popup.Services.PopupNavigation.PushAsync(searchPopup);
        }

        private void CreateTiles()
        {
            tiles = new Collection<ContentView>();
            tiles.Add(vehicleInfoTile = new VehicleInfoTile());
            tiles.Add(fmsFuelTile = new FMSFuelTile());
            tiles.Add(fuelConsTile = new FuelConsumptionTile());
            tiles.Add(vehicleExceptionsTile = new VehicleExceptionsTile(viewModel));
            tiles.Add(tripsSummaryTile = new TripSummaryTile());
            //tiles.Add(dispatchTile = new DispatchTile());
            tiles.Add(recentViolationsTile = new RecentViolationsTile());

        }


        public async Task OnSearchSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return; //ItemSelected is called on deselection, which results in SelectedItem being set to null
            }
            var selectedVehicle = (VehicleItem)e.SelectedItem;
            searchEntry.Text = selectedVehicle.Description;
            await viewModel.FetchSelectedVehicle(selectedVehicle.Id);
            await viewModel.LoadData();
            vehicleExceptionsTile.SetUpExceptionLineGraphData();
        }

        private void SetUpGrid()
        {
            MainGrid.RowDefinitions.Clear();
            MainGrid.ColumnDefinitions.Clear();

            if (Device.Idiom == TargetIdiom.Phone)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                //set frame cols and rows
                int index = 0;
                foreach (ContentView tile in tiles)
                {
                    MainGrid.Children.Add(tiles[index], 0, index);
                    index += 1;
                }
            }
            else
            {
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

                //set frame cols and rows
                int col = 0;
                int row = 0;
                for (int i = 1; i <= tiles.Count; i++)
                {
                    MainGrid.Children.Add(tiles[i - 1], col, row);

                    if (i % 3 == 0)
                    {
                        col = 0;
                        row += 1;
                    }
                    else
                        col += 1;


                }

            }


        }

    }
}
