using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Menu.Assets.Factories
{
    public class AppFactory
    {
        private const string ModuleRuntimeVersion = "1.0.0";
        internal static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>();

            AngularView setting_assets = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "assets",
                },
                IsDefaultTemplate = true,
                TemplatePath = "setting/assets.html",
                Identifier = Identifier.setting_assets.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting_assets);

            AngularView setting_permission = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "permission/:pid",
                },
                TemplatePath = "setting/permission.html",
                Identifier = Identifier.setting_permission.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting_permission);

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
            setting_assets, setting_permission, common_controls_url, common_controls_editorconfig
        }
    }
}