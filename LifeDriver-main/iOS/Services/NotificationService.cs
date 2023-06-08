using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;
using StyexFleetManagement.Abstractions;
using UserNotifications;
using StyexFleetManagement.iOS.Services;

[assembly: Xamarin.Forms.Dependency(typeof(NotificationService))]
namespace StyexFleetManagement.iOS.Services
{
    public class NotificationService : INotificationService
    {
        public NotificationService() { }


        public void SendNotification(string message, int notificationId)
        {
            // Get current notification settings
            UNUserNotificationCenter.Current.GetNotificationSettings((settings) => {
                var alertsAllowed = (settings.AlertSetting == UNNotificationSetting.Enabled);
                if (!alertsAllowed)
                    return;
            });

            var content = new UNMutableNotificationContent();
            content.Title = "Notification";
            content.Body = message;

            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);

            var requestID = notificationId.ToString();
            var request = UNNotificationRequest.FromIdentifier(requestID, content, trigger);

            UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) => {
                if (err != null)
                {
                    // Do something with error...
                }
            });
        }
        public void CancelNotification(int notificationId)
        {
            var requests = new string[] { notificationId.ToString() };
            UNUserNotificationCenter.Current.RemoveDeliveredNotifications(requests);
        }

    }
}