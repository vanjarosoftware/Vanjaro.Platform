using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using DotNetNuke.Web.InternalServices;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Services.Social.Messaging.Internal;
using Vanjaro.Core.Extensions.Notification.Notification.Managers;
using static Vanjaro.Core.Managers;
using static Vanjaro.Core.Factories;

namespace Vanjaro.Core.Extensions.Notification.Notification.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "user")]
    public class NotificationController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID, UserInfo userInfo, string identifier)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            switch (identifier)
            {
                case "notification_tasks":
                    {
                        Settings.Add("Notifications", new UIData { Name = "Notifications", Options = TasksManager.GetBaseModel(PortalID) });
                        break;
                    }
            }
            return Settings.Values.ToList();
        }

        [HttpGet]
        public dynamic GetNotificationList(int Page, int PageSize)
        {
            return TasksManager.GetNotifications(Page, PageSize);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public dynamic Dismiss(NotificationDTO postData)
        {
            dynamic response = new ExpandoObject();
            response.IsSuccess = false;
            try
            {
                NotificationManager.Dismiss(postData.NotificationId, this.UserInfo.UserID);
                response.NotifyCount = Core.Managers.NotificationManager.RenderNotificationsCount(PortalSettings.PortalId);
                response.NotificationsCount = NotificationsController.Instance.CountNotifications(this.UserInfo.UserID, PortalSettings.PortalId);
                response.IsSuccess = true;
                CacheFactory.Clear(CacheFactory.Keys.NotificationTask + "PortalID" + PortalSettings.PortalId);
            }
            catch (Exception exc)
            {
                ExceptionManager.LogException(exc);
            }
            return response;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public dynamic DismissAll()
        {
            dynamic response = new ExpandoObject();
            response.IsSuccess = false;
            try
            {
                int TotalNotifications = NotificationsController.Instance.CountNotifications(UserInfo.UserID, this.PortalSettings.PortalId);
                IEnumerable<DotNetNuke.Services.Social.Notifications.Notification> notificationsDomainModel = NotificationsController.Instance.GetNotifications(UserInfo.UserID, this.PortalSettings.PortalId, -1, TotalNotifications);
                foreach (DotNetNuke.Services.Social.Notifications.Notification Notification in notificationsDomainModel)
                {
                    NotificationManager.Dismiss(Notification.NotificationID, this.UserInfo.UserID);
                }
                response.TotalNotifications = TotalNotifications;
                response.IsSuccess = true;
                CacheFactory.Clear(CacheFactory.Keys.NotificationTask + "PortalID" + PortalSettings.PortalId);
            }
            catch (Exception exc)
            {
                ExceptionManager.LogException(exc);
            }
            return response;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}