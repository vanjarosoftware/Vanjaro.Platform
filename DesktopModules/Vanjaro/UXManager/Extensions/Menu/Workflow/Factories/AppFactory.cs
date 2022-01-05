using DotNetNuke.Entities.Users;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;

namespace Vanjaro.UXManager.Extensions.Menu.Workflow.Factories
{
    public class AppFactory
    {
        private const string ModuleRuntimeVersion = "1.0.0";
        internal static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>();

            AngularView Options = new AngularView
            {
                AccessRoles = "admin",
                UrlPaths = new List<string> {
                  "workflow",
                },
                IsDefaultTemplate = true,
                TemplatePath = "setting/workflow.html",
                Identifier = Identifier.setting_workflow.ToString(),
                Defaults = new Dictionary<string, string> {
                    { "WorkflowID", "1" }
                }
            };

            Views.Add(Options);
            return Views;
        }

        internal static AppInformation GetAppInformation()
        {
            return new AppInformation(WorkflowInfo.Name, WorkflowInfo.FriendlyName, WorkflowInfo.GUID, GetRuntimeVersion, "", "", 14, 7, new List<string> { "Domain", "Server" }, false);
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
            common_controls_url, common_controls_editorconfig,
            setting_workflow, manage_workflow
        }
        public static string SharedResourceFile()
        {
            return VirtualPathUtility.ToAbsolute("~/DesktopModules/Vanjaro/UXManager/Extensions/Menu/Workflow/Views/App_LocalResources/Shared.resx");
        }
    }
}