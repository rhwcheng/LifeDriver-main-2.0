using Syncfusion.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Models;

namespace StyexFleetManagement.Converters
{
    public class CustomComparer : IComparer<object>, ISortDirection
    {
        public int Compare(object x, object y)
        {
            DateTimeOffset namX;
            DateTimeOffset namY;

            //For OrderInfo type data
            if (x.GetType() == typeof(Trip))
            {
                //Calculating the length of FirstName in OrderInfo objects
                namX = ((Trip)x).StartLocalTimestamp;
                namY = ((Trip)y).StartLocalTimestamp;
            }

            //For Group type data                                   
            else
            {
                if (x.GetType() == typeof(Group))
                {
                    //Calculating the group key length
                    namX = DateTimeOffset.Parse(((Group)x).Key.ToString());
                    namY = DateTimeOffset.Parse(((Group)y).Key.ToString());
                }
            }

            // Objects are compared and return the SortDirection
            if (namX.CompareTo(namY) > 0)
                return SortDirection == ListSortDirection.Ascending ? 1 : -1;
            if (namX.CompareTo(namY) == -1)
                return SortDirection == ListSortDirection.Ascending ? -1 : 1;
            else
                return 0;
        }

        //Gets or sets the SortDirection value
        public ListSortDirection SortDirection { get; set; }
    }
}
