using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Menu.Security.Factories
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
        public static List<AngularView> Views = new List<AngularView>();
        public static List<AngularView> GetViews()
        {
            AngularView setting = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "settings"
                },
                IsDefaultTemplate = true,
                TemplatePath = "security/settings.html",
                Identifier = Identifier.security_settings.ToString(),
                Defaults = new Dictionary<string, string> {
                    { "Picture_DefaultFolder","-1" },
                    { "Picture_MaxUploadSize",(Config.GetMaxUploadSize() / (1024 * 1024)).ToString() },
                    { "Picture_AllowableFileExtensions","jpg,jpeg,gif,png,svg,webp"},
                    { "Video_DefaultFolder","-1" },
                    { "Video_MaxUploadSize",(Config.GetMaxUploadSize() / (1024 * 1024)).ToString() },
                    { "Video_AllowableFileExtensions","webm,mp4" }
                }
            };
            Views.Add(setting);

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
            return new AppInformation("Security", "Security", ExtensionInfo.GUID, GetRuntimeVersion, "http://www.mandeeps.com/store", "http://www.mandeeps.com/Activation", 14, 7, new List<string> { "Domain", "Server" }, false);
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
            setting_advanced, security_settings
        }
    }
}