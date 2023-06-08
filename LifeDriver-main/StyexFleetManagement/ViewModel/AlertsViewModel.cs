using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using StyexFleetManagement.Services;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using StyexFleetManagement.Models;

namespace StyexFleetManagement.ViewModel
{
	public class AlertsViewModel : BaseViewModel
	{
        public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public ReportDateRange SelectedDate { get; set; }
		public List<Alert> Alerts {get;set;}
        private SemaphoreSlim throttler;
        

        public AlertsViewModel()
		{
            SelectedDate = App.SelectedDate;
            StartDate = App.StartDateSelected;
			EndDate = App.EndDateSelected;
			Alerts = new List<Alert>();
			throttler = new SemaphoreSlim(5);
        }

        public async Task LoadData()
        {
            var vehicles = await VehicleAPI.GetVehicleInGroupAsync(App.SelectedVehicleGroup);
            var allTasks = new List<Task>();

			foreach (var vehicle in vehicles){
                await throttler.WaitAsync();
				allTasks.Add(
                                Task.Run(async () =>
                                {
                                    try
                                    {
                        			    var vehicleAlerts = await AlertsAPI.GetAlertsByVehicle(vehicle.Id, StartDate, EndDate);
                                        foreach (var vehicleAlert in vehicleAlerts){
                                            vehicleAlert.VehicleDescription = vehicle.Description;
                                        }
                                        Alerts.AddRange(vehicleAlerts);
                                    }
                                    finally
                                    {
                                        throttler.Release();
                                    }
                                }
					                    ));
			}

                await Task.WhenAll(allTasks);

			Alerts = Alerts.OrderByDescending(x => x.LocalTimestamp.DateTime)
                           .ThenBy(x => x.VehicleDescription).ToList();
			    return;
                               
                
            }
        }
}