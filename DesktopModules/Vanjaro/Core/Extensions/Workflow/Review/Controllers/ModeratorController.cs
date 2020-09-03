using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Extensions.Workflow.Review.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "review,pageedit")]
    public class ModeratorController : UIEngineController
    {
        internal static List<IUIData> GetData(UserInfo UserInfo, PortalSettings PortalSettings, Dictionary<string, string> Parameters)
        {
            int Version = 0;
            try
            {
                Version = int.Parse(Parameters["version"].ToString());
            }
            catch { }

            Dictionary<string, IUIData> Settings = Managers.ModeratorManager.GetData(PortalSettings, Version);
            return Settings.Values.ToList();
        }

        [HttpPost]
        public dynamic addcomment(dynamic Data)
        {
            dynamic Result = new ExpandoObject();

            string Useraction = HttpContext.Current.Request.Form["Action"] != null ? HttpContext.Current.Request.Form["Action"] : Data.Action.ToString();
            string Comment = HttpContext.Current.Request.Form["Action"] != null ? HttpContext.Current.Request.Form["Comment"] : Data.Comment.ToString();
            if (!string.IsNullOrEmpty(Useraction) && !string.IsNullOrEmpty(Comment))
            {
                WorkflowManager.AddComment(PortalSettings, Useraction, Comment);
            }

            Result.Data = Managers.ModeratorManager.GetData(PortalSettings, 0);
            Result.NotifyCount = NotificationManager.RenderNotificationsCount(PortalSettings.PortalId);
            Result.ReviewVariable = PageManager.GetPageReviewSettings(PortalSettings);
            return Result;
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}