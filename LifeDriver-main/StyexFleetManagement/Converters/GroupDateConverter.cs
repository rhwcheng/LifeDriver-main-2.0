using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Models;
using Xamarin.Forms;

namespace StyexFleetManagement.Converters
{
    public class GroupDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var trip = value as Trip;

            var date = trip.StartLocalTimestamp;
            var groupingDate = new DateTime(date.Year, date.Month, date.Day);
            return groupingDate.ToString("dd MMM yyyy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
