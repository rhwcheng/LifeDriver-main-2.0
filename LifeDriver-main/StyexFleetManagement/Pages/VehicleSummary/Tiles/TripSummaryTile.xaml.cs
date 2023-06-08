using System;
using System.Collections.Generic;
using Xamarin.Forms;
using StyexFleetManagement.CustomControls;

namespace StyexFleetManagement.Pages.VehicleSummary.Tiles
{
    public partial class TripSummaryTile : ContentView
    {
        public TripSummaryTile()
        {
            InitializeComponent();
            this.titleLabel.Text = "Trips Summary";
        }
    }
}
