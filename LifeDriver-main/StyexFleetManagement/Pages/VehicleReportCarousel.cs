using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using StyexFleetManagement.Models;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages
{
    public class VehicleReportCarousel : CarouselPage
    {

        private TimeUtilization utilizationData;
        private DrivingSummary drivingSummaryData;
        private OvertimeUtilization overtimeUtilizationData;

        private int dateIndex;
        private int vehicleIndex;

        public VehicleReportCarousel(TimeUtilization utilizationData, DrivingSummary drivingSummaryData, OvertimeUtilization overtimeUtilizationData, int datePickerIndex, int vehiclePickerIndex)
        {
            this.utilizationData = utilizationData;
            this.drivingSummaryData = drivingSummaryData;
            this.overtimeUtilizationData = overtimeUtilizationData;

            dateIndex = datePickerIndex;
            vehicleIndex = vehiclePickerIndex;

            var overtimeReport = new VehicleOvertimeDetailPage(overtimeUtilizationData, datePickerIndex, vehiclePickerIndex);
            var summaryReport = new VehicleSummaryDetailPage(drivingSummaryData, datePickerIndex, vehiclePickerIndex);
            var utilizationReport = new VehicleUtilizationDetailPage(utilizationData, datePickerIndex, vehiclePickerIndex);

            Children.Add(summaryReport);
            Children.Add(overtimeReport);
            Children.Add(utilizationReport);
        }

        protected override void OnCurrentPageChanged()
        {
            base.OnCurrentPageChanged();

        }
    }
}
