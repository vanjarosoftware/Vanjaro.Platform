using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Core.Extensions.Workflow.Review.Managers;
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
            string Entity = string.Empty;
            int EntityID = 0;
            try
            {
                Version = int.Parse(Parameters["version"].ToString());
                Entity = Parameters["entity"].ToString();
                EntityID = int.Parse(Parameters["entityid"].ToString());
            }
            catch { }

            Dictionary<string, IUIData> Settings = ModeratorManager.GetData(PortalSettings, Version, Entity, EntityID);
            return Settings.Values.ToList();
        }

        [HttpPost]
        public dynamic addcomment(dynamic Data)
        {
            dynamic Result = new ExpandoObject();

            string Useraction = HttpContext.Current.Request.Form["Action"] != null ? HttpContext.Current.Request.Form["Action"] : Data.Action.ToString();
            string Comment = HttpContext.Current.Request.Form["Comment"] != null ? HttpContext.Current.Request.Form["Comment"] : Data.Comment.ToString();
            string Entity = HttpContext.Current.Request.Form["Entity"] != null ? HttpContext.Current.Request.Form["Entity"] : Data.Entity.ToString();
            string EntityID = HttpContext.Current.Request.Form["EntityID"] != null ? HttpContext.Current.Request.Form["EntityID"] : Data.EntityID.ToString();
            string Version = HttpContext.Current.Request.Form["Version"] != null ? HttpContext.Current.Request.Form["Version"] : Data.Version.ToString();

            if (!string.IsNullOrEmpty(Useraction) && !string.IsNullOrEmpty(Comment))
            {
                ModeratorManager.AddComment(Entity, int.Parse(EntityID), Useraction, Comment, PortalSettings);
            }

            Result.Data = ModeratorManager.GetData(PortalSettings, int.Parse(Version), Entity, int.Parse(EntityID));
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