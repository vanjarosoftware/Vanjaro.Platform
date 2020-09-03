using DotNetNuke.Entities.Tabs;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Components;
using Vanjaro.Common.Factories;

namespace Vanjaro.Common.Engines.UIEngine.AngularBootstrap.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "user")]
    public class PageLinkController : WebApiController
    {
        [ValidateAntiForgeryToken]
        [HttpGet]
        public List<ListItem> GetPageAnchors(int pageid)
        {
            return BrowseUploadFactory.GetPageAnchors(TabController.Instance.GetTab(pageid, ActiveModule.PortalID));
        }

        public override string AccessRoles()
        {
            if (UserInfo.UserID > 0)
            {
                return "user";
            }
            else
            {
                return "";
            }
        }
    }
}