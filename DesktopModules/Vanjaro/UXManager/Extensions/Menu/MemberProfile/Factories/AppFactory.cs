using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Menu.MemberProfile.Factories
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
            AngularView memberprofile_memberprofile = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "memberprofile"
                },
                IsDefaultTemplate = true,
                TemplatePath = "MemberProfile/memberprofile.html",
                Identifier = Identifier.memberprofile_memberprofile.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(memberprofile_memberprofile);

            AngularView Addmemberprofile = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "memberprofilesettings"
                },
                IsDefaultTemplate = false,
                TemplatePath = "memberprofile/memberprofilesettings.html",
                Identifier = Identifier.memberprofile_memberprofilesettings.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(Addmemberprofile);

            AngularView Updatememberprofile = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "memberprofilesettings/:mpid"
                },
                IsDefaultTemplate = false,
                TemplatePath = "memberprofile/memberprofilesettings.html",
                Identifier = Identifier.memberprofile_memberprofilesettings.ToString(),
                Defaults = new Dictionary<string, string> { }
            };
            Views.Add(Updatememberprofile);

            AngularView setting = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "settings"
                },
                IsDefaultTemplate = false,
                TemplatePath = "memberprofile/settings.html",
                Identifier = Identifier.memberprofile_settings.ToString(),
                Defaults = new Dictionary<string, string> { }
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
            common_licensing, common_activation, memberprofile_settings, memberprofile_memberprofile, memberprofile_memberprofilesettings
        }
    }
}