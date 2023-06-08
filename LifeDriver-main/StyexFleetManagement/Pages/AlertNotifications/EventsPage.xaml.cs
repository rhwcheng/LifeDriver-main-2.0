﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StyexFleetManagement.CustomControls;
using StyexFleetManagement.Pages.AppSettings;
using StyexFleetManagement.Resx;
using StyexFleetManagement.Services;
using StyexFleetManagement.ViewModel;
using Syncfusion.SfDataGrid.XForms;
using Xamarin.Forms;

using Xamarin.Forms;
using SelectionMode = Syncfusion.SfDataGrid.XForms.SelectionMode;

namespace StyexFleetManagement.Pages.AlertNotifications
{
    public partial class EventsPage : ReportPage
    {
        private EventsViewModel viewModel;
        private SfDataGrid dataGrid;
        private GridTextColumn latColumn;
        private GridTextColumn longColumn;
        private GridTextColumn odoColumn;
        private GridTextColumn speedColumn;


        public EventsPage() : base(forceLandscape: false)
        {
            InitializeComponent();
            this.Title = "Events";
            if (Device.RuntimePlatform == Device.iOS)
                this.Icon = "baseline_warning_black_24";
            viewModel = new EventsViewModel();
            BindingContext = viewModel;

            SetupUI();

            this.ReportHeader.DatePickerLayout.PropertyChanged += Date_PropertyChanged;
            this.ReportHeader.SearchTapped += OnSearch_Tapped;

            SetupTable();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            if (Device.Idiom == TargetIdiom.Phone && DependencyService.Get<IDeviceOrientationService>().GetOrientation() == DeviceOrientation.Portrait)
            {
                latColumn.IsHidden = true;
                longColumn.IsHidden = true;
                odoColumn.IsHidden = true;
                speedColumn.IsHidden = true;
            }
            else
            {
                latColumn.IsHidden = false;
                longColumn.IsHidden = false;
                odoColumn.IsHidden = false;
                speedColumn.IsHidden = false;
            }

        }

        private async Task SetupTable()
        {
            App.ShowLoading(true);

            viewModel.Events.Clear();
            dataGrid.ItemsSource = null;

            await viewModel.LoadData();
            dataGrid.ItemsSource = viewModel.Events;
            noEventsLabel.IsVisible = (viewModel.Events == null || viewModel.Events.Count == 0);
            dataGrid.IsVisible = !noEventsLabel.IsVisible;

            App.ShowLoading(false);
        }

        private void SetupUI()
        {
            ToolbarItems.Add(new ToolbarItem
            {
                Icon = "ic_action_settings.png",
                Order = ToolbarItemOrder.Primary,
                Command = new Command(() => ((MainPage)App.MainDetailPage).NavigateTo(typeof(SettingsPage)))
            });

            double height = 0;

            switch (Device.Idiom)
            {
                case TargetIdiom.Phone:
                    height = 200;
                    break;
                case TargetIdiom.Tablet:
                    height = 250;
                    break;
            }


            //Set up data grid
            dataGrid = new SfDataGrid { HorizontalOptions = LayoutOptions.FillAndExpand, ColumnSizer = ColumnSizer.Star, AutoGenerateColumns = false, GridStyle = new GridStyle(), SelectionMode = SelectionMode.SingleDeselect, HeightRequest = height };

            GridTextColumn descriptionColumn = new GridTextColumn { };
            descriptionColumn.MappingName = "VehicleDescription";
            descriptionColumn.HeaderText = AppResources.vehicle_description;

            GridTextColumn alertDescColumn = new GridTextColumn { };
            alertDescColumn.MappingName = "EventTypeDescription";
            alertDescColumn.HeaderText = "Alert";

            GridTextColumn dateColumn = new GridTextColumn { };
            dateColumn.MappingName = "DateString";
            dateColumn.HeaderText = "Date";


            odoColumn = new GridTextColumn { };
            odoColumn.MappingName = "Odometer";
            odoColumn.HeaderText = "Odometer";


            speedColumn = new GridTextColumn { };
            speedColumn.MappingName = "Speed";
            speedColumn.HeaderText = "Speed";

            latColumn = new GridTextColumn { };
            latColumn.MappingName = "Lattitude";
            latColumn.HeaderText = "Lattitude";

            longColumn = new GridTextColumn { };
            longColumn.MappingName = "Longitude";
            longColumn.HeaderText = "Longitude";

            dataGrid.Columns.Add(descriptionColumn);
            dataGrid.Columns.Add(alertDescColumn);
            dataGrid.Columns.Add(dateColumn);
            dataGrid.Columns.Add(odoColumn);
            dataGrid.Columns.Add(speedColumn);
            dataGrid.Columns.Add(latColumn);
            dataGrid.Columns.Add(longColumn);

            if (Device.Idiom == TargetIdiom.Phone && DependencyService.Get<IDeviceOrientationService>().GetOrientation() == DeviceOrientation.Portrait)
            {
                latColumn.IsHidden = true;
                longColumn.IsHidden = true;
            }

            TableStack.Children.Add(dataGrid);

        }


        private void Date_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StartDate")
            {
                var value = (sender as DatePickerLayout).StartDate;
                if (viewModel.StartDate != value)
                {
                    viewModel.StartDate = value;
                }
                else
                    return;
            }
            if (e.PropertyName == "EndDate")
            {
                var value = (sender as DatePickerLayout).EndDate;
                if (viewModel.EndDate != value)
                {
                    viewModel.EndDate = value;
                }
                else
                    return;
            }
        }
        private async void OnSearch_Tapped(object sender, EventArgs e)
        {
            await SetupTable();
        }
    }
}
