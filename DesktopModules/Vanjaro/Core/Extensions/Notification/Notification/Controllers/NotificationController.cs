using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Core.Extensions.Notification.Notification.Managers;

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

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}