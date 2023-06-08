using System;
using StyexFleetManagement.Helpers;
using Xamarin.Forms;

namespace StyexFleetManagement.Models
{
    public class VehicleUtilisationInfo
    {
        public string VehicleId { get; set; }
        public string VehicleDescription { get; set; }
        public string UnitId { get; set; }
        public double Distance { get; set; }
        public string DistanceTotalAndAverage { get; set; }
        public string DurationTotalAndAverage { get; set; }

        public string DistanceAsString => FormatHelper.FormatDistance(Math.Round(Distance,2).ToString()).ToString();
        public double Duration { get; set; }
        public string DurationAsString
        {
            get
            {
                var timespan = TimeSpan.FromSeconds(Duration);
                return FormatHelper.ToShortForm(timespan).ToString();
            }
        }
        public TimeSpan LastReportedTime { get; set; }
        public string LastReportedTimeAsString => FormatHelper.ToShortForm(LastReportedTime).ToString();
        public string UtilisationPercentage { get; set; }
        public Result Result { get; set; }
        
        public ImageSource ResultImage
        {
            get
            {
                switch (Result)
                {
                    case Result.Increased:
                        return ImageSource.FromFile("ic_increase.png");
                    case Result.Decreased:
                        return ImageSource.FromFile("ic_decrease.png");
                    default:
                    case Result.Same:
                        return ImageSource.FromFile("ic_same.png");
                }
            }
        }

        public VehicleUtilisationInfo()
        {
            Result = Result.Same;
        }
    }

    public enum Result
    {
        Increased,
        Decreased,
        Same
    }
}
