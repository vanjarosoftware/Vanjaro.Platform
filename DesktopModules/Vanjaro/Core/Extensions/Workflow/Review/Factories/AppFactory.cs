using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Permissions;
using System.Collections.Generic;
using System.Linq;
using Vanjaro.Common.Engines.UIEngine.AngularBootstrap;
using Vanjaro.Common.Entities.Apps;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Extensions.Workflow.Review.Factories
{
    public class AppFactory
    {
        private const string ModuleRuntimeVersion = "1.0.0";
        internal static List<AngularView> GetViews()
        {
            List<AngularView> Views = new List<AngularView>();

            AngularView Options = new AngularView
            {
                AccessRoles = "review,pageedit",
                UrlPaths = new List<string> {
                  "moderator",
                },
                IsDefaultTemplate = true,
                TemplatePath = "review/moderator.html",
                Identifier = Identifier.review_moderator.ToString(),
                Defaults = new Dictionary<string, string> {
                    { "ReviewID", "1" }
                }
            };
            Views.Add(Options);

            AngularView version = new AngularView
            {
                AccessRoles = "review,pageedit",
                UrlPaths = new List<string> {
                    "moderator",
                  "moderator/:version/:entity",
                },
                TemplatePath = "review/moderator.html",
                Identifier = Identifier.review_moderator.ToString(),
                Defaults = new Dictionary<string, string> {
                    { "ReviewID", "1" }
                }
            };
            Views.Add(version);

            AngularView Review = new AngularView
            {
                AccessRoles = "review",
                UrlPaths = new List<string> {
                  "review",
                  "review/:reviewtype"
                },
                TemplatePath = "review/review.html",
                Identifier = Identifier.review_review.ToString(),
            };
            Views.Add(Review);
            return Views;
        }

        internal static AppInformation GetAppInformation()
        {
            return new AppInformation(ReviewInfo.Name, ReviewInfo.FriendlyName, ReviewInfo.GUID, GetRuntimeVersion, "", "", 14, 7, new List<string> { "Domain", "Server" }, false);
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
                AccessRoles.Add("review");
            }

            if (UserInfo.UserID > -1 && (UserInfo.IsInRole("Administrators")))
            {
                AccessRoles.Add("admin");
                AccessRoles.Add("review");
            }

            if (TabPermissionController.HasTabPermission("EDIT"))
            {
                AccessRoles.Add("pageedit");
            }

            if (WorkflowManager.HasReviewPermission(UserInfo))
            {
                AccessRoles.Add("review");
            }

            return string.Join(",", AccessRoles.Distinct());
        }

        internal enum Identifier
        {
            review_moderator,
            review_review
        }

    }
}