using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Apps.SectionSettings.Factories
{
    public class AppFactory
    {
        private const string ModuleRuntimeVersion = "1.0.0";
        public AppInformation AppInformation => GetAppInformation();
        internal static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>();
            AngularView Options = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "permissions/:entityid/:entity",
                },
                IsDefaultTemplate = false,
                TemplatePath = "settings/permission.html",
                Identifier = Identifier.settings_permission.ToString(),
                Defaults = new Dictionary<string, string>()
            };
            Views.Add(Options);
            return Views;
        }
        internal static AppInformation GetAppInformation()
        {
            return new AppInformation(SettingInfo.Name, SettingInfo.FriendlyName, SettingInfo.GUID, GetRuntimeVersion, "http://www.mandeeps.com/store", "http://www.mandeeps.com/Activation", 14, 7, new List<string> { "Domain", "Server" }, false);
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
            }

            if (UserInfo.UserID > -1 && (UserInfo.IsInRole("Administrators")))
            {
                AccessRoles.Add("admin");
            }

            return string.Join(",", AccessRoles.Distinct());
        }
        internal enum Identifier
        {
            settings_permission
        }
    }
}