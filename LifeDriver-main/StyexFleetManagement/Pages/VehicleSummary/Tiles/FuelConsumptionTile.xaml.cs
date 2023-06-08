using System;
using System.Collections.Generic;
using Xamarin.Forms;
using StyexFleetManagement.CustomControls;

namespace StyexFleetManagement.Pages.VehicleSummary.Tiles
{
    public partial class FuelConsumptionTile : ContentView
    {
        public FuelConsumptionTile()
        {
            InitializeComponent();
            this.titleLabel.Text = "Fuel Used";
            this.headerImage.Source = "ic_fuel_used_tile.png";
        }
    }
}
