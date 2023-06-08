using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using StyexFleetManagement.Models;
using Xamarin.Forms;

namespace StyexFleetManagement.ViewModel
{
    public class DLTDriverReportViewModel : BaseViewModel
    {
        List<DriverEvent> eventData;
        List<DriverData> topFiveDrivers;
        bool tLicenseOverlayIsVisible, bLicenseOverlayIsVisible, personalLicenseOverlayIsVisible;
        private List<string> driverIds;

        public ICommand UpdateEventDataCommand { get; private set; }

        public DLTDriverReportViewModel() : base()
        {
            eventData = new List<DriverEvent>();
            topFiveDrivers = new List<DriverData>();
            driverIds = new List<string>();
        }
        

        public List<DriverEvent> EventData
        {
            get => eventData;
            set
            {
                if (eventData == value)
                    return;
                eventData = value;
                
                OnPropertyChanged();
            }
        }

        public List<DriverData> TopFiveDrivers
        {
            get => topFiveDrivers;
            set
            {
                topFiveDrivers = value;
                OnPropertyChanged();
            }
        }

        public List<DriverData> GetTopFiveDrivers()
        {
            var result = eventData.Where(x => x.DriverData.DriverId != null).GroupBy(s => s.DriverData.DriverId).OrderByDescending(g => g.Count()).Select(grp => new { GroupID = grp.Key, Driver = grp.First(), Count = grp.Count() }).ToList();
            //Clear existing list of drivers
            topFiveDrivers.Clear();
            int count = Math.Min(5, result.Count);
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var driver = result[i].Driver.DriverData;
                    driver.NumberOfTrips = result[i].Count;
                    topFiveDrivers.Add(driver);
                }
            }

            if (count < 5)
            {
                var diff = 5 - count;
                for (int i = 0; i < diff; i++)
                {
                    topFiveDrivers.Add(null);
                }
            }
            return topFiveDrivers;
        }

        public bool TLicenseOverlayIsVisible
        {
            get => tLicenseOverlayIsVisible;
            set
            {
                if (tLicenseOverlayIsVisible == value)
                    return;
                tLicenseOverlayIsVisible = value;
                OnPropertyChanged();
            }
        }
        public bool BLicenseOverlayIsVisible
        {
            get => bLicenseOverlayIsVisible;
            set
            {
                if (bLicenseOverlayIsVisible == value)
                    return;
                bLicenseOverlayIsVisible = value;
                OnPropertyChanged();
            }
        }
        public bool PersonalLicenseOverlayIsVisible
        {
            get => personalLicenseOverlayIsVisible;
            set
            {
                if (personalLicenseOverlayIsVisible == value)
                    return;
                personalLicenseOverlayIsVisible = value;
                OnPropertyChanged();
            }
        }

        public List<string> DriverIds {
            get => driverIds;
            set
            {
                if (driverIds == value)
                    return;
                driverIds = value;
                //Not needed for now: OnPropertyChanged();
            }
        }
    }
}
