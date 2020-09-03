using static Vanjaro.Core.Factories;

namespace Vanjaro.Core
{
    public static partial class Managers
    {
        public class NotificationManager
        {
            #region INotificationTask
            public static int RenderNotificationsCount(int PortalID)
            {
                return NotificationTaskFactory.GetNotificationCount(PortalID);
            }
            #endregion
        }
    }
}