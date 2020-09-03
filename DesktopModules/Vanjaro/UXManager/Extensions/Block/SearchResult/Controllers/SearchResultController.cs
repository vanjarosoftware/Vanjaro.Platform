using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.UXManager.Extensions.Block.SearchResult.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin,anonymous")]
    public class SearchResultController : UIEngineController
    {
        internal static List<IUIData> GetData(UserInfo userInfo, string identifier, Dictionary<string, string> parameters, PortalSettings portalSettings)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "Global", new UIData { Name = "Global", Value = "true" } },
                { "LinkTarget", new UIData { Name = "LinkTarget", Value = "true" } },
                { "EnableWildSearch", new UIData { Name = "EnableWildSearch", Value = "true" } },
                { "GlobalConfigs", new UIData { Name = "GlobalConfigs", Options = Core.Managers.BlockManager.GetGlobalConfigs(portalSettings, "search result") } },
                { "IsAdmin", new UIData { Name = "IsAdmin", Value = userInfo.IsInRole("Administrators").ToString().ToLower() } }
            };
            return Settings.Values.ToList();
        }

        [HttpPost]
        [DnnPageEditor]
        [AuthorizeAccessRoles(AccessRoles = "admin")]
        public void Update(Dictionary<string, string> Attributes)
        {
            Core.Managers.BlockManager.UpdateDesignElement(PortalSettings, Attributes);
        }
        public override string AccessRoles()
        {
            return Factories.AppFactory.GetAccessRoles(UserInfo);
        }

    }
}