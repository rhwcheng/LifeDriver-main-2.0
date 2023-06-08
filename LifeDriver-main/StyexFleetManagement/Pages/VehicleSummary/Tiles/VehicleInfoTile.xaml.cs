using System;
using System.Collections.Generic;
using Acr.UserDialogs;
using StyexFleetManagement.CustomControls;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages.VehicleSummary.Tiles
{
    public partial class VehicleInfoTile : ContentView
    {
        public VehicleInfoTile()
        {
            InitializeComponent();
            titleLabel.Text = "Vehicle Information";
        }

    }
}
