using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Social.Messaging.Internal;
using DotNetNuke.Services.Social.Notifications;
using System;
using System.Collections.Generic;
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


            public static void Dismiss(int NotificationId, int UserID)
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(NotificationId, UserID);
                if (recipient != null)
                {
                    NotificationsController.Instance.DeleteNotificationRecipient(NotificationId, UserID);

                    #region Clear Notification count cache
                    var cacheKey = string.Format(DataCache.UserNotificationsCountCacheKey, PortalSettings.Current.PortalId, UserID);
                    CachingProvider.Instance().Clear("Prefix", cacheKey);
                    #endregion
                }
            }
            #endregion
        }
    }
}