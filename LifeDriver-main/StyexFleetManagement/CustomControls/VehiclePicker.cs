using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using StyexFleetManagement.Models;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class VehiclePicker : Picker, INotifyPropertyChanged
    {
        public string Selection { get; private set; }

        private string selectedVehicleGroup;
        public string SelectedVehicleGroup
        {
            get => selectedVehicleGroup;
            set
            {
                if (selectedVehicleGroup == value)
                    return;
                selectedVehicleGroup = value;
                
                App.SelectedVehicleGroup = value;

                OnPropertyChanged();
            }
        }

        public VehiclePicker() : base()
        {
            HorizontalOptions = LayoutOptions.FillAndExpand;
            BackgroundColor = Color.Transparent;

            //Populate Vehicle Picker
            foreach (VehicleGroup vehicleGroup in App.VehicleGroups.VehicleGroups)
            {
                Items.Add(vehicleGroup.Description);
            }

            int defaultIndex = Settings.Current.DefaultVehicleGroup;
            SelectedIndex = defaultIndex;
            SelectedVehicleGroup = App.VehicleGroups.FindIdFromDescription(Items[defaultIndex]);
            SelectedIndexChanged += VehiclePicker_SelectedIndexChanged;
        }

        public void FocusVehiclePicker()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                this.Focus();
            });
        }

        private void VehiclePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedIndex == -1)
            {
                App.SelectedVehicleGroup = null;
                App.SelectedVehicleGroupIndex = -1;
            }
            else
            {
                App.SelectedVehicleGroupIndex = SelectedIndex;
                Selection = Items[SelectedIndex];
                

                if (App.VehicleGroups != null)
                {
                    SelectedVehicleGroup = App.VehicleGroups.FindIdFromDescription(Selection);
                    
                }

            }
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName]string name = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;
            changed(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
