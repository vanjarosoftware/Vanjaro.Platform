using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Menu.Scheduler.Factories
{
    public class AppFactory
    {
        private const string ModuleRuntimeVersion = "1.0.0";
        internal static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>();

            AngularView setting_scheduler = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "scheduler",
                },

                TemplatePath = "setting/scheduler.html",
                Identifier = Identifier.setting_scheduler.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting_scheduler);

            AngularView setting_TaskQueue = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "taskqueue",
                },
                IsDefaultTemplate = true,
                TemplatePath = "setting/taskqueue.html",
                Identifier = Identifier.setting_taskqueue.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting_TaskQueue);

            AngularView setting_history = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "history",
                },
                TemplatePath = "setting/history.html",
                Identifier = Identifier.setting_history.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting_history);

            AngularView setting_settings = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "settings",
                },
                TemplatePath = "setting/settings.html",
                Identifier = Identifier.setting_settings.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting_settings);

            return Views;
        }

        public static string GetAccessRoles(UserInfo UserInfo)
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
            }

            if (UserInfo.UserID > -1 && (UserInfo.IsInRole("Administrators")))
            {
                AccessRoles.Add("admin");
            }
            return string.Join(",", AccessRoles.Distinct());
        }

        internal static AppInformation GetAppInformation()
        {
            return new AppInformation(ExtensionInfo.Name, ExtensionInfo.FriendlyName, ExtensionInfo.GUID, GetRuntimeVersion, "http://www.mandeeps.com/store", "http://www.mandeeps.com/Activation", 14, 7, new List<string> { "Domain", "Server" }, false);
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

        internal enum Identifier
        {
            setting_scheduler, setting_taskqueue, setting_history, common_controls_url, common_controls_editorconfig, setting_settings
        }
    }
}