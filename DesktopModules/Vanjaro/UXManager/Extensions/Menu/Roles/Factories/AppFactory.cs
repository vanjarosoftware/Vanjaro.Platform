using DotNetNuke.Entities.Users;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Menu.Roles.Factories
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
            AngularView roles = new AngularView
            {
                AccessRoles = "user,anonymous",
                UrlPaths = new List<string> {
                  "roles"
                },
                IsDefaultTemplate = true,
                TemplatePath = "setting/roles.html",
                Identifier = Identifier.setting_roles.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(roles);

            AngularView add = new AngularView
            {
                AccessRoles = "user,anonymous",
                UrlPaths = new List<string> {
                  "add"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/add.html",
                Identifier = Identifier.setting_add.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(add);

            AngularView edit = new AngularView
            {
                AccessRoles = "user,anonymous",
                UrlPaths = new List<string> {
                  "add/:rid"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/add.html",
                Identifier = Identifier.setting_add.ToString(),
                Defaults = new Dictionary<string, string> { }
            };

            Views.Add(edit);

            AngularView AddGroup = new AngularView
            {
                AccessRoles = "user,anonymous",
                UrlPaths = new List<string> {
                  "addgroup"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/addgroup.html",
                Identifier = Identifier.setting_addgroup.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(AddGroup);

            AngularView editGroup = new AngularView
            {
                AccessRoles = "user,anonymous",
                UrlPaths = new List<string> {
                  "addgroup/:gid"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/addgroup.html",
                Identifier = Identifier.setting_addgroup.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(editGroup);

            AngularView AddUser = new AngularView
            {
                AccessRoles = "user,anonymous",
                UrlPaths = new List<string> {
                  "adduser"
                },
                IsDefaultTemplate = false,
                TemplatePath = "setting/adduser.html",
                Identifier = Identifier.setting_adduser.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(AddUser);

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
            setting_roles, setting_addgroup, setting_add, setting_adduser
        }

        public static dynamic GetLocalizedEnumOption(Type EnumType)
        {
            Array data = System.Enum.GetNames(EnumType);
            List<dynamic> list = new List<dynamic>();
            dynamic item = null;
            foreach (string name in data)
            {
                int value = (int)Enum.Parse(EnumType, name);
                item = new ExpandoObject();
                item.Key = name;
                item.Value = value;
                list.Add(item);
            }
            return list;
        }

    }
}