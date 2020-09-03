using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;
using Vanjaro.Common.Utilities;
using Vanjaro.UXManager.Extensions.Menu.SiteGroups.Managers;

namespace Vanjaro.UXManager.Extensions.Menu.SiteGroups.Controllers
{
    [AuthorizeAccessRoles(AccessRoles = "host")]
    [ValidateAntiForgeryToken]
    public class SiteGroupsController : UIEngineController
    {
        internal static List<IUIData> GetData(int PortalID)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>();
            string SitesUrl = string.Empty;
            if (Library.Managers.MenuManager.GetURL().ToLower().Contains("guid=4553cd87-d95a-44e6-81f7-ba4e5c3fb654"))
                SitesUrl = ServiceProvider.NavigationManager.NavigateURL() + Library.Managers.MenuManager.GetURL().ToLower().Replace("guid=4553cd87-d95a-44e6-81f7-ba4e5c3fb654", "guid=a6c54290-79f7-4ae8-abae-5ad4cca3daf1").TrimEnd('&');
            else
                SitesUrl = ServiceProvider.NavigationManager.NavigateURL() + Library.Managers.MenuManager.GetURL() + "mid=0&icp=true&guid=a6c54290-79f7-4ae8-abae-5ad4cca3daf1";
            string SiteGroupUrl = ServiceProvider.NavigationManager.NavigateURL() + Library.Managers.MenuManager.GetURL() + "mid=0&icp=true&guid=4553cd87-d95a-44e6-81f7-ba4e5c3fb654";
            Settings.Add("SitesUrl", new UIData { Name = "SitesUrl", Value = SitesUrl });
            Settings.Add("SiteGroupUrl", new UIData { Name = "SiteGroupUrl", Value = SiteGroupUrl });
            Settings.Add("SiteGroups", new UIData { Name = "SiteGroups", Options = SiteGroupManager.SiteGroups() });
            return Settings.Values.ToList();
        }

        [HttpGet]
        public List<Components.PortalGroupInfo> Delete(int PortalGroupId)
        {
            SiteGroupManager.Delete(PortalGroupId);
            return SiteGroupManager.SiteGroups();
        }

        [HttpGet]
        public List<Components.PortalGroupInfo> GetAll()
        {
            return SiteGroupManager.SiteGroups();
        }

        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }
    }
}