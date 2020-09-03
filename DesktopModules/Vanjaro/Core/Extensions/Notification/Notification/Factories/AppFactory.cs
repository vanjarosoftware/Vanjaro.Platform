using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.Core.Extensions.Notification.Notification.Factories
{
    public class AppFactory
    {
        private const string ModuleRuntimeVersion = "1.0.0";
        internal static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>();

            AngularView options = new AngularView
            {
                AccessRoles = "user",
                UrlPaths = new List<string> {
                  "tasks"
                },
                IsDefaultTemplate = true,
                TemplatePath = "notification/tasks.html",
                Identifier = Identifier.notification_tasks.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(options);

            AngularView notifications = new AngularView
            {
                AccessRoles = "user",
                UrlPaths = new List<string> {
                  "notifications"
                },
                TemplatePath = "notification/notifications.html",
                Identifier = Identifier.notification_notifications.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(notifications);
            return Views;
        }

        internal static AppInformation GetAppInformation()
        {
            return new AppInformation(NotificationInfo.Name, NotificationInfo.FriendlyName, NotificationInfo.GUID, GetRuntimeVersion, "http://www.mandeeps.com/store", "http://www.mandeeps.com/Activation", 14, 7, new List<string> { "Domain", "Server" }, false);
        }
        internal static string GetRuntimeVersion
        {
            get
            {
                try
                {
                    return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                catch { }

                return ModuleRuntimeVersion;
            }
        }

        public AppInformation AppInformation => GetAppInformation();

        internal static string GetAllowedRoles(string Identifier)
        {
            AngularView template = GetViews().Where(t => t.TemplatePath.StartsWith(Identifier.Replace("_", "/"))).FirstOrDefault();

            if (template != null)
            {
                return template.AccessRoles;
            }

            return string.Empty;
        }
        internal static string GetAccessRoles(UserInfo UserInfo)
        {
            List<string> AccessRoles = new List<string>();

            if (UserInfo.UserID > 0)
            {
                AccessRoles.Add("user");
            }
            else
            {
                AccessRoles.Add("anonymous");
            }

            if (UserInfo.IsSuperUser)
            {
                AccessRoles.Add("host");
                AccessRoles.Add("review");
            }

            if (UserInfo.UserID > -1 && (UserInfo.IsInRole("Administrators")))
            {
                AccessRoles.Add("admin");
            }
            return string.Join(",", AccessRoles.Distinct());
        }

        public enum Identifier
        {
            notification_tasks, notification_notifications
        }
    }
}