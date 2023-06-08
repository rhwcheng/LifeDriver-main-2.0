using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyexFleetManagement.Abstractions
{
    public interface INotificationService
    {
        void SendNotification(string message, int notificationId);
        void CancelNotification(int notificationId);
    }
}
