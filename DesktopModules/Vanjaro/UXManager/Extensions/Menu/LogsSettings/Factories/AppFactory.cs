using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Apps.LogsSettings.Factories
{
    public class AppFactory
    {
        private const string ModuleRuntimeVersion = "1.0.0";

        public static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>();
            AngularView setting_logSetting = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "logSetting"
                },
                IsDefaultTemplate = true,
                TemplatePath = "setting/logSetting.html",
                Identifier = Identifier.setting_logSetting.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting_logSetting);

            AngularView setting_editlogsetting = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "editlogsetting",
                  "editlogsetting/:id"
                },
                TemplatePath = "setting/editlogsetting.html",
                Identifier = Identifier.setting_editlogsetting.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting_editlogsetting);

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

            if (UserInfo.UserID > -1 && UserInfo.IsInRole("Administrators"))
            {
                AccessRoles.Add("admin");
            }

            return string.Join(",", AccessRoles.Distinct());
        }

        internal static string GetAllowedRoles(string Identifier)
        {
            AngularView template = GetViews().Where(t => t.Identifier == Identifier).FirstOrDefault();

            if (template != null)
            {
                return template.AccessRoles;
            }

            return string.Empty;
        }

        public static AppInformation GetAppInformation()
        {
            return new AppInformation(ExtensionInfo.Name, ExtensionInfo.FriendlyName, ExtensionInfo.GUID, GetRuntimeVersion, "http://www.mandeeps.com/store", "http://www.mandeeps.com/Activation", 14, 7, new List<string> { "Domain", "Server" }, false);
        }

        public AppInformation AppInformation => GetAppInformation();

        public static string GetRuntimeVersion
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

        public enum Identifier
        {
            setting_logSetting, setting_editlogsetting
        }
    }
}