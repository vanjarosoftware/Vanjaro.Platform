using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Menu.Languages.Factories
{
    public class AppFactory
    {
        private const string ModuleRuntimeVersion = "1.0.0";

        internal static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>();
            AngularView languages = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "languages"
                },
                IsDefaultTemplate = true,
                TemplatePath = "setting/languages.html",
                Identifier = Identifier.setting_languages.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(languages);

            AngularView add = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "add"
                },
                TemplatePath = "setting/add.html",
                Identifier = Identifier.setting_add.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(add);

            AngularView translator = new AngularView
            {
                AccessRoles = "host",
                UrlPaths = new List<string> {
                  "translator/lid/:lid"
                },
                TemplatePath = "setting/translator.html",
                Identifier = Identifier.setting_translator.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(translator);

            AngularView resources = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "resources/lid/:lid"
                },
                TemplatePath = "setting/resources.html",
                Identifier = Identifier.setting_resources.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(resources);

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
            setting_languages, setting_add, setting_translator, setting_resources
        }
    }
}