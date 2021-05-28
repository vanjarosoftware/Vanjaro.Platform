using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Menu.Sites.Factories
{
    public class AppFactory
    {
        private const string ModuleRuntimeVersion = "1.0.0";

        internal static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>();
            AngularView sites = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "sites"
                },
                IsDefaultTemplate = true,
                TemplatePath = "setting/sites.html",
                Identifier = Identifier.setting_sites.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(sites);

            AngularView addSite = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "add"
                },
                TemplatePath = "setting/add.html",
                Identifier = Identifier.setting_add.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(addSite);


            AngularView choosetemplate = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "choosetemplate"
                },
                TemplatePath = "setting/choosetemplate.html",
                Identifier = Identifier.setting_choosetemplate.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(choosetemplate);

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

            if (UserInfo.IsSuperUser)
            {
                AccessRoles.Add("host");
            }

            return string.Join(",", AccessRoles.Distinct());
        }

        internal enum Identifier
        {
            setting_sites, setting_add, setting_choosetemplate
        }
    }
}