using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Utilities;
using Vanjaro.Core.Extensions.Workflow.Review.Managers;
using static Vanjaro.Core.Managers;

namespace Vanjaro.Core.Extensions.Workflow.Review.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "review")]
    public class ReviewController : UIEngineController
    {
        internal static List<IUIData> GetData(UserInfo UserInfo)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "States", new UIData { Name = "States", Options = ReviewManager.GetStatesforReview(UserInfo.PortalID, UserInfo.UserID), OptionsText = "Text", OptionsValue = "Value", Value = "0" } },
                { "NotificationExtensionURL", new UIData { Name = "NotificationExtensionURL", Value = ServiceProvider.NavigationManager.NavigateURL("", "mid=0", "icp=true", "guid=cd8c127f-da66-4036-b107-90061361cf87") } }
            };

            return Settings.Values.ToList();
        }

        [HttpGet]
        public dynamic GetPages(int skip, int pagesize, int StateID)
        {
            skip = (skip / pagesize) + 1;
            int TotalCount = WorkflowManager.GetReviewPagesCountByUserID(PortalSettings.UserId, skip, pagesize, StateID);
            double NumberOfPages = (double)TotalCount / pagesize;
            dynamic result = new ExpandoObject();

            if ((int)NumberOfPages > 0)
            {
                NumberOfPages = Math.Ceiling(NumberOfPages);
            }

            result.NumberOfPages = NumberOfPages;
            result.Data = WorkflowManager.GetReviewPagesbyUserID(PortalSettings.UserId, skip, pagesize, StateID);
            return result;
        }



        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}