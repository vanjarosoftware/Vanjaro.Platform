using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Library.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "user,anonymous")]
    public class AnalyticsController : UIEngineController
    {
        [HttpPost]
        public ActionResult SetClientKey()
        {
            ActionResult actionResult = new ActionResult();
            if (HttpContext.Current.Request.Headers["client_id"] != null)
            {
                Core.Managers.CookieManager.AddValue("vj_AnalyticsClientID", HttpContext.Current.Request.Headers["client_id"], DateTime.UtcNow.AddMinutes(30));
                Core.Managers.AnalyticsManager.AnalyticsPost();
            }
            return actionResult;
        }

        public override string AccessRoles()
        {
            List<string> AccessRoles = new List<string>();
            if (UserInfo.UserID > 0)
                AccessRoles.Add("user");
            else
                AccessRoles.Add("anonymous");
            return string.Join(",", AccessRoles.Distinct());
        }
    }
}