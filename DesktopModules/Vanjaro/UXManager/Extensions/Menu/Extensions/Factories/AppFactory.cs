using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Menu.Extensions.Factories
{
    public class AppFactory
    {
        private const string ModuleRuntimeVersion = "1.0.0";
        internal static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>();

            AngularView Options = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "dashboard",
                },
                IsDefaultTemplate = true,
                TemplatePath = "settings/dashboard.html",
                Identifier = Identifier.settings_dashboard.ToString(),
                Defaults = new Dictionary<string, string>()
            };
            Views.Add(Options);

            AngularView install = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "install",
                  "install/type/:type/name/:name",
                },
                TemplatePath = "settings/installextension.html",
                Identifier = Identifier.settings_installextension.ToString(),
                Defaults = new Dictionary<string, string>()
            };
            Views.Add(install);

            AngularView editext = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "edit/pid/:pid",
                },
                TemplatePath = "settings/editextension.html",
                Identifier = Identifier.settings_editextension.ToString(),
                Defaults = new Dictionary<string, string>()
            };
            Views.Add(editext);

            AngularView deleteext = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "delete/pid/:pid",
                },
                TemplatePath = "settings/deleteextension.html",
                Identifier = Identifier.settings_deleteextension.ToString(),
                Defaults = new Dictionary<string, string>()
            };
            Views.Add(deleteext);

            AngularView usageext = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "usage/pid/:pid",
                },
                TemplatePath = "settings/usageextension.html",
                Identifier = Identifier.settings_usageextension.ToString(),
                Defaults = new Dictionary<string, string>()
            };
            Views.Add(usageext);

            AngularView installpack = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "installpackage",
                },
                TemplatePath = "settings/installpackage.html",
                Identifier = Identifier.settings_installpackage.ToString(),
                Defaults = new Dictionary<string, string>()
            };
            Views.Add(installpack);

            return Views;
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
            AccessRoles.Add("permissiongrid");
            if (UserInfo.IsSuperUser)
            {
                AccessRoles.Add("host");
            }

            return string.Join(",", AccessRoles.Distinct());
        }

        internal enum Identifier
        {
            settings_dashboard, settings_installextension, settings_editextension, settings_deleteextension, common_controls_url, common_controls_editorconfig, settings_usageextension, settings_installpackage
        }
    }
}