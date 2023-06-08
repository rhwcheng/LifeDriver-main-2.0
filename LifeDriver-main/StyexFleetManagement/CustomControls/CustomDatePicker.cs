using StyexFleetManagement.Resx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class CustomDatePicker : Picker
    {
        public CustomDatePicker(bool showCustom = true) : base()
        {
            this.Items.Add(AppResources.today);
            this.Items.Add(string.Format(AppResources.last_x_days, "7"));
            this.Items.Add(AppResources.this_month);
            this.Items.Add(AppResources.previous_month);
            if (showCustom)
                this.Items.Add(AppResources.custom);

            SelectedIndex = (int) App.SelectedDate;

            HorizontalOptions = LayoutOptions.FillAndExpand;
        }

        public CustomDatePicker() : base()
        {
            this.Items.Add(AppResources.today);
            this.Items.Add(string.Format(AppResources.last_x_days, "7"));
            this.Items.Add(AppResources.this_month);
            this.Items.Add(AppResources.previous_month);

            SelectedIndex = (int)App.SelectedDate;

            HorizontalOptions = LayoutOptions.FillAndExpand;
        }
    }

}
