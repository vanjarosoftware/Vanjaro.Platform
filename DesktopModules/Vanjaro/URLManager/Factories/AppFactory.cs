using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Vanjaro.URL.Factories
{
    internal class AppFactory
    {
        private const string ModuleRuntimeVersion = "1.0.0";

        internal static string GetAccessRoles(ModuleInfo ModuleInfo, UserInfo UserInfo)
        {
            List<string> AccessRoles = new List<string>();

            if (UserInfo.UserID > 0)
                AccessRoles.Add("user");
            else
                AccessRoles.Add("anonymous");

            //Admin / Superuser / Edit Permission
            if (UserInfo.UserID > -1 && (UserInfo.IsSuperUser || UserInfo.IsInRole("Administrators") || ModulePermissionController.CanEditModuleContent(ModuleInfo)))
                AccessRoles.Add("admin");

            if (UserInfo.IsSuperUser)
                AccessRoles.Add("host");

            return string.Join(",", AccessRoles);
        }

        internal static bool IsAuthorized(string Roles, ModuleInfo ModuleInfo, UserInfo UserInfo)
        {
            bool result = false;
            string[] AccessRoles = GetAccessRoles(ModuleInfo, UserInfo).Split(',');
            if (AccessRoles.Length > 0 && !string.IsNullOrEmpty(Roles))
            {
                string[] NewRoles = Roles.Split(',');
                foreach (string r in NewRoles)
                {
                    if (AccessRoles.Contains(r))
                        result = true;
                }
            }
            return result;
        }

        internal static string GetAllowedRoles(string Identifier)
        {
            var template = GetViews().Where(t => t.TemplatePath.StartsWith(Identifier.Replace("_", "/"))).FirstOrDefault();

            if (template != null)
                return template.AccessRoles;

            return string.Empty;
        }

        internal static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>();

            AngularView Default = new AngularView
            {
                AccessRoles = "user",
                UrlPaths = new List<string> {
                  "url/manage"
                },
                IsDefaultTemplate = true,
                TemplatePath = "url/manage.html",
                Identifier = Identifier.url_manage.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(Default);

            AngularView manage = new AngularView
            {
                AccessRoles = "user",
                UrlPaths = new List<string> {
                  "url/manage/:ename/:eid"
                },
                IsDefaultTemplate = false,
                TemplatePath = "url/manage.html",
                Identifier = Identifier.url_manage.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(manage);

            return Views;
        }
        internal static AppInformation GetAppInformation()
        {
            return new AppInformation("URLManager", "E069BD62-2DEC-483F-B17E-27B6AA07FBB0", GetRuntimeVersion, "http://www.mandeeps.com/store.aspx", "http://www.mandeeps.com/Activation.aspx", 14, 7, new List<string> { "Domain" }, false);
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

        internal enum Identifier
        {
            url_manage
        }

    }
}
