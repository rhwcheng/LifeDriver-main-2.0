using System;
using System.Collections.Generic;
using Xamarin.Forms;
using StyexFleetManagement.CustomControls;

namespace StyexFleetManagement.Pages.VehicleSummary.Tiles
{
    public partial class RecentViolationsTile : ContentView
    {
        public RecentViolationsTile()
        {
			InitializeComponent();
            this.titleLabel.Text = "Recent Violations";
        }
    }
}
