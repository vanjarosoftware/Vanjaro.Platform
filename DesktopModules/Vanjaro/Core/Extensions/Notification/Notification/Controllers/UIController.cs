using DotNetNuke.Services.Social.Notifications;
using System;
using System.Collections.Generic;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Core.Extensions.Notification.Notification.Factories;
using Vanjaro.Core.Extensions.Notification.Notification.Managers;

namespace Vanjaro.Core.Extensions.Notification.Notification.Controllers
{
    public class UIController : UIEngineController
    {
        public override List<IUIData> GetData(string Identifier, Dictionary<string, string> Parameters)
        {
            List<IUIData> result = new List<IUIData>();
            switch ((AppFactory.Identifier)Enum.Parse(typeof(AppFactory.Identifier), Identifier))
            {
                case AppFactory.Identifier.notification_tasks:
                    result = NotificationController.GetData(PortalSettings.PortalId, UserInfo, Identifier);
                    break;
                case AppFactory.Identifier.notification_notifications:
                    result = NotificationController.GetData(PortalSettings.PortalId, UserInfo, Identifier);
                    break;
                default:
                    break;
            }
            result.Add(new UIData { Name = "TasksCount", Value = TasksManager.GetNotificationCount(PortalSettings.PortalId).ToString() });
            result.Add(new UIData { Name = "NotificationsCount", Value = NotificationsController.Instance.CountNotifications(UserInfo.UserID, PortalSettings.PortalId).ToString() });
            return result;
        }
        public override string AccessRoles()
        {
            return AppFactory.GetAccessRoles(UserInfo);
        }
        public override string AllowedAccessRoles(string Identifier)
        {
            return AppFactory.GetAllowedRoles(Identifier);
        }
    }
}