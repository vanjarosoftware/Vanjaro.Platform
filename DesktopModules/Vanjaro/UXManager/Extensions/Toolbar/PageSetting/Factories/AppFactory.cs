using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Toolbar.PageSetting.Factories
{
    public class AppFactory
    {
        private const string ModuleRuntimeVersion = "1.0.0";
        internal static string GetAllowedRoles(string Identifier)
        {
            AngularView template = GetViews().Where(t => t.Identifier == Identifier).FirstOrDefault();

            if (template != null)
            {
                return template.AccessRoles;
            }

            return string.Empty;
        }
        
        public static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>();
            AngularView setting = new AngularView
            {
                AccessRoles = "user,anonymous",
                UrlPaths = new List<string> {
                  "setting"
                },
                IsDefaultTemplate = true,
                TemplatePath = "setting/setting.html",
                Identifier = Identifier.setting_setting.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting);

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

            if (UserInfo.UserID > -1 && (UserInfo.IsInRole("Administrators")))
            {
                AccessRoles.Add("admin");
            }

            if (UserInfo.IsSuperUser)
            {
                AccessRoles.Add("host");
            }

            return string.Join(",", AccessRoles);
        }

        public static AppInformation GetAppInformation()
        {
            return new AppInformation("PageSetting", "PageSetting", ExtensionInfo.GUID, GetRuntimeVersion, "", "", 14, 7, new List<string> { "Domain", "Server" }, false);
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
            setting_setting, setting_permissions, setting_advanced
        }
    }
}