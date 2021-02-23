using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using Vanjaro.UXManager.Library.Entities;

namespace Vanjaro.UXManager.Extensions.Apps.Video.Factories
{
    public class AppFactory
    {
        private const string ModuleRuntimeVersion = "1.0.0";

        public AppInformation AppInformation => GetAppInformation();

        internal static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>();
            AngularView Options = new AngularView
            {
                AccessRoles = "editpage",
                UrlPaths = new List<string> {
                  "video",
                },
                IsDefaultTemplate = true,
                TemplatePath = "settings/video.html",
                Identifier = Identifier.settings_video.ToString(),
                Defaults = new Dictionary<string, string>()
            };
            Views.Add(Options);

            AngularView VideoOnline = new AngularView
            {
                AccessRoles = "editpage",
                UrlPaths = new List<string> {
                  "videoonline",
                },
                TemplatePath = "settings/videoonline.html",
                Identifier = Identifier.settings_videoonline.ToString(),
                Defaults = new Dictionary<string, string>()
            };

            Views.Add(VideoOnline);

            return Views;
        }

        internal static AppInformation GetAppInformation()
        {
            return new AppInformation(VideoInfo.Name, VideoInfo.FriendlyName, VideoInfo.GUID, GetRuntimeVersion, "http://www.mandeeps.com/store", "http://www.mandeeps.com/Activation", 14, 7, new List<string> { "Domain", "Server" }, false);
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
                AccessRoles.Add("editpage");
            }

            if (TabPermissionController.HasTabPermission("EDIT") || Core.Entities.Editor.HasExtensionAccess())
            {
                AccessRoles.Add("editpage");
            }

            return string.Join(",", AccessRoles.Distinct());
        }

        internal enum Identifier
        {
            settings_video,
            settings_videoonline
        }
    }
}