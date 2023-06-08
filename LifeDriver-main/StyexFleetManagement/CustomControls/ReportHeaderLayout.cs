using FFImageLoading.Forms;
using StyexFleetManagement.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class ReportHeaderLayout : StackLayout
    {
        private int searchButtonIndex;
        private View additionalView;
        private StackLayout secondRow;
        private StackLayout innerStack;

        public DatePickerLayout DatePickerLayout { get; private set; }
        public event EventHandler SearchTapped;

        public ReportHeaderLayout() : base()
        {
            SetupLayout();
            AddSearchButton(2);
        }
        public ReportHeaderLayout(bool showDatePicker = true) : base()
        {
            SetupLayout(showDatePicker);
            AddSearchButton(2);
        }

        private void SetupLayout(bool showDatePicker = true)
        {
            WidthRequest = App.PageWidth;
            Orientation = StackOrientation.Horizontal;
            HorizontalOptions = LayoutOptions.FillAndExpand;
            WidthRequest = App.PageWidth;

            innerStack = new StackLayout { HorizontalOptions = LayoutOptions.FillAndExpand };
            DatePickerLayout = new DatePickerLayout();

            if (Device.Idiom == TargetIdiom.Phone && DependencyService.Get<IDeviceOrientationService>().GetOrientation() == DeviceOrientation.Portrait)
            {
                innerStack.Orientation = StackOrientation.Vertical;
            }
            else
            {
                innerStack.Orientation = StackOrientation.Horizontal;
            }

            if (showDatePicker)
            {
                innerStack.Children.Add(DatePickerLayout);
            }

            var vehiclePickerLayout = new VehiclePickerLayout();

            secondRow = new StackLayout { HorizontalOptions = LayoutOptions.FillAndExpand, Orientation = StackOrientation.Horizontal };
            secondRow.Children.Add(vehiclePickerLayout);

            innerStack.Children.Add(secondRow);
           

            App.GetInstance().PropertyChanged += ReportHeaderLayout_PropertyChanged;

            Children.Add(innerStack);

        }

        private void ReportHeaderLayout_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Orientation" && Device.Idiom == TargetIdiom.Phone)
            {
                if (DependencyService.Get<IDeviceOrientationService>().GetOrientation() == DeviceOrientation.Portrait)
                {
                    innerStack.Orientation = StackOrientation.Vertical;
                }
                else
                {
                    innerStack.Orientation = StackOrientation.Horizontal;
                }
            }
        }

        private void AddSearchButton(int columnIndex)
        {
            searchButtonIndex = columnIndex;

            var searchButton = new CachedImage { Source = "ic_search", HorizontalOptions = LayoutOptions.End };
            var searchGesture = new TapGestureRecognizer();
            searchGesture.Tapped += SearchGesture_Tapped; ;
            searchButton.GestureRecognizers.Add(searchGesture);
            
            Children.Add(searchButton);
        }

        public ReportHeaderLayout(View view, bool showDatePicker = true) : base()
        {
            SetupLayout(showDatePicker);

            additionalView = view;

            secondRow.Children.Add(view);

            AddSearchButton(3);
        }
        public ReportHeaderLayout(Collection<View> views, bool showDatePicker = true) : base()
        {
            SetupLayout(showDatePicker);

            foreach (var view in views)
            {
                secondRow.Children.Add(view);
            }
            

            AddSearchButton(2 + views.Count());
        }

        private void SearchGesture_Tapped(object sender, EventArgs e)
        {
            var handler = SearchTapped;
            if (handler == null)
                return;
            handler(this, e);
        }
    }
}
