using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Helpers;
using StyexFleetManagement.Models;

namespace StyexFleetManagement.ViewModel
{
    public class TripModel : Trip, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public string LocalStartTime => StartLocalTimestamp.LocalDateTime.ToString("HH:mm tt") ?? "";


        public string LocalEndTime => EndLocalTimestamp.LocalDateTime.ToString("HH:mm tt") ?? "";

        public string DistanceString
        {
            get
            {
                decimal distance = Math.Round(Convert.ToDecimal(Distance), 2);
                return ((string) FormatHelper.FormatDistance(distance.ToString()));
            }
        }
        public string DurationString
        {
            get
            {
                TimeSpan duration = EndLocalTimestamp - StartLocalTimestamp;
                return FormatHelper.ToShortForm(duration).ToString();
            }
        }
    }

    public class GroupedTripModel : ObservableCollection<TripModel>
    {
        public string Date { get; set; }
    }
}
