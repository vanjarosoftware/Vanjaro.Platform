using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Menu.Users.Factories
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
            AngularView users = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "users"
                },
                IsDefaultTemplate = true,
                TemplatePath = "setting/users.html",
                Identifier = Identifier.setting_users.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(users);

            AngularView setting = new AngularView
            {
                AccessRoles = "user",
                UrlPaths = new List<string> {
                  "setting"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/setting.html",
                Identifier = Identifier.setting_setting.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting);

            AngularView edit = new AngularView
            {
                AccessRoles = "user",
                UrlPaths = new List<string> {
                  "setting/:uid"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/setting.html",
                Identifier = Identifier.setting_setting.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(edit);

            AngularView add = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "adduser"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/adduser.html",
                Identifier = Identifier.setting_adduser.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(add);

            AngularView manage = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "changepassword"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/changepassword.html",
                Identifier = Identifier.setting_changepassword.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(manage);

            AngularView change = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "changepassword/:uid"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/changepassword.html",
                Identifier = Identifier.setting_changepassword.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(change);

            AngularView profile = new AngularView
            {
                AccessRoles = "user,anonymous",
                UrlPaths = new List<string> {
                  "updateprofile"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/updateprofile.html",
                Identifier = Identifier.setting_updateprofile.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(profile);

            AngularView updateprofile = new AngularView
            {
                AccessRoles = "user,anonymous",
                UrlPaths = new List<string> {
                  "updateprofile/:uid"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/updateprofile.html",
                Identifier = Identifier.setting_updateprofile.ToString(),
                Defaults = new Dictionary<string, string> { }
            };

            Views.Add(updateprofile);

            AngularView import = new AngularView
            {
                AccessRoles = "user",
                UrlPaths = new List<string> {
                  "import"
                },
                TemplatePath = "setting/import.html",
                Identifier = Identifier.setting_import.ToString(),
                Defaults = new Dictionary<string, string> { }
            };

            Views.Add(import);

            Views.Add(
                new AngularView
                {
                    Identifier = Identifier.common_controls_url.ToString(),
                    IsCommon = true,
                    UrlPaths = new List<string>() { "common/controls/url/:uid" },
                    TemplatePath = "common/controls/url.html",
                    AccessRoles = "user",
                    Defaults = new Dictionary<string, string>()
                }
            );

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
            return new AppInformation(ExtensionInfo.Name, ExtensionInfo.FriendlyName, ExtensionInfo.GUID, GetRuntimeVersion, "", "", 14, 7, new List<string> { "Domain", "Server" }, false);
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
            setting_setting, setting_adduser, setting_users, setting_changepassword, setting_updateprofile, setting_import, common_controls_url
        }
    }
}