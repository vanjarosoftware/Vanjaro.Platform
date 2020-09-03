using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.Sites.Managers;
using Vanjaro.UXManager.Library.Common;

namespace Vanjaro.UXManager.Extensions.Menu.Sites.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "host")]
    [ValidateAntiForgeryToken]
    public class SitesController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string SiteGroupUrl = string.Empty;
            if (Library.Managers.MenuManager.GetURL().ToLower().Contains("guid=a6c54290-79f7-4ae8-abae-5ad4cca3daf1"))
                SiteGroupUrl = ServiceProvider.NavigationManager.NavigateURL() + Library.Managers.MenuManager.GetURL().ToLower().Replace("guid=a6c54290-79f7-4ae8-abae-5ad4cca3daf1", "guid=4553cd87-d95a-44e6-81f7-ba4e5c3fb654").TrimEnd('&');
            else
                SiteGroupUrl = ServiceProvider.NavigationManager.NavigateURL() + Library.Managers.MenuManager.GetURL() + "mid=0&icp=true&guid=4553cd87-d95a-44e6-81f7-ba4e5c3fb654";

            Settings.Add("SiteGroupUrl", new UIData { Name = "SiteGroupUrl", Value = SiteGroupUrl });
            return Settings.Values.ToList();
        }

        [HttpGet]
        public ActionResult GetPortals(int pageSize, int pageIndex)
        {
            return SitesManager.GetAllPortals(PortalSettings.PortalId, string.Empty, pageIndex, pageSize);
        }

        [HttpGet]
        public ActionResult Delete(int PortalID)
        {
            return SitesManager.DeletePortal(PortalID, PortalSettings);
        }

        [HttpGet]
        public HttpResponseMessage Export(int PortalID, string Name)
        {
            return SitesManager.Export(PortalID, Name);
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}