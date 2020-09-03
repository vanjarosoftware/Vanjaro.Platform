using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Toolbar.VersionManagement.Factories
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

        internal static string SharedResourcePath()
        {
            return VirtualPathUtility.ToAbsolute("~/DesktopModules/Vanjaro/UXManager/Extensions/Toolbar/VersionManagement/Views/App_LocalResources/Shared.resx");
        }

        public static List<AngularView> Views = new List<AngularView>();

        public static List<AngularView> GetViews()
        {
            AngularView revisions = new AngularView
            {
                AccessRoles = "editpage",
                UrlPaths = new List<string> {
                  "revisions"
                },
                IsDefaultTemplate = true,
                TemplatePath = "history/revisions.html",
                Identifier = Identifier.history_revisions.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(revisions);

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

            if (TabPermissionController.CanManagePage(PortalSettings.Current.ActiveTab))
            {
                AccessRoles.Add("editpage");
            }

            return string.Join(",", AccessRoles);
        }

        public static AppInformation GetAppInformation()
        {
            return new AppInformation("VersionManagement", "VersionManagement", ExtensionInfo.GUID, GetRuntimeVersion, "http://www.mandeeps.com/store", "http://www.mandeeps.com/Activation", 14, 7, new List<string> { "Domain", "Server" }, false);
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
            setting_setting, history_revisions
        }
    }
}