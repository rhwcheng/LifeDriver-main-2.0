using System;
using System.Collections.Generic;
using StyexFleetManagement.Resx;
using Xamarin.Forms;

namespace StyexFleetManagement.Pages.AppSettings
{
	public partial class SettingsPageStandalone : ContentPage
    {
		public SettingsPageStandalone()
        {
            InitializeComponent();
            this.Title = AppResources.title_settings;
        }
    }
}
