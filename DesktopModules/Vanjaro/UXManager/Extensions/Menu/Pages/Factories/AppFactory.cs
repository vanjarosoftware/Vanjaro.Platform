using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Menu.Pages.Factories
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
            AngularView setting_pages = new AngularView
            {
                AccessRoles = "admin,edit",
                UrlPaths = new List<string> {
                  "pages"
                },
                IsDefaultTemplate = true,
                TemplatePath = "setting/pages.html",
                Identifier = Identifier.setting_pages.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting_pages);

            AngularView setting_detail = new AngularView
            {
                AccessRoles = "admin,edit",
                UrlPaths = new List<string> {
                  "detail"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/detail.html",
                Identifier = Identifier.setting_detail.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting_detail);

            AngularView Edit_Page = new AngularView
            {
                AccessRoles = "admin,edit",
                UrlPaths = new List<string> {
                  "detail/:pid"
                },
                TemplatePath = "setting/detail.html",
                Identifier = Identifier.setting_detail.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(Edit_Page);

            AngularView CopyPage = new AngularView
            {
                AccessRoles = "admin,edit",
                UrlPaths = new List<string> {
                  "detail/:pid/:copy"
                },
                TemplatePath = "setting/detail.html",
                Identifier = Identifier.setting_detail.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(CopyPage);

            AngularView SaveTemplateAs = new AngularView
            {
                AccessRoles = "admin,edit",
                UrlPaths = new List<string> {
                  "savetemplateas/:pid"
                },
                TemplatePath = "setting/savetemplateas.html",
                Identifier = Identifier.setting_savetemplateas.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(SaveTemplateAs);

            AngularView setting_permissions = new AngularView
            {
                AccessRoles = "admin,edit",
                UrlPaths = new List<string> {
                  "permissions"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/permissions.html",
                Identifier = Identifier.setting_permissions.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting_permissions);

            AngularView setting_advanced = new AngularView
            {
                AccessRoles = "admin,edit",
                UrlPaths = new List<string> {
                  "advanced"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/advanced.html",
                Identifier = Identifier.setting_advanced.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting_advanced);

            AngularView setting_recyclebin = new AngularView
            {
                AccessRoles = "admin,edit",
                UrlPaths = new List<string> {
                  "recyclebin"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/recyclebin.html",
                Identifier = Identifier.setting_recyclebin.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(setting_recyclebin);

            #region Choose Template when migrate the page
            AngularView ChooseTemplate = new AngularView
            {
                AccessRoles = "admin,edit",
                UrlPaths = new List<string> {
                  "choosetemplate",
                  "choosetemplate/:pid"
                },
                TemplatePath = "setting/choosetemplate.html",
                Identifier = Identifier.setting_choosetemplate.ToString(),
                IsDefaultTemplate = false,
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(ChooseTemplate);
            #endregion


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

            if (TabPermissionController.HasTabPermission("EDIT"))
            {
                AccessRoles.Add("edit");
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
            setting_detail, setting_recyclebin, setting_permissions, setting_advanced, setting_pages, common_licensing, common_activation, setting_savetemplateas, setting_choosetemplate
        }
    }
}