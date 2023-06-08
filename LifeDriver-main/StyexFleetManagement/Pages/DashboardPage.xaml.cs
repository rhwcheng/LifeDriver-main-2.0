using Acr.UserDialogs;
using Serilog;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StyexFleetManagement.Models;
using StyexFleetManagement.MZone.Client;
using StyexFleetManagement.MZone.Client.Models;
using StyexFleetManagement.Pages.AppSettings;
using Xamarin.Forms;
using Event = StyexFleetManagement.Models.Event;
using EventTypeGroup = StyexFleetManagement.Models.EventTypeGroup;
using VehicleGroup = StyexFleetManagement.Models.VehicleGroup;

namespace StyexFleetManagement.Pages
{
    public partial class DashboardPage : ContentPage
    {
        private List<Event> eventData;
        private DateTime startDate;
        private DateTime endDate;
        private SemaphoreSlim throttler;
        private List<DashboardTile> tiles;

        public DashboardPage()
        {
            InitializeComponent();

            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_truck_white.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => FocusVehiclePicker())
            });

            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_loop_white_36dp_rotated.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => RefreshPage())
            });

            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });


            eventData = new List<Event>();
            tiles = new List<DashboardTile>();
            throttler = new SemaphoreSlim(initialCount: 10);

            //Set date range to beginning of prev month
            var today = DateTime.Today;
            var month = new DateTime(today.Year, today.Month, 1);
            startDate = month.AddMonths(-1);
            endDate = today;

            //Set up picker
            //Populate Vehicle Picker
            foreach (VehicleGroup vehicleGroup in App.VehicleGroups.VehicleGroups)
            {
                vehiclePicker.Items.Add(vehicleGroup.Description);
            }
            vehiclePicker.SelectedIndex = vehiclePicker.Items.IndexOf(App.VehicleGroups.FindDescriptionFromId(Settings.Current.DashboardVehicleGroup));
            vehiclePicker.PropertyChanged += VehiclePicker_PropertyChanged;

            Init();


        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }

        private void FocusVehiclePicker()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                vehiclePicker.Focus();
            });
        }

        private void VehiclePicker_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedIndex")
            {
                var picker = (sender as Picker);
                var index = picker.SelectedIndex;

                Settings.Current.DashboardVehicleGroup = App.VehicleGroups.FindIdFromDescription(picker.Items[index]);

                RefreshPage();
            }
        }

        private void RefreshPage()
        {

            Init();
        }

        private async void Init()
        {
            App.ShowLoading(true);
            await LoadCachedData();
            App.ShowLoading(false);
            if (eventData != null && eventData.Count > 0){
                SetupTiles();
                await GetLatestEvents();
            }
            else
            {
                using (var loading = UserDialogs.Instance.Progress("Fetching Data", null, null, true, MaskType.Black))
                {
                    await LoadLatestData(loading);
                    if (eventData != null && eventData.Count > 0)
                        SetupTiles();
                }
            }

        }

        private async Task GetLatestEvents()
        {
            await LoadLatestData();
            if (eventData != null && eventData.Count > 0)
                SetupTiles();
        }

        private void SetupTiles()
        {

            var reports = Settings.Current.DashboardReports;

            tiles.Clear();

            foreach (DashboardReport report in reports)
            {
                switch (report)
                {
                    case DashboardReport.AccidentCount:
                        var accidentTile = new DashboardTile(DashboardTileType.ACCIDENT_COUNT, eventData);
                        tiles.Add(accidentTile);
                        break;
                    case DashboardReport.DistanceDriven:
                        var distanceTile = new DashboardTile(DashboardTileType.DISTANCE, eventData);
                        tiles.Add(distanceTile);
                        break;
                    case DashboardReport.DriveDuration:
                        var durationTile = new DashboardTile(DashboardTileType.DRIVE_DURATION, eventData);
                        tiles.Add(durationTile);
                        break;
                    case DashboardReport.ExceptionCounts:
                        var exceptionTile = new DashboardTile(DashboardTileType.EXCEPTION_COUNTS, eventData);
                        tiles.Add(exceptionTile);
                        break;
                    case DashboardReport.FuelConsumption:
                        var fuelConsumptionTile = new DashboardTile(DashboardTileType.FUEL_CONSUMPTION, eventData);
                        tiles.Add(fuelConsumptionTile);
                        break;
                    case DashboardReport.FuelTheft:
                        var fuelTheftTile = new DashboardTile(DashboardTileType.FUEL_THEFT, eventData);
                        tiles.Add(fuelTheftTile);
                        break;
                    case DashboardReport.IdleTime:
                        var idleTile = new DashboardTile(DashboardTileType.IDLE, eventData);
                        tiles.Add(idleTile);
                        break;
                    case DashboardReport.NonReporting:
                        var nonReportingTile = new DashboardTile(DashboardTileType.NON_REPORTING, eventData);
                        tiles.Add(nonReportingTile);
                        break;
                    case DashboardReport.StoppedTime:
                        var stoppedTimeTile = new DashboardTile(DashboardTileType.STOPPED_TIME, eventData);
                        tiles.Add(stoppedTimeTile);
                        break;
                    case DashboardReport.VehicleDTC:
                        var dtcTile = new DashboardTile(DashboardTileType.VEHICLE_DTC, eventData);
                        tiles.Add(dtcTile);
                        break;
                }
            }
            SetUpGrid();


            foreach (DashboardTile tile in tiles)
            {
                tile.Init();
            }
        }

        private async Task LoadCachedData()
        {
            try
            {
                var vehicleGroup = Settings.Current.DashboardVehicleGroup;
                if (string.IsNullOrEmpty(vehicleGroup))
                {
                    vehicleGroup = Settings.Current.DashboardVehicleGroup = App.SelectedVehicleGroup;
                }
                var vehicles = await VehicleAPI.GetVehicleInGroupAsync(vehicleGroup);
                var totalCount = vehicles.Count;
                var iter = 0;
                var allTasks = new List<Task>();
                var plotFleetEvents = Settings.Current.PlotFleetExceptions;
                List<Event> events = new List<Event>();
                foreach (VehicleItem vehicle in vehicles)
                {
                    await throttler.WaitAsync();
                    allTasks.Add(
                                Task.Run(async () =>
                                {
                                    try
                                    {
                                        List<Event> data;
                                        //if (plotFleetEvents)
                                        data = await EventAPI.GetEventsByArrayFromCache(vehicle.Id.ToString(), startDate, endDate, EventTypeGroup.AllExceptions, true);
                                        if (data != null)
                                        {
                                            foreach (var item in data)
                                            {
                                                item.VehicleId = vehicle.Id;
                                            }

                                            events = events.Concat(data).ToList();
                                        }

                                    }
                                    finally
                                    {
                                        throttler.Release();
                                    }
                                }));

                }

                await Task.WhenAll(allTasks);
                eventData = events;
            }
            catch (NullReferenceException ne)
            {
                Log.Debug(ne.Message);
                await DisplayAlert(AppResources.error_label, AppResources.error_label, AppResources.button_ok);
            }
            catch (Exception e)
            {
                Log.Debug(e.Message);
                await DisplayAlert(AppResources.error_label, AppResources.error_label, AppResources.button_ok);
            }
        }

        private async Task LoadLatestData(IProgressDialog loading = null)
        {
            //using (var loading = UserDialogs.Instance.Progress("Loading", null, null, true, MaskType.Black))
            //{
            try
            {
                // test

                var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJSUzI1NiIsImtpZCI6IkVGMUUxMkVFOTQ1NTdBNDg5MzlCMUJBNjJFQUUxQzFBN0ZDNTY2MkQiLCJ0eXAiOiJKV1QiLCJ4NXQiOiI3eDRTN3BSVmVraVRteHVtTHE0Y0duX0ZaaTAifQ.eyJuYmYiOjE2MDQyMTQ3NjYsImV4cCI6MTYwNDIxODM2NiwiaXNzIjoiaHR0cHM6Ly9sb2dpbi5tem9uZXdlYi5uZXQiLCJhdWQiOlsiaHR0cHM6Ly9sb2dpbi5tem9uZXdlYi5uZXQvcmVzb3VyY2VzIiwibXo2LWFwaSJdLCJjbGllbnRfaWQiOiJtei1zdHlleCIsInN1YiI6IjBjYmZhNmRkLTQ3YTItNDU4OC04MDIyLWE4Y2EzYjU1N2U3OCIsImF1dGhfdGltZSI6MTYwNDIxNDc2NiwiaWRwIjoibG9jYWwiLCJtel91c2VybmFtZSI6InJodyIsIm16X3VzZXJncm91cF9pZCI6IjAwMDAwMDAwLTAwMDAtMDAwMC0wMDAwLTAwMDAwMDAwMDAwMCIsIm16X3NoYXJkX2NvZGUiOiJFTUVBIiwic2NvcGUiOlsibXpfdXNlcm5hbWUiLCJvcGVuaWQiLCJtejYtYXBpLmFsbCJdLCJhbXIiOlsicHdkIl19.aIep8GHdL8iOwzjsWl4OFf_92OOVtTcK-8uJ-CpMOWV5gZ5Ne6nXjaHZaaenBy1_05Jv4EDj19LDMYt1B4GDVg704q6cup7KnUpJZHMnSuLxD7rg_opaAqIgI80Un6cmcFPssFITm0opsa5n_O9TojyDCwJYJdXOqFJjmqp3bv9QDw1yaE6phJbuD3dXKbZBowykluEW20scXTrQbWpXuEbOm_T7Ewgi7msCIar6NTCM-zcOjtIRU03lwX4xWs_--LKJHdTlKyC3t9jip5QM5hLtvZq9S38j0oQGaSk7v8lil64ccJs95eYCml_BZ3g4Irf3kqZtLcdIMMwpX3flDWVMU3cnKYVersG9J3R1ThjAGAuesB2q_ibVU6ViNgd6uR7yjqUWEYUKZJpt4wdwfDLHHmJc41oF0shZxCNjLhjbD73kA8Bqd_pQbMMH1lf8HqAR5ZcKRPq7hl6ZF38bPBxjdYDPxP78MyFpKiBtAoESkiW7R5UZPATVTDviysW86Ou0A_7cSPl1k07JqEspzbgi81Z7phykGqythzmzO5PIQEiaIxquwOm8wqkOupiEIyVOvsEykO8BqceLaYjYHSOqpxCzNn7Aa8zmIlJNVWHFdBwC9YH2qM3Vl-YKr1lsMOQ5LUfSBp99Zj9o1sSZ09YYf6JKe_Awa8FolR3Mt-A");
                try
                {
                    var client = new EventsClient(httpClient);
                    var result1 = await client.GetAsync(null,
                        "eventType_Id ne 85efe52b-1e42-435c-9b05-c6de4b581933 and eventType_Id ne 3784281c-8660-4ec2-9588-8c9068955985 and eventType_Description eq \'Trip Startup\'",
                        null, null, 100, null, false, startDate, endDate, Settings.Current.DashboardVehicleGroup, null,
                        null, false);

                }
                catch (Exception e)
                {
                    Serilog.Log.Error(e, e.Message);
                }

                // end test



                eventData.Clear();
                var vehicleGroup = Settings.Current.DashboardVehicleGroup;
                if (string.IsNullOrEmpty(vehicleGroup))
                {
                    vehicleGroup = Settings.Current.DashboardVehicleGroup = App.SelectedVehicleGroup;
                }
                var vehicles = await VehicleAPI.GetVehicleInGroupAsync(vehicleGroup);
                var allTasks = new List<Task>();
                var plotFleetEvents = Settings.Current.PlotFleetExceptions;
                List<Event> events = new List<Event>();
                var totalCount = vehicles.Count;
                var iter = 0;

                    foreach (VehicleItem vehicle in vehicles)
                    {
                        await throttler.WaitAsync();
                        allTasks.Add(
                                    Task.Run(async () =>
                                    {
                                        try
                                        {
                                            List<Event> data;
                                        //if (plotFleetEvents)
                                        data = await EventAPI.GetEventsByArrayAsync(vehicle.Id.ToString(), startDate, endDate, EventTypeGroup.AllExceptions, true);
                                        
                            if (loading != null){

                                        var progress = (iter * 100) / totalCount;
                                                if (progress > 0)
                                                    loading.PercentComplete = progress;
                                                //else
                            }

                                        //    data = await EventAPI.GetEventsByArrayAsync(vehicle.Id.ToString(), startDate, endDate, EventTypeGroup.UBIExceptions,true);


                                        //var progress = (iter * 100) / totalCount;
                                        //if (progress > 0)
                                        //loading.PercentComplete = progress;

                                        foreach (var item in data)
                                            {
                                                item.VehicleId = vehicle.Id;
                                            }

                                            events = events.Concat(data).ToList();
                                            iter++;
                                        }
                                        finally
                                        {
                                            throttler.Release();
                                        }
                                    }));

                    }

                    await Task.WhenAll(allTasks);
                    eventData = events;

            }
            catch (NullReferenceException nullReference)
            {
                Log.Debug(nullReference.Message);
                await DisplayAlert(AppResources.error_label, AppResources.error_label, AppResources.button_ok);
            }
            catch (Exception e)
            {
                Log.Debug(e.Message);
                await DisplayAlert(AppResources.error_label, AppResources.error_label, AppResources.button_ok);
            }
            //}
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
                foreach (DashboardTile tile in tiles)
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
