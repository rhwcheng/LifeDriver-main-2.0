using System;
using System.Linq;
using System.Threading.Tasks;
using Akavache;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Services;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Enums;
using StyexFleetManagement.Models;
using StyexFleetManagement.Models.Enum;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages.AppSettings
{
    public partial class SettingsContent : ContentView
    {
        bool fleetSwitchToggled;

        private View _fullView;

        public bool ShowContactSection { get; set; }

        public SettingsContent()
        {
            InitializeComponent();

            LoadContent();

            MessagingCenter.Subscribe<AuthenticationService>(this, AppEvent.LoggedInMZone.ToString(), async (t) =>
            {
                SetUsernameLabel();
                SetupAddAccountButton();
                if (PopupNavigation.Instance.PopupStack.Any())
                {
                    await PopupNavigation.Instance.PopAllAsync();
                }
            });
            MessagingCenter.Subscribe<AuthenticationService>(this, AppEvent.LoggedInSalus.ToString(), async (t) =>
            {
                SetUsernameLabel();
                SetupAddAccountButton();
                if (PopupNavigation.Instance.PopupStack.Any())
                {
                    await PopupNavigation.Instance.PopAllAsync();
                }
            });
        }

        private void LoadContent()
        {
            SetUsernameLabel();
            SetupAddAccountButton();

            if (Settings.Current.SalusUser != null)
            {
                SalusContactSection.IsVisible = true;
                firstNameEntry.Text = Settings.Current.SalusUserFirstName;
                lastNameEntry.Text = Settings.Current.SalusUserLastName;
                emailAddressEntry.Text = Settings.Current.SalusUserEmailAddress;
                phoneNumberEntry.Text = Settings.Current.SalusUserPhoneNumber;
                sosNumberEntry.Text = Settings.Current.SalusUserSosNumber;
                priorityNumberEntry.Text = Settings.Current.SalusUserPriorityNumber;
            }

            if (Settings.Current.MzoneUserId == Guid.Empty)
            {
                _fullView = Content;
                Content = new StackLayout
                {
                    Padding = 10,
                    Children =
                    {
                        usernameLabel,
                        SalusContactSection,
                        new BoxView {Color = Xamarin.Forms.Color.Gray, HeightRequest = 1, HorizontalOptions = LayoutOptions.FillAndExpand, Opacity = 0.5},
                        addAccountButton,
                        logoutButton
                    }
                };

                return;
            }

            LandingPagePicker.Items.Add(AppResources.dashboard_title);
            LandingPagePicker.Items.Add(AppResources.title_map);
            LandingPagePicker.Items.Add(AppResources.sos);

            MapPinPicker.Items.Add("Pin");
            MapPinPicker.Items.Add("Vehicle");

            //Populate Vehicle Picker
            foreach (VehicleGroup vehicleGroup in App.VehicleGroups.VehicleGroups)
            {
                VehicleGroupPicker.Items.Add(vehicleGroup.Description);
            }


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



            var version = DependencyService.Get<IInfoService>().AppVersionCode;

            //Set labels
            nameLabel.Text = Settings.Current.UserDescription;
            serverLabel.Text = Settings.Current.Server;
            currencyLabel.Text = Settings.Current.Currency;
            distanceLabel.Text = DistanceMeasurementUnit.GetDescription(Settings.Current.DistanceMeasurementUnit);
            fluidLabel.Text = FluidMeasurementUnit.GetDescription(Settings.Current.FluidMeasurementUnit);
            versionLabel.Text = AppResources.version + " " + version;
            phoneIdEntry.Text = Settings.Current.DeviceId;
            driverCodeEntry.Text = Settings.Current.DriverId.ToString();
            reportingIntervalEntry.Text = Settings.Current.ReportingInterval.ToString();

            SetDashboardLimits();

            fleetSwitch.IsToggled = Settings.Current.PlotFleetExceptions;
            ubiSwitch.IsToggled = Settings.Current.PlotUBIExceptions;
            multipleTripPlottingSwitch.IsToggled = Settings.Current.AllowMultipleTripPlotting;
            snapTripToRoadSwitch.IsToggled = Settings.Current.SnapTripToRoad;
            locationUpdatesSwitch.IsToggled = Settings.Current.LocationUpdates;
            tripReportingSwitch.IsToggled = Settings.Current.TripRecording;

            ubiSwitch.Toggled += UbiSwitch_Toggled;
            fleetSwitch.Toggled += FleetSwitch_Toggled;
            multipleTripPlottingSwitch.Toggled += MultipleTripPlottingSwitch_Toggled;
            snapTripToRoadSwitch.Toggled += SnapTripToRoadSwitch_Toggled;
            locationUpdatesSwitch.Toggled += LocationUpdatesSwitch_Toggled;
            tripReportingSwitch.Toggled += TripReportingSwitch_Toggled;

            LandingPagePicker.SelectedIndex = (int)Settings.Current.LandingPage;
            LandingPagePicker.SelectedIndexChanged += LandingPagePicker_SelectedIndexChanged;

            MapPinPicker.SelectedIndex = (int)Settings.Current.MapMarker;
            MapPinPicker.SelectedIndexChanged += MapPinPicker_SelectedIndexChanged;

            VehicleGroupPicker.SelectedIndex = (int)Settings.Current.DefaultVehicleGroup;
            VehicleGroupPicker.SelectedIndexChanged += VehicleGroupPicker_SelectedIndexChanged;

            //Set dashboard Toggles
            SetDashboardToggleSwitches();
        }

        private void SetupAddAccountButton()
        {
            if (Settings.Current.SalusUser != null && Settings.Current.MzoneUserId != Guid.Empty)
            {
                addAccountButton.IsVisible = false;
            }
            else if (Settings.Current.SalusUser != null)
            {
                addAccountButton.Text = AppResources.add_mzone_account;
            }
            else if (Settings.Current.MzoneUserId != Guid.Empty)
            {
                addAccountButton.Text = AppResources.add_salus_account;
            }
        }

        private void SetUsernameLabel()
        {
            var mZoneUsername = DependencyService.Get<ICredentialsService>().MZoneUserName;
            var salusUsername = DependencyService.Get<ICredentialsService>().SalusUserName;

            if (!string.IsNullOrEmpty(mZoneUsername) && !string.IsNullOrEmpty(salusUsername))
            {
                usernameLabel.Text = $"{mZoneUsername} | {salusUsername}";
            }
            else if (!string.IsNullOrEmpty(mZoneUsername))
            {
                usernameLabel.Text = mZoneUsername;
            }
            else if (!string.IsNullOrEmpty(salusUsername))
            {
                usernameLabel.Text = salusUsername;
            }
        }

        private void VehicleGroupPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Current.DefaultVehicleGroup = VehicleGroupPicker.SelectedIndex;
        }

        private void TripReportingSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            bool tripReporting = tripReportingSwitch.IsToggled;

            Settings.Current.TripRecording = tripReporting;
        }

        private void LocationUpdatesSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            bool locationUpdates = locationUpdatesSwitch.IsToggled;

            Settings.Current.LocationUpdates = locationUpdates;

            MessagingCenter.Send(this, "LocationUpdatesPreferenceChanged");
        }

        private void SetDashboardToggleSwitches()
        {
            var reports = Settings.Current.DashboardReports;

            nonReportingSwitch.IsToggled = reports.Contains(DashboardReport.NonReporting);
            exceptionAlertsSwitch.IsToggled = reports.Contains(DashboardReport.ExceptionCounts);
            fuelConsumptionSwitch.IsToggled = reports.Contains(DashboardReport.FuelConsumption);
            fuelTheftSwitch.IsToggled = reports.Contains(DashboardReport.FuelTheft);
            accidentCountSwitch.IsToggled = reports.Contains(DashboardReport.AccidentCount);
            vehicleDTCSwitch.IsToggled = reports.Contains(DashboardReport.VehicleDTC);
            stoppedTimeSwitch.IsToggled = reports.Contains(DashboardReport.StoppedTime);
            idleTimeSwitch.IsToggled = reports.Contains(DashboardReport.IdleTime);
            distDrivenSwitch.IsToggled = reports.Contains(DashboardReport.DistanceDriven);
            driveDurationSwitch.IsToggled = reports.Contains(DashboardReport.DriveDuration);

            nonReportingSwitch.Toggled += DashboardReportToggler;
            exceptionAlertsSwitch.Toggled += DashboardReportToggler;
            fuelConsumptionSwitch.Toggled += DashboardReportToggler;
            fuelTheftSwitch.Toggled += DashboardReportToggler;
            accidentCountSwitch.Toggled += DashboardReportToggler;
            vehicleDTCSwitch.Toggled += DashboardReportToggler;
            stoppedTimeSwitch.Toggled += DashboardReportToggler;
            idleTimeSwitch.Toggled += DashboardReportToggler;
            distDrivenSwitch.Toggled += DashboardReportToggler;
            driveDurationSwitch.Toggled += DashboardReportToggler;
        }

        private void SetDashboardLimits()
        {
            //Set limits
            nonReportingEntry.Text = Settings.Current.NonReportingThreshold.ToString();
            exceptionAlertsEntry.Text = Settings.Current.ExceptionCountTreshold.ToString();
            fuelConsumptionEntry.Text = Settings.Current.FuelConsumptionThreshold.ToString();
            fuelTheftEntry.Text = Settings.Current.FuelTheftThreshold.ToString();
            accidentCountEntry.Text = Settings.Current.AccidentCountThreshold.ToString();
            vehicleDTCEntry.Text = Settings.Current.VehicleDTCCountThreshold.ToString();
            idleTimeEntry.Text = Settings.Current.IdleThreshold.ToString();
            distDrivenEntry.Text = Settings.Current.DistanceDrivenThreshold.ToString();
            driveDurationEntry.Text = Settings.Current.DriveDurationThreshold.ToString();
            stoppedTimeEntry.Text = Settings.Current.StoppedTimeThreshold.ToString();

        }

        void Entry_LimitChanged(object sender, TextChangedEventArgs e)
        {
            var oldText = e.OldTextValue;
            var newText = e.NewTextValue;

            if (newText == oldText)
                return;

            var s = sender as Entry;

            int threshold;
            bool isInt = int.TryParse(s.Text, out threshold);

            switch (s.Placeholder.ToLower())
            {
                case "nonreporting":
                    if (threshold == Settings.Current.NonReportingThreshold)
                    {
                        nonReportingButton.IsVisible = false;
                        return;
                    }
                    nonReportingButton.IsVisible = true;
                    break;
                case "exceptionalerts":
                    if (threshold == Settings.Current.ExceptionCountTreshold)
                    {
                        exceptionAlertsButton.IsVisible = false;
                        return;
                    }
                    exceptionAlertsButton.IsVisible = true;
                    break;
                case "fuelconsumption":
                    if (threshold == Settings.Current.FuelConsumptionThreshold)
                    {
                        fuelConsumptionButton.IsVisible = false;
                        return;
                    }
                    fuelConsumptionButton.IsVisible = true;
                    break;
                case "fueltheft":
                    if (threshold == Settings.Current.FuelTheftThreshold)
                    {
                        fuelTheftButton.IsVisible = false;
                        return;
                    }
                    fuelTheftButton.IsVisible = true;
                    break;
                case "accidentcount":
                    if (threshold == Settings.Current.AccidentCountThreshold)
                    {
                        accidentCountButton.IsVisible = false;
                        return;
                    }
                    accidentCountButton.IsVisible = true;
                    break;
                case "vehicledtc":
                    if (threshold == Settings.Current.VehicleDTCCountThreshold)
                    {
                        vehicleDTCButton.IsVisible = false;
                        return;
                    }
                    vehicleDTCButton.IsVisible = true;
                    break;
                case "stoppedtime":
                    if (threshold == Settings.Current.StoppedTimeThreshold)
                    {
                        stoppedTimeButton.IsVisible = false;
                        return;
                    }
                    stoppedTimeButton.IsVisible = true;
                    break;
                case "idletime":
                    if (threshold == Settings.Current.IdleThreshold)
                    {
                        idleTimeButton.IsVisible = false;
                        return;
                    }
                    idleTimeButton.IsVisible = true;
                    break;
                case "distancedriven":
                    if (threshold == Settings.Current.DistanceDrivenThreshold)
                    {
                        distDrivenButton.IsVisible = false;
                        return;
                    }
                    distDrivenButton.IsVisible = true;
                    break;
                case "driveduration":
                    if (threshold == Settings.Current.DriveDurationThreshold)
                    {
                        driveDurButton.IsVisible = false;
                        return;
                    }
                    driveDurButton.IsVisible = true;
                    break;

            }


        }

        void FirstName_Changed(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            var value = entry?.Text;

            firstNameButton.IsVisible = value != Settings.Current.SalusUserFirstName;
        }

        void LastName_Changed(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            var value = entry?.Text;

            lastNameButton.IsVisible = value != Settings.Current.SalusUserLastName;
        }

        void EmailAddress_Changed(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            var value = entry?.Text;

            emailAddressButton.IsVisible = value != Settings.Current.SalusUserEmailAddress;
        }

        void PhoneNumber_Changed(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            var value = entry?.Text;

            phoneNumberButton.IsVisible = value != Settings.Current.SalusUserPhoneNumber;
        }

        void SosNumber_Changed(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            var value = entry?.Text;

            sosNumberButton.IsVisible = value != Settings.Current.SalusUserSosNumber;
        }

        void PriorityNumber_Changed(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            var value = entry?.Text;

            priorityNumberButton.IsVisible = value != Settings.Current.SalusUserPriorityNumber;
        }

        void PhoneId_Changed(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            var value = entry.Text;

            if (value == Settings.Current.DeviceId)
            {
                phoneIdButton.IsVisible = false;
                return;
            }
            phoneIdButton.IsVisible = true;
        }
        void DriverCode_Changed(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            uint value;
            var success = uint.TryParse(entry.Text, out value);

            if (!success)
                return;

            if (value == Settings.Current.DriverId)
            {
                driverCodeButton.IsVisible = false;
                return;
            }
            driverCodeButton.IsVisible = true;
        }
        void ReportingInterval_Changed(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;
            int value;
            var success = int.TryParse(entry.Text, out value);

            if (!success)
                return;

            if (value == Settings.Current.ReportingInterval)
            {
                reportingIntervalButton.IsVisible = false;
                return;
            }
            reportingIntervalButton.IsVisible = true;
        }
        void UpdateFirstName(object sender, EventArgs args)
        {
            Settings.Current.SalusUserFirstName = firstNameEntry.Text;
            firstNameButton.IsVisible = false;
        }

        void UpdateLastName(object sender, EventArgs args)
        {
            var entry = sender as Entry;
            Settings.Current.SalusUserLastName = lastNameEntry.Text;
            lastNameButton.IsVisible = false;
        }
        void UpdateEmailAddress(object sender, EventArgs args)
        {
            var entry = sender as Entry;
            Settings.Current.SalusUserEmailAddress = emailAddressEntry.Text;
            emailAddressButton.IsVisible = false;
        }
        void UpdateSosNumber(object sender, EventArgs args)
        {
            var entry = sender as Entry;
            Settings.Current.SalusUserSosNumber = sosNumberEntry.Text;
            sosNumberButton.IsVisible = false;
        }
        void UpdatePriorityNumber(object sender, EventArgs args)
        {
            var entry = sender as Entry;
            Settings.Current.SalusUserPriorityNumber = priorityNumberEntry.Text;
            priorityNumberButton.IsVisible = false;
        }
        void UpdatePhoneNumber(object sender, EventArgs args)
        {
            var entry = sender as Entry;
            Settings.Current.SalusUserPhoneNumber = phoneNumberEntry.Text;
            phoneNumberButton.IsVisible = false;
        }
        void UpdatePhoneId(object sender, EventArgs args)
        {
            var s = sender as Image;

            string value = phoneIdEntry.Text;

            Settings.Current.DeviceId = value;
            phoneIdButton.IsVisible = false;
        }
        void UpdateDriverCode(object sender, EventArgs args)
        {
            var s = sender as Image;

            uint value;
            bool isInt = uint.TryParse(driverCodeEntry.Text, out value);

            if (!isInt)
                return;

            Settings.Current.DriverId = value;
            driverCodeButton.IsVisible = false;
        }
        void UpdateReportingInterval(object sender, EventArgs args)
        {
            var s = sender as Image;

            int value;
            bool isInt = int.TryParse(reportingIntervalEntry.Text, out value);

            Settings.Current.ReportingInterval = value;
            reportingIntervalButton.IsVisible = false;
        }

        void UpdateNonReportingThreshold(object sender, EventArgs args)
        {
            var s = sender as Image;

            int threshold;
            bool isInt = int.TryParse(nonReportingEntry.Text, out threshold);

            UpdateThreshold(threshold, nonReportingEntry.Placeholder.ToLower());
        }

        void UpdateExceptionAlertsThreshold(object sender, EventArgs args)
        {
            var s = sender as Image;

            int threshold;
            bool isInt = int.TryParse(exceptionAlertsEntry.Text, out threshold);

            UpdateThreshold(threshold, exceptionAlertsEntry.Placeholder.ToLower());
        }

        void UpdateFuelConsumptionThreshold(object sender, EventArgs args)
        {
            var s = sender as Image;

            int threshold;
            bool isInt = int.TryParse(fuelConsumptionEntry.Text, out threshold);

            UpdateThreshold(threshold, fuelConsumptionEntry.Placeholder.ToLower());
        }

        void UpdateFuelTheftThreshold(object sender, EventArgs args)
        {
            var s = sender as Image;

            int threshold;
            bool isInt = int.TryParse(fuelTheftEntry.Text, out threshold);

            UpdateThreshold(threshold, fuelTheftEntry.Placeholder.ToLower());
        }

        void UpdateAccidentCountThreshold(object sender, EventArgs args)
        {
            var s = sender as Image;

            int threshold;
            bool isInt = int.TryParse(accidentCountEntry.Text, out threshold);

            UpdateThreshold(threshold, accidentCountEntry.Placeholder.ToLower());
        }

        void UpdateVehicleDTCThreshold(object sender, EventArgs args)
        {
            var s = sender as Image;

            int threshold;
            bool isInt = int.TryParse(vehicleDTCEntry.Text, out threshold);

            UpdateThreshold(threshold, vehicleDTCEntry.Placeholder.ToLower());
        }

        void UpdateStoppedTimeThreshold(object sender, EventArgs args)
        {
            var s = sender as Image;

            int threshold;
            bool isInt = int.TryParse(stoppedTimeEntry.Text, out threshold);

            UpdateThreshold(threshold, stoppedTimeEntry.Placeholder.ToLower());
        }

        void UpdateIdleTimeThreshold(object sender, EventArgs args)
        {
            var s = sender as Image;

            int threshold;
            bool isInt = int.TryParse(idleTimeEntry.Text, out threshold);

            UpdateThreshold(threshold, idleTimeEntry.Placeholder.ToLower());
        }

        void UpdateDistDrivenThreshold(object sender, EventArgs args)
        {
            var s = sender as Image;

            int threshold;
            bool isInt = int.TryParse(distDrivenEntry.Text, out threshold);

            UpdateThreshold(threshold, distDrivenEntry.Placeholder.ToLower());
        }

        void UpdateDriveDurationThreshold(object sender, EventArgs args)
        {
            var s = sender as Image;

            int threshold;
            bool isInt = int.TryParse(driveDurationEntry.Text, out threshold);

            UpdateThreshold(threshold, driveDurationEntry.Placeholder.ToLower());
        }

        private async void UpdateThreshold(int threshold, string name)
        {
            switch (name)
            {
                case "nonreporting":
                    Settings.Current.NonReportingThreshold = threshold;
                    nonReportingButton.IsVisible = false;
                    break;
                case "exceptionalerts":
                    Settings.Current.ExceptionCountTreshold = threshold;
                    exceptionAlertsButton.IsVisible = false;
                    break;
                case "fuelconsumption":
                    Settings.Current.FuelConsumptionThreshold = threshold;
                    fuelConsumptionButton.IsVisible = false;
                    break;
                case "fueltheft":
                    Settings.Current.FuelTheftThreshold = threshold;
                    fuelTheftButton.IsVisible = false;
                    break;
                case "accidentcount":
                    Settings.Current.AccidentCountThreshold = threshold;
                    accidentCountButton.IsVisible = false;
                    break;
                case "vehicledtc":
                    Settings.Current.VehicleDTCCountThreshold = threshold;
                    vehicleDTCButton.IsVisible = false;
                    break;
                case "stoppedtime":
                    Settings.Current.StoppedTimeThreshold = threshold;
                    stoppedTimeButton.IsVisible = false;
                    break;
                case "idletime":
                    Settings.Current.IdleThreshold = threshold;
                    idleTimeButton.IsVisible = false;
                    break;
                case "distancedriven":
                    Settings.Current.DistanceDrivenThreshold = threshold;
                    distDrivenButton.IsVisible = false;
                    break;
                case "driveduration":
                    Settings.Current.DriveDurationThreshold = threshold;
                    driveDurButton.IsVisible = false;
                    break;
            }


            await App.MainDetailPage.DisplayAlert(AppResources.success_label, AppResources.label_updated, AppResources.button_ok);
        }

        private async void LimitChanged(object sender, EventArgs e)
        {
            try
            {
                var s = sender as Entry;

                int threshold;
                bool isInt = int.TryParse(s.Text, out threshold);
                if (!isInt)
                {
                    await App.MainDetailPage.DisplayAlert(AppResources.error_label, AppResources.invalid_value, AppResources.button_ok);
                    return;
                }
                UpdateThreshold(threshold, s.Placeholder.ToLower());
            }
            catch (NullReferenceException error)
            {
                Serilog.Log.Debug(error.Message);
                await App.MainDetailPage.DisplayAlert(AppResources.error_label, AppResources.error_label, AppResources.button_ok);
            }


        }


        private void LandingPagePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Current.LandingPage = (LandingPage)LandingPagePicker.SelectedIndex;
        }

        private void MapPinPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Current.MapMarker = (MapMarkerImage)MapPinPicker.SelectedIndex;
        }

        private void SnapTripToRoadSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            bool snapTripToRoad = snapTripToRoadSwitch.IsToggled;

            Settings.Current.SnapTripToRoad = snapTripToRoad;
        }

        private void MultipleTripPlottingSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            bool allowMultipleTripPlotting = multipleTripPlottingSwitch.IsToggled;

            Settings.Current.AllowMultipleTripPlotting = allowMultipleTripPlotting;
        }

        private void FleetSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            fleetSwitchToggled = true;
            bool plotFleetExceptions = fleetSwitch.IsToggled;

            Settings.Current.PlotFleetExceptions = plotFleetExceptions;

            if (fleetSwitch.IsToggled == ubiSwitch.IsToggled)
                ubiSwitch.IsToggled = !(ubiSwitch.IsToggled);



        }

        private void DashboardReportToggler(object sender, ToggledEventArgs e)
        {
            var toggledSwitch = sender as CustomDashboardSwitch;

            if (e.Value == false)
            {
                if (Settings.Current.ReportOne == toggledSwitch.Report)
                    Settings.Current.ReportOne = DashboardReport.None;
                if (Settings.Current.ReportTwo == toggledSwitch.Report)
                    Settings.Current.ReportTwo = DashboardReport.None;
                if (Settings.Current.ReportThree == toggledSwitch.Report)
                    Settings.Current.ReportThree = DashboardReport.None;
                if (Settings.Current.ReportFour == toggledSwitch.Report)
                    Settings.Current.ReportFour = DashboardReport.None;
                if (Settings.Current.ReportFive == toggledSwitch.Report)
                    Settings.Current.ReportFive = DashboardReport.None;
                if (Settings.Current.ReportSix == toggledSwitch.Report)
                    Settings.Current.ReportSix = DashboardReport.None;


            }
            else
            {
                var reports = Settings.Current.DashboardReports;
                if (reports.Count == 6) //Reached max
                {
                    toggledSwitch.Toggled -= DashboardReportToggler;
                    toggledSwitch.IsToggled = !(toggledSwitch.IsToggled);
                    toggledSwitch.Toggled += DashboardReportToggler;
                    return;
                }

                if (Settings.Current.ReportOne == DashboardReport.None)
                {
                    Settings.Current.ReportOne = toggledSwitch.Report;
                    return;
                }
                if (Settings.Current.ReportTwo == DashboardReport.None)
                {
                    Settings.Current.ReportTwo = toggledSwitch.Report;
                    return;
                }
                if (Settings.Current.ReportThree == DashboardReport.None)
                {
                    Settings.Current.ReportThree = toggledSwitch.Report;
                    return;
                }
                if (Settings.Current.ReportFour == DashboardReport.None)
                {
                    Settings.Current.ReportFour = toggledSwitch.Report;
                    return;
                }
                if (Settings.Current.ReportFive == DashboardReport.None)
                {
                    Settings.Current.ReportFive = toggledSwitch.Report;
                    return;
                }
                if (Settings.Current.ReportSix == DashboardReport.None)
                {
                    Settings.Current.ReportSix = toggledSwitch.Report;
                    return;
                }
            }
        }

        private void UbiSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            bool plotUBIExceptions = ubiSwitch.IsToggled;

            Settings.Current.PlotUBIExceptions = plotUBIExceptions;

            if (ubiSwitch.IsToggled == fleetSwitch.IsToggled)
                fleetSwitch.IsToggled = !(fleetSwitch.IsToggled);


        }


        async void OnLogoutButtonClicked(object sender, EventArgs e)
        {
            DependencyService.Get<ICredentialsService>().DeleteCredentials();
            ClearCache(isLoggingOut: true);

            var page = new LoginPage();

            await Navigation.PushModalAsync(page);

            if (PopupNavigation.Instance.PopupStack.Any())
            {
                await PopupNavigation.Instance.PopAllAsync();
            }
        }
        async void OnResetDashboardSettingsClicked(object sender, EventArgs e)
        {
            var answer = await App.MainDetailPage.DisplayAlert("Note", AppResources.reset_dashboard_prompt, "Confirm", "Cancel");
            if (answer)
            {
                Settings.Current.ClearDashboardSettings();
                SetDashboardLimits();
            }
        }
        async void OnRefreshButtonClicked(object sender, EventArgs e)
        {
            App.ShowLoading(true);
            var username = DependencyService.Get<ICredentialsService>().MZoneUserName;
            var password = DependencyService.Get<ICredentialsService>().MZonePassword;

            var response = await RestService.GetUsersAsync(username, password);

            if (response.ResponseToken != null && response.ErrorStatus == ServiceError.NO_ERROR)
            {
                //Configure Settings
                Settings.Current.MzoneUserId = ((User)response.ResponseToken).Id;

                response = await RestService.GetUserSettings(Settings.Current.MzoneUserId);
                try
                {
                    var userSettings = (UserSettings)response.ResponseToken;
                    Settings.Current.UserDescription = userSettings.Description;
                    Settings.Current.Currency = userSettings.CurrencyCode;
                    Settings.Current.DistanceMeasurementUnit = userSettings.UnitOfMeasureDistanceId;
                    Settings.Current.FluidMeasurementUnit = userSettings.UnitOfMeasureFluidId;
                }
                catch (InvalidCastException ex)
                {
                    Serilog.Log.Debug(ex.Message);
                    await App.MainDetailPage.DisplayAlert("Refresh failed", response.GetErrorMessage(), "OK");
                }
            }
            App.ShowLoading(false);
        }

        void OnClearCacheButtonClicked(object sender, EventArgs e)
        {
            ClearCache(true);
        }

        public static async void ClearCache(bool showMessage = false, bool isLoggingOut = false)
        {
            if (App.GetVehicleIconsTask != null)
                App.CancellationToken.Cancel();

            BlobCache.LocalMachine.InvalidateAll();
            Settings.Current.Clear(isLoggingOut);
            App.Singleton.HttpClient = null;

            if (showMessage)
                await App.MainDetailPage.DisplayAlert("Success", "Cache cleared", "OK");

            App.ShowLoading(true);

            if (!isLoggingOut)
                await App.InitializeData();

            App.ShowLoading(false);
        }

        private async void OnAddAccountClicked(object sender, EventArgs e)
        {
            var accountType = Settings.Current.MzoneUserId != Guid.Empty ? AccountType.Salus : AccountType.MZone;
            await Navigation.PushPopupAsync(new AddAccountPage(accountType));
        }
    }
}

