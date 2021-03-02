using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Apps.ThemeBuilder.Factories
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
            AngularView Default = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "categories"
                },
                IsDefaultTemplate = true,
                TemplatePath = "setting/categories.html",
                Identifier = Identifier.setting_categories.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(Default);

            AngularView DefaultWithTheme = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "categories/:themename"
                },
                TemplatePath = "setting/categories.html",
                Identifier = Identifier.setting_categories.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(DefaultWithTheme);

            AngularView Settings = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "settings/:themename/:cat/:guid"
                },
                TemplatePath = "setting/settings.html",
                Identifier = Identifier.setting_settings.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(Settings);

            AngularView Manage = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "manage/:guid"
                },
                TemplatePath = "setting/manage.html",
                Identifier = Identifier.setting_manage.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(Manage);

            AngularView New = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "edit/:catguid"
                },
                TemplatePath = "setting/edit.html",
                Identifier = Identifier.setting_edit.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(New);

            AngularView Edit = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "edit/:catguid/:guid",
                  "edit/:catguid/:cat/:type"
                },
                TemplatePath = "setting/edit.html",
                Identifier = Identifier.setting_edit.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(Edit);

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
            return new AppInformation(ThemeBuilderInfo.Name, ThemeBuilderInfo.FriendlyName, ThemeBuilderInfo.GUID, GetRuntimeVersion, "http://www.mandeeps.com/store", "http://www.mandeeps.com/Activation", 14, 7, new List<string> { "Domain", "Server" }, false);
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
            setting_settings, setting_edit, setting_manage, setting_categories
        }
    }
}