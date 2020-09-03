using System.Collections.Generic;

namespace Vanjaro.Core.Entities
{
    public class NotificationsViewModel
    {
        public double TotalNotifications { get; set; }
        public IList<NotificationViewModel> Notifications { get; set; }
    }
}