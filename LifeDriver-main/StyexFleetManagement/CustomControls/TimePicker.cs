using Syncfusion.SfPicker.XForms;
using System.Collections.ObjectModel;
using StyexFleetManagement.Resx;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class TimePicker : SfPicker
    {
        public ObservableCollection<object> Time { get; set; }

        public ObservableCollection<object> Minutes;
        public ObservableCollection<object> Hours;
        public ObservableCollection<object> Seconds;

        public ObservableCollection<string> Headers { get; set; }

        public TimePicker()

        {
            Time = new ObservableCollection<object>();
            Hours = new ObservableCollection<object>();
            Minutes = new ObservableCollection<object>();
            Seconds = new ObservableCollection<object>();
            Headers = new ObservableCollection<string>();

            PopulateTimeCollection();
            PopulateHeaders();

            ItemsSource = Time;

            ColumnHeaderText = Headers;
            ShowColumnHeader = true;
            ShowHeader = false;
            ShowFooter = false;
            SelectedItemTextColor = Color.Black;
        }

        private void PopulateTimeCollection()

        {
            //Populate Hours
            for (var i = 0; i < 24; i++)
            {
                Hours.Add(i < 10 ? $"0{i}" : i.ToString());
            }

            //Populate Minutes and Seconds
            for (var i = 0; i < 60; i++)
            {
                Minutes.Add(i < 10 ? $"0{i}" : i.ToString());
                Seconds.Add(i < 10 ? $"0{i}" : i.ToString());
            }

            Time.Add(Hours);
            Time.Add(Minutes);
            Time.Add(Seconds);
        }

        private void PopulateHeaders()
        {
            Headers.Add(AppResources.hours.ToUpperInvariant());
            Headers.Add(AppResources.minutes.ToUpperInvariant());
            Headers.Add(AppResources.seconds.ToUpperInvariant());
        }
    }
}