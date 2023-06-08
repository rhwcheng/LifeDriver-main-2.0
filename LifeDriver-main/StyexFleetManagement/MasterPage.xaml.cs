using StyexFleetManagement.Models;
using StyexFleetManagement.Pages;
using StyexFleetManagement.Pages.AlertNotifications;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using System;
using System.Collections.Generic;
using StyexFleetManagement.Enums;
using StyexFleetManagement.Services;
using Xamarin.Forms;

namespace StyexFleetManagement
{
    public partial class MasterPage : ContentPage
    {
        public ListView ListView => listView;
        public TapGestureRecognizer SettingsClick { get; set; }
        public TapGestureRecognizer LogoutClick { get; set; }


        public MasterPage()
        {
            InitializeComponent();

            SetupGestures();

            this.BackgroundColor = Color.White;

            listView.BackgroundColor = Color.Transparent;

            MessagingCenter.Subscribe<AuthenticationService>(this, AppEvent.LoggedInMZone.ToString(), (vm) => LoggedIn());
            MessagingCenter.Subscribe<AuthenticationService>(this, AppEvent.LoggedInSalus.ToString(), (vm) => LoggedIn());
        }

        private void SetupMenuItems()
        {
            var masterPageItems = new List<MasterPageItem>();
            if (Settings.Current.MzoneUserId != Guid.Empty)
            {
                masterPageItems.AddRange(new List<MasterPageItem>
                {
                    new MasterPageItem
                    {
                        Title = AppResources.dashboard_title,
                        IconSource = "ic_action_menu_star.png",
                        TargetType = typeof(DashboardCarousel)
                    },
                    //new MasterPageItem
                    //{
                    //    Title = "Vehicle Summary",
                    //    IconSource = "menu_vehicle.png",
                    //    TargetType = typeof(VehicleSummaryPage)
                    //},
                    new MasterPageItem
                    {
                        Title = AppResources.title_map,
                        IconSource = "ic_action_menu_map.png",
                        TargetType = typeof(MapPage)
                    },
                    new MasterPageItem
                    {
                        Title = AppResources.reports_title,
                        IconSource = "ic_action_entities.png",
                        TargetType = typeof(ReportsPage)
                    },
                    new MasterPageItem
                    {
                        Title = AppResources.report_title_alert_notifications,
                        IconSource = "ic_action_alert_notifcations.png",
                        TargetType = typeof(AlertNotificationsPage)
                    },
                    new MasterPageItem
                    {
                        Title = AppResources.report_title_personal_logbook,
                        IconSource = "ic_action_actions.png",
                        TargetType = typeof(LogbookPage)
                    }
                });
                //  masterPageItems.Add (new MasterPageItem {
                //	Title = AppResources.report_title_fleet,
                //	IconSource = "ic_action_menu_vehicle.png",
                //	TargetType = typeof(VehiclesPage)
                //});masterPageItems.Add(new MasterPageItem
                //{
                //	Title = AppResources.report_title_entities,
                //	IconSource = "ic_action_entities.png",
                //	TargetType = typeof(EntitiesPage)
                //});
                //masterPageItems.Add (new MasterPageItem {
                //	Title = AppResources.fuel,
                //	IconSource = "ic_action_menu_fuel.png",
                //	TargetType = typeof(FuelPage)
                //});
            }

            if (Settings.Current.SalusUser != null)
            {
                masterPageItems.AddRange(new List<MasterPageItem>
                {
                    new MasterPageItem
                    {
                        Title = AppResources.sos,
                        IconSource = "ic_sos.png",
                        TargetType = typeof(SosPage)
                    },
                    new MasterPageItem
                    {
                        Title = AppResources.amber_alert_title,
                        IconSource = "ic_amber.png",
                        TargetType = typeof(AmberAlertPage)
                    },
                    new MasterPageItem
                    {
                        Title = AppResources.guardian_angel_title,
                        IconSource = "ic_guardian.png",
                        TargetType = typeof(CountdownTimerPage)
                    },
                    new MasterPageItem
                    {
                        Title = AppResources.nfc, 
                        IconSource = "ic_nfc.png", 
                        TargetType = typeof(NfcPage)
                    },
                    new MasterPageItem
                    {
                        Title = AppResources.send_position_event,
                        IconSource = "ic_send_position.png",
                        TargetType = typeof(SendPositionEventPage)
                    },
                    new MasterPageItem
                    {
                        Title = AppResources.flic,
                        IconSource = "ic_flic.png", 
                        TargetType = typeof(FlicPage)
                    },
                    new MasterPageItem
                    {
                        Title = AppResources.beacon_tagging,
                        IconSource = "ic_beacon_tagging.png",
                        TargetType = typeof(BeaconTaggingPage)
                    }
                });
            }

            listView.ItemsSource = masterPageItems;
        }

        private void SetupGestures()
        {
            SettingsClick = new TapGestureRecognizer();
            LogoutClick = new TapGestureRecognizer();
            settingsLabel.GestureRecognizers.Add(SettingsClick);
            logoutLabel.GestureRecognizers.Add(LogoutClick);
        }

        private void LoggedIn()
        {
            name.Text = $"{AppResources.welcome_label}";
            if (!string.IsNullOrWhiteSpace(Settings.Current.UserDescription))
            {
                name.Text += $", \n{Settings.Current.UserDescription}";
            }
            SetupMenuItems();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            name.Text = $"{AppResources.welcome_label}, \n{Settings.Current.UserDescription}";
        }

        void NavigateToSettings()
        {
            ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage));
        }
    }
}