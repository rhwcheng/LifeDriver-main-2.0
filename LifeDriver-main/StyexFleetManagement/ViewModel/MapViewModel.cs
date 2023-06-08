using StyexFleetManagement.Map;
using StyexFleetManagement.Models;
using StyexFleetManagement.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using MapSpan = Xamarin.Forms.GoogleMaps.MapSpan;
using Position = Xamarin.Forms.GoogleMaps.Position;

namespace StyexFleetManagement.ViewModel
{
    public class MapViewModel : INotifyPropertyChanged
    {

        public MapViewModel()
        {
            Init();
        }

        private void Init(){
            this.VehicleList = new ObservableCollection<Vehicle>();
            this.AllVehicles = new ObservableCollection<Vehicle>();
        }

        public async Task PopulateAllVehicles()
        {
            AllVehicles = await VehicleAPI.GetAllVehicles(App.VehicleGroups.VehicleGroups);
        }

        private ObservableCollection<Vehicle> vehicleList;
        public ObservableCollection<Vehicle> VehicleList
        {
            get => vehicleList;
            set
            {
                if (vehicleList != value)
                {
                    vehicleList = value;
                    OnPropertyChanged("VehicleList");
                }
            }
        }

        private ObservableCollection<Vehicle> allVehicles;
        public ObservableCollection<Vehicle> AllVehicles
        {
            get => allVehicles;
            set
            {
                if (allVehicles != value)
                {
                    allVehicles = value;
                    OnPropertyChanged("AllVehicles");
                }
            }
        }

        private MapSpan _mapRegion;
        private Position _mapCenter;
        private ExtendedPin _selectedPin;
        private ObservableCollection<ExtendedPin> _pins;
        private bool newPinSelected;


        /// <summary>
        /// Map region bound to <see cref="TKCustomMap"/>
        /// </summary>
        public MapSpan MapRegion
        {
            get => this._mapRegion;
            set
            {
                if (this._mapRegion != value)
                {
                    this._mapRegion = value;
                    this.OnPropertyChanged("MapRegion");
                }
            }
        }


        public ObservableCollection<ExtendedPin> Pins
        {
            get { return _pins; }
            set
            {
                if (_pins != value)
                {
                    _pins = value;
                    OnPropertyChanged("Pins");
                }
            }
        }

        /// <summary>
        /// Map center bound to the <see cref="TKCustomMap"/>
        /// </summary>
        public Position MapCenter
        {
            get => this._mapCenter;
            set
            {
                if (this._mapCenter != value)
                {
                    this._mapCenter = value;

                    if (!newPinSelected)
                    {
                        this.SelectedPin = null;
                    }
                    else
                    {
                        newPinSelected = false;
                    }
                    this.OnPropertyChanged("MapCenter");
                }
            }
        }

        /// <summary>
        /// Selected pin bound to the <see cref="TKCustomMap"/>
        /// </summary>
        public ExtendedPin SelectedPin
        {
            get => this._selectedPin;
            set
            {
                if (this._selectedPin != value)
                {
                    this._selectedPin = value;
                    newPinSelected = true;
                    this.OnPropertyChanged("SelectedPin");
                }
            }
        }


        /// <summary>
        /// Command when a route calculation finished
        /// </summary>
        //public Command<TKRoute> RouteCalculationFinishedCommand
        //{
        //    get
        //    {
        //        return new Command<Route>(r =>
        //        {
        //            // move to the bounds of the route
        //            this.MapRegion = r.Bounds;
        //        });
        //    }
        //}

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}

