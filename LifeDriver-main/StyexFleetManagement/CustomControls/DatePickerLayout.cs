using FFImageLoading.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Services;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class DatePickerLayout : StackLayout, INotifyPropertyChanged
    {
        private DatePicker startDatePicker;
        private DatePicker endDatePicker;
        private Grid customDateStack;
        private CustomDatePicker datePicker;
        private DateTime startDate;
        private DateTime endDate;
        public bool isDashboardTile { get; set; }

        public DateTime StartDate {
            get => startDate;
            set
            {
                if (startDate == value)
                    return;
                startDate = value;

                //Set app-wide date
                App.StartDateSelected = value;

                OnPropertyChanged();
            }
        }
        public DateTime EndDate {
            get => endDate;
            set
            {
                if (endDate == value)
                    return;
                endDate = value;

                App.EndDateSelected = value;

                OnPropertyChanged();
            }
        }

        private ReportDateRange selectedDate;
        public ReportDateRange SelectedDate
        {
            get => selectedDate;

            set {
                if (selectedDate == value)
                    return;
                selectedDate = value;

                if (!isDashboardTile)
                    App.SelectedDate = value;
                else
                {
                    switch (value)
                    {
                        case ReportDateRange.TODAY:
                            datePicker.SelectedIndex = 0;
                            break;
                        case ReportDateRange.LAST_SEVEN_DAYS:
                            datePicker.SelectedIndex = 1;
                            break;
                        case ReportDateRange.THIS_MONTH:
                            datePicker.SelectedIndex = 2;
                            break;
                        case ReportDateRange.PREVIOUS_MONTH:
                            datePicker.SelectedIndex = 3;
                            break;
                    }
                }

                OnPropertyChanged();
            }
        }

        public void FocusDatePicker()
        {
            datePicker.Focus();
        }

        public DatePickerLayout(bool showCustom = true, bool dashboardTile = false)
        {
            Orientation = StackOrientation.Horizontal;

            StartDate = App.StartDateSelected;
            EndDate = App.EndDateSelected;
            //StartDate = DateHelper.GetDateRangeStartDate(ReportDateRange.LAST_SEVEN_DAYS);
            //EndDate = DateHelper.GetDateRangeEndDate(ReportDateRange.LAST_SEVEN_DAYS);

            isDashboardTile = dashboardTile;

            //Set up picker elements
            datePicker = new CustomDatePicker (showCustom) { VerticalOptions = LayoutOptions.Center };
            datePicker.SelectedIndexChanged += DatePicker_SelectedIndexChanged;

            var dateImage = new CachedImage { Source = "ic_calendar", Aspect = Aspect.AspectFit };
            var datePickerSwitchGesture = new TapGestureRecognizer();
            datePickerSwitchGesture.Tapped += DatePickerSwitchGesture_Tapped;
            dateImage.GestureRecognizers.Add(datePickerSwitchGesture);

            startDatePicker = new DatePicker
            {
                Format = "dd MMM yy",
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
            startDatePicker.Date = StartDate;

            startDatePicker.DateSelected += StartDatepicker_DateSelected;

            endDatePicker = new DatePicker
            {
                Format = "dd MMM yy",
                VerticalOptions = LayoutOptions.CenterAndExpand
            };
            endDatePicker.Date = EndDate;

            endDatePicker.DateSelected += EndDatepicker_DateSelected;

            //set max dates for pickers
            startDatePicker.MaximumDate = endDatePicker.Date;
            endDatePicker.MaximumDate = DateTime.Now;
            endDatePicker.MinimumDate = startDatePicker.Date;

            customDateStack = new Grid
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                IsVisible = false
            };
            customDateStack.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
            customDateStack.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            customDateStack.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });

            customDateStack.Children.Add(startDatePicker, 0, 0);
            customDateStack.Children.Add(new Label { Text = "-", FontAttributes = FontAttributes.Bold, HorizontalTextAlignment = TextAlignment.Start, VerticalTextAlignment = TextAlignment.Center, HorizontalOptions = LayoutOptions.CenterAndExpand }, 1, 0);
            customDateStack.Children.Add(endDatePicker, 2, 0);

            Children.Add(dateImage);
            Children.Add(datePicker);
            Children.Add(customDateStack);
        }

        private void DatePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (datePicker.SelectedIndex == -1)
            {
                if (!isDashboardTile)
                    App.SelectedDate = ReportDateRange.TODAY;
            }
            else
            {
                switch (datePicker.SelectedIndex)
                {
                    case 0:
                        if (!isDashboardTile)
                            App.SelectedDate = ReportDateRange.TODAY;
                        SelectedDate = ReportDateRange.TODAY;
                        break;
                    case 1:
                        if (!isDashboardTile)
                            App.SelectedDate = ReportDateRange.LAST_SEVEN_DAYS;
                        SelectedDate = ReportDateRange.LAST_SEVEN_DAYS;
                        break;
                    case 2:
                        if (!isDashboardTile)
                            App.SelectedDate = ReportDateRange.THIS_MONTH;
                        SelectedDate = ReportDateRange.THIS_MONTH;
                        break;
                    case 3:
                        if (!isDashboardTile)
                            App.SelectedDate = ReportDateRange.PREVIOUS_MONTH;
                        SelectedDate = ReportDateRange.PREVIOUS_MONTH;
                        break;
                    case 4:
                        if (!isDashboardTile)
                            App.SelectedDate = ReportDateRange.CUSTOM;
                        SelectedDate = ReportDateRange.CUSTOM;
                        break;
                }

                if (customDateStack.IsVisible)
                {
                    return;
                }
                if (App.SelectedDate == ReportDateRange.CUSTOM)
                {
                    //Show custom picker
                    DatePickerSwitchGesture_Tapped(null, null);
                }
                else
                {
                    var oldStartDate = StartDate;
                    var oldEndDate = EndDate;

                    StartDate = DateHelper.GetDateRangeStartDate(App.SelectedDate);
                    EndDate = DateHelper.GetDateRangeEndDate(App.SelectedDate);


                    DateChangedEventArgs startDateEventArgs = new DateChangedEventArgs(oldStartDate, StartDate);
                    DateChangedEventArgs endDateEventArgs = new DateChangedEventArgs(oldEndDate, EndDate);

                    StartDatepicker_DateSelected(datePicker, startDateEventArgs);
                    EndDatepicker_DateSelected(datePicker, endDateEventArgs);

                }


            }

        }

        private void DatePickerSwitchGesture_Tapped(object sender, EventArgs e)
        {
            if (customDateStack.IsVisible == false)
            {
                datePicker.SelectedIndex = (int)ReportDateRange.CUSTOM;
                datePicker.IsVisible = false;
                customDateStack.IsVisible = true;
            }
            else
            {
                datePicker.IsVisible = true;
                customDateStack.IsVisible = false;

            }
        }

        private void StartDatepicker_DateSelected(object sender, DateChangedEventArgs e)
        {

            if (startDatePicker.Date != e.NewDate)
            {
                startDatePicker.Date = e.NewDate;
            }
            endDatePicker.MaximumDate = DateTime.Now;
            endDatePicker.MinimumDate = startDatePicker.Date;

            StartDate = startDatePicker.Date.Date;
            EndDate = endDatePicker.Date.Date.AddDays(1).AddSeconds(-1);
        }
        private void EndDatepicker_DateSelected(object sender, DateChangedEventArgs e)
        {

            if (endDatePicker.Date != e.NewDate)
            {
                endDatePicker.Date = e.NewDate;
            }
            startDatePicker.MaximumDate = endDatePicker.Date;

            StartDate = startDatePicker.Date.Date;
            EndDate = endDatePicker.Date.Date.AddDays(1).AddSeconds(-1);
        }

        public CustomDatePicker DatePicker => datePicker;

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;
      
        protected void OnPropertyChanged([CallerMemberName]string name = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;
            changed(this, new PropertyChangedEventArgs(name));
        }

        #endregion


    }
}
