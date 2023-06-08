using StyexFleetManagement.Pages;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Models;
using StyexFleetManagement.Pages.Reports;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class ReportTile : Frame
    {
        private StackLayout mainStack;
        public ReportType ReportType { get; set; }

        public ReportTile(ReportType reportType, bool isInDevelopment = false)
        {

            ReportType = reportType;

            //Set up properties
            Margin = 5;
            OutlineColor = Palette.MainAccent;
            HeightRequest = 220;
            this.HorizontalOptions = LayoutOptions.FillAndExpand;
            this.VerticalOptions = LayoutOptions.FillAndExpand;

            var scrollLayout = new ScrollView { InputTransparent = true };
            mainStack = new StackLayout { InputTransparent = true, HorizontalOptions = LayoutOptions.Fill, VerticalOptions=LayoutOptions.Fill };
            // Set up template

            //Top line
            var topLine = new Grid { InputTransparent = true, HorizontalOptions = LayoutOptions.Fill };
            topLine.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            topLine.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            var icon = new Image { InputTransparent = true };
            var title = new Label { InputTransparent = true, LineBreakMode = LineBreakMode.WordWrap, VerticalOptions = LayoutOptions.FillAndExpand, VerticalTextAlignment = TextAlignment.Center };

            var description = new Label { HorizontalOptions = LayoutOptions.FillAndExpand, InputTransparent = true };

            //Add click gesture
            var clickGesture = new TapGestureRecognizer();
            clickGesture.Tapped += ClickGesture_Tapped;
            GestureRecognizers.Add(clickGesture);

            switch (ReportType)
            {
                //Fuel Reports
                case ReportType.FUEL_SUMMARY:
                    icon.Source = "ic_fuel_fuel_entry_summary_.png";
                    title.Text = AppResources.report_title_fuel_summary;
                    description.Text = AppResources.fuel_entry_summary_description;
                    break;
                case ReportType.FUEL_CONSUMPTION:
                    icon.Source = "ic_fuel_fuel_entry_consumption.png";
                    title.Text = AppResources.report_title_fuel_consumption;
                    description.Text = AppResources.fuel_entry_consumption_description;
                    break;
                case ReportType.FUEL_COST:
                    icon.Source = "ic_fuel_fuel_entry_costs.png";
                    title.Text = AppResources.report_title_fuel_cost;
                    description.Text = AppResources.fuel_entry_costs_description;
                    break;
                case ReportType.FUEL_LEVEL_MONITOR:
                    icon.Source = "ic_fuel_level_monitor.png";
                    title.Text = AppResources.report_title_fuel_level_monitor;
                    description.Text = AppResources.fuel_level_monitor_description;
                    break;
                case ReportType.CONSUMPTION_MEASURES:
                    icon.Source = "ic_fuel_consumption_measures.png";
                    title.Text = AppResources.report_title_fuel_consumption_measures;
                    description.Text = AppResources.consumption_measures_description;
                    break;
                //Entity Reports
                case ReportType.DRIVER_ROUTE_ACTIVITY:
                    icon.Source = "ic_entities_driver_route_activity.png";
                    title.Text = AppResources.report_title_driver_route_activity;
                    description.Text = AppResources.driver_route_description;
                    break;
                case ReportType.DLT_DRIVER_STATS:
                    icon.Source = "ic_entities_dlt_card_reader.png";
                    title.Text = AppResources.report_title_dlt_driver_stats;
                    description.Text = AppResources.driver_stats_description;
                    break;
                case ReportType.POI_ACTIVITY:
                    icon.Source = "ic_entities_poi_activity.png";
                    title.Text = AppResources.report_title_poi_activity;
                    description.Text = AppResources.poi_activity_description;
                    break;
                case ReportType.PER_VARIABLE_DISPLAY:
                    icon.Source = "ic_entities_per_variable_display.png";
                    title.Text = AppResources.report_title_per_variable_display;
                    description.Text = AppResources.per_variable_display_description;
                    break;
                case ReportType.VEHICLE_SCORE_RATING:
                    icon.Source = "ic_fleet_vehicle_scroe_rating.png";
                    title.Text = AppResources.report_title_vehicle_score;
                    description.Text = AppResources.vehicle_score_description;
                    break;
                //Fleet reports
                case ReportType.EXCEPTION_SUMMARY:
                    icon.Source = "ic_fleet_exception_summary.png";
                    title.Text = AppResources.report_title_fleet_exception_summary;
                    description.Text = AppResources.exception_summary_description;
                    break;
                case ReportType.ROUTE_ACTIVITY:
                    icon.Source = "ic_fleet_route_activity.png";
                    title.Text = AppResources.report_title_fleet_route_activity;
                    description.Text = AppResources.route_activity_description;
                    break;
                case ReportType.FLEET_UTILIZATION:
                    icon.Source = "ic_fleet_fleet_utilization.png";
                    title.Text = AppResources.report_title_fleet_utilization;
                    description.Text = AppResources.fleet_utilization_description;
                    break;
                case ReportType.TOTAL_COST_OF_OWNERSHIP:
                    icon.Source = "ic_fleet_total_cost_of_ownership.png";
                    title.Text = AppResources.report_title_fleet_total_cost_of_ownership;
                    description.Text = AppResources.total_cost_of_ownership_description;
                    break;
                case ReportType.ACCIDENT_SUMMARY:
                    icon.Source = "ic_fleet_accident.png";
                    title.Text = AppResources.report_title_fleet_accident_summary;
                    description.Text = AppResources.accident_summary_description;
                    break;
                case ReportType.CUSTOM_DATA_SUMMARY:
                    icon.Source = "ic_fleet_custom_data_summary.png";
                    title.Text = AppResources.report_title_fleet_custom_data_summary;
                    description.Text = AppResources.custom_data_summary_description;
                    break;
                case ReportType.DTC_CODE_MAINTENANCE:
                    icon.Source = "ic_fleet_dtc_code_maintenance.png";
                    title.Text = AppResources.report_title_fleet_dtc_code_maintenance;
                    description.Text = AppResources.dtc_code_maintenance_description;
                    break;
                case ReportType.OBD_ROUTE_DATA:
                    icon.Source = "ic_fleet_obd_route_data.png";
                    title.Text = AppResources.report_title_fleet_obd_route_data;
                    description.Text = AppResources.obd_route_data;
                    break;
                //Alert notifications
                case ReportType.ALERT_SUMMARY:
                    icon.Source = "ic_alert_notifications.png";
                    title.Text = AppResources.report_title_alert_summary;
                    description.Text = AppResources.alert_summary_description;
                    break;
                case ReportType.RISK_MANAGEMENT:
                    icon.Source = "ic_risk_management.png";
                    title.Text = AppResources.report_title_risk_management;
                    description.Text = AppResources.risk_management_description;
                    break;
                //Actions
                case ReportType.LOCATION_MONITOR:
                    icon.Source = "ic_risk_management.png";
                    title.Text = AppResources.report_title_location_monitor;
                    description.Text = AppResources.location_monitor_description;
                    break;

            }
            topLine.Children.Add(icon,0,0);
            topLine.Children.Add(title,1,0);

           
            mainStack.Children.Add(topLine);
            mainStack.Children.Add(description);
            //scrollLayout.Content = mainStack;

            if (!isInDevelopment)
                Content = mainStack;
            else
            {
                Margin = 0;
                var relativeLayout = new RelativeLayout { VerticalOptions = LayoutOptions.Fill, HorizontalOptions = LayoutOptions.Fill };
                relativeLayout.Children.Add(mainStack,
                    Constraint.Constant(0),
                    Constraint.Constant(0));

                var inProductionOverlay = new StackLayout { VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand };
                this.BackgroundColor = Palette.MainAccent.MultiplyAlpha(0.4);
                mainStack.BackgroundColor = Color.Transparent;
                inProductionOverlay.Children.Add(new Label { Text = AppResources.coming_soon, Rotation = -45, TextColor=Color.White, FontSize = 24, FontAttributes = FontAttributes.Bold, VerticalOptions = LayoutOptions.FillAndExpand, HorizontalOptions = LayoutOptions.FillAndExpand,HorizontalTextAlignment=TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center});

                relativeLayout.Children.Add(inProductionOverlay, Constraint.Constant(0),
                Constraint.Constant(0),
                Constraint.RelativeToParent((parent) =>
                {
                    return ( parent.Width);
                }),
                 Constraint.RelativeToParent((parent) =>
                 {
                     return (parent.Height);
                 }));

                Content = relativeLayout;
            }
        }

        private async void ClickGesture_Tapped(object sender, EventArgs e)
        {
            var tile = sender as ReportTile;
            //if (Device.Idiom == TargetIdiom.Phone && tile.ReportType != ReportType.LOCATION_MONITOR)
                //return;

            switch (tile.ReportType)
            {
                case (ReportType.DLT_DRIVER_STATS):
                    {
                        var page = new DLTDriverReport();
                        await Navigation.PushAsync(page);
                        break;
                    }
                case (ReportType.EXCEPTION_SUMMARY):
                    {
                        var page = new ExceptionSummaryPage();
                        await Navigation.PushAsync(page);
                        break;
                    }
                case (ReportType.ROUTE_ACTIVITY):
                    {
                        var page = new RouteActivityPage();
                        await Navigation.PushAsync(page);
                        break;
                    }
                case (ReportType.FLEET_UTILIZATION):
                    {
                        var page = new VehicleUtilisationPage();
                        await Navigation.PushAsync(page);
                        break;
                    }
                case (ReportType.FUEL_CONSUMPTION):
                    {
                        var page = new FuelEntryConsumption();
                        await Navigation.PushAsync(page);
                        break;
                    }
                case (ReportType.FUEL_COST):
                    {
                        var page = new FuelCostPage();
                        await Navigation.PushAsync(page);
                        break;
                    }
                case (ReportType.LOCATION_MONITOR):
                    {
                        var page = new LocationMonitorPage();
                        await Navigation.PushAsync(page);
                        break;
                    }
            }
            
        }
    }
}
