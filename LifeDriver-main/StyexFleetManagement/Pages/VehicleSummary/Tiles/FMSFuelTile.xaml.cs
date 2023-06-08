using System;
using System.Collections.Generic;
using Xamarin.Forms;
using StyexFleetManagement.CustomControls;

namespace StyexFleetManagement.Pages.VehicleSummary.Tiles
{
    public partial class FMSFuelTile : ContentView
    {
        public FMSFuelTile()
        {
            InitializeComponent();
            this.titleLabel.Text = "FMS Fuel Consumption";
            this.headerImage.Source = "ic_fuel_used_tile.png";
        }
    }
}
