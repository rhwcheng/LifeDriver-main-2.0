using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using StyexFleetManagement.Abstractions;
using StyexFleetManagement.Droid.Services;

[assembly: Xamarin.Forms.Dependency(typeof(NotificationService))]
namespace StyexFleetManagement.Droid.Services
{
    public class NotificationService : INotificationService
    {
        public void SendNotification(string message, int notificationId)
        {
            AppDroid.Current.LocationService.UpdateNotification(message, notificationId);
        }
        public void CancelNotification(int notificationId)
        {
            AppDroid.Current.LocationService.CancelNotification(notificationId);
        }
    }
}