using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Api;
using DotNetNuke.Web.Api.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Vanjaro.Common.ASPNET.WebAPI;
using Vanjaro.Common.Engines.UIEngine;

namespace Vanjaro.UXManager.Extensions.Block.RegisterLink.Controllers
{
    [ValidateAntiForgeryToken]
    [AuthorizeAccessRoles(AccessRoles = "admin,anonymous")]
    public class RegisterLinkController : UIEngineController
    {
        internal static List<IUIData> GetData(string identifier, Dictionary<string, string> parameters, UserInfo userInfo, PortalSettings portalSettings)
        {
            Dictionary<string, IUIData> Settings = new Dictionary<string, IUIData>
            {
                { "ShowRegisterLink", new UIData { Name = "ShowRegisterLink", Value = "false" } },
                { "ShowAvatar", new UIData { Name = "ShowAvatar", Value = "false" } },
                { "ShowNotification", new UIData { Name = "ShowNotification", Value = "false" } },
                { "GlobalConfigs", new UIData { Name = "GlobalConfigs", Options = Core.Managers.BlockManager.GetGlobalConfigs(portalSettings, "register link") } },
                { "Global", new UIData { Name = "Global", Value = "false" } }
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